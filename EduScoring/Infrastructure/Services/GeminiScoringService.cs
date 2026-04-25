using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using EduScoring.Features.Submissions.Services;

namespace EduScoring.Infrastructure.Services;

public class GeminiScoringService : IAiScoringService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiScoringService> _logger;

    // IConfiguration không cần inject trực tiếp — dùng IOptions<GeminiOptions> sau này
    // Tạm thời HttpClient được cấu hình sẵn BaseAddress + ApiKey qua IHttpClientFactory
    public GeminiScoringService(HttpClient httpClient, ILogger<GeminiScoringService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AiEvaluationResult> EvaluateAsync(string studentAnswer, string rubricJson, string language)
    {
        // ── 1. Validate input trước khi gọi API ──────────────────────────────
        if (string.IsNullOrWhiteSpace(studentAnswer))
            throw new ArgumentException("[GeminiService] studentAnswer không được rỗng.");
        if (string.IsNullOrWhiteSpace(rubricJson))
            throw new ArgumentException("[GeminiService] rubricJson không được rỗng.");

        // Parse rubric để dùng validate sau
        List<RubricItem>? rubricItems = null;
        try
        {
            rubricItems = JsonSerializer.Deserialize<List<RubricItem>>(rubricJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[GeminiService] Không parse được rubricJson — bỏ qua validate MaxScore.");
        }

        // ── 2. Build prompt ───────────────────────────────────────────────────
        var systemPrompt = $@"You are a strict and highly accurate academic examiner.
        Evaluate the student's answer based ONLY on the provided JSON rubric.
        The student's language is: {language}. Provide 'Reasoning' and 'OverallFeedback' in this language.

        CRITICAL INSTRUCTIONS:
        - You MUST output ONLY a raw, valid JSON object. NO markdown, NO explanation outside JSON.
        - DO NOT wrap in ```json ... ```.
        - Score for each criterion MUST NOT exceed its MaxScore.
        - Use EXACTLY these property names (case-sensitive):
        {{
            ""TotalScore"": (number),
            ""CriteriaScores"": [
                {{
                    ""CriteriaName"": (string),
                    ""Score"": (number),
                    ""MaxScore"": (number),
                    ""Reasoning"": (string)
                }}
            ],
            ""OverallFeedback"": (string),
            ""ConfidenceScore"": (number between 0.0 and 1.0)
        }}";

        var userPrompt = $"RUBRIC:\n{rubricJson}\n\nSTUDENT ANSWER:\n{studentAnswer}";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = $"{systemPrompt}\n\n{userPrompt}" } } }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                temperature = 0.1
            }
        };

        // ── 3. Gọi API ────────────────────────────────────────────────────────
        _logger.LogInformation("[GeminiService] Gọi Gemini API...");
        var response = await _httpClient.PostAsJsonAsync(string.Empty, payload);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        // ── 4. Extract text từ Gemini response structure ──────────────────────
        using var doc = JsonDocument.Parse(responseString);
        var textResult = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;

        // ── 5. Strip markdown nếu AI không tuân lệnh ─────────────────────────
        textResult = textResult.Trim();
        if (textResult.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            textResult = textResult[7..];
        if (textResult.StartsWith("```"))
            textResult = textResult[3..];
        if (textResult.EndsWith("```"))
            textResult = textResult[..^3];
        textResult = textResult.Trim();

        _logger.LogWarning("[GeminiService][RAW RESPONSE]\n{Text}", textResult);

        // ── 6. Deserialize ────────────────────────────────────────────────────
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };

        AiEvaluationResult result;
        try
        {
            result = JsonSerializer.Deserialize<AiEvaluationResult>(textResult, deserializeOptions)
                ?? throw new InvalidOperationException("Gemini trả về null sau khi deserialize.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[GeminiService] Deserialize thất bại. Raw text:\n{Text}", textResult);
            throw new InvalidOperationException($"Gemini trả về JSON không hợp lệ: {ex.Message}", ex);
        }

        // ── 7. Validate và clamp Score không vượt MaxScore ────────────────────
        if (result.CriteriaScores is { Count: > 0 })
        {
            var clampedCriteria = result.CriteriaScores.Select(c =>
            {
                if (c.Score > c.MaxScore)
                {
                    _logger.LogWarning(
                        "[GeminiService] Criteria '{Name}': Score={Score} vượt MaxScore={Max} — Clamp.",
                        c.CriteriaName, c.Score, c.MaxScore);
                    return c with { Score = c.MaxScore };
                }
                if (c.Score < 0)
                {
                    _logger.LogWarning(
                        "[GeminiService] Criteria '{Name}': Score={Score} âm — Clamp về 0.",
                        c.CriteriaName, c.Score);
                    return c with { Score = 0 };
                }
                return c;
            }).ToList();

            // Tính lại TotalScore từ criteria đã clamp (không tin TotalScore AI tự tính)
            var recalculatedTotal = clampedCriteria.Sum(c => c.Score);

            // Validate TotalScore với rubric gốc nếu có
            if (rubricItems is { Count: > 0 })
            {
                var maxAllowed = (decimal)rubricItems.Sum(r => r.MaxScore);
                if (recalculatedTotal > maxAllowed)
                {
                    _logger.LogWarning(
                        "[GeminiService] TotalScore={Score} vượt tổng MaxScore={Max} — Clamp.",
                        recalculatedTotal, maxAllowed);
                    recalculatedTotal = maxAllowed;
                }
            }

            result = result with
            {
                CriteriaScores = clampedCriteria,
                TotalScore = recalculatedTotal
            };
        }

        // ── 8. Validate ConfidenceScore trong [0, 1] ──────────────────────────
        if (result.ConfidenceScore < 0 || result.ConfidenceScore > 1)
        {
            _logger.LogWarning("[GeminiService] ConfidenceScore={Score} ngoài [0,1] — Clamp.", result.ConfidenceScore);
            result = result with { ConfidenceScore = Math.Clamp(result.ConfidenceScore, 0, 1) };
        }

        _logger.LogInformation("[GeminiService] Chấm xong — TotalScore={Score} | Confidence={Conf}",
            result.TotalScore, result.ConfidenceScore);

        return result;
    }

    // Helper record để parse rubric JSON cho validation — sealed vì không kế thừa
    private sealed record RubricItem(string CriteriaName, double MaxScore, string Description);
}