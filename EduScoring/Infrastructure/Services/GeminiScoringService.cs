using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using EduScoring.Features.Submissions.Services;
using Microsoft.Extensions.Logging;

namespace EduScoring.Infrastructure.Services;

public class GeminiScoringService : IAiScoringService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiScoringService> _logger;

    // Bơm thêm ILogger vào để in ra Console
    public GeminiScoringService(HttpClient httpClient, IConfiguration config, ILogger<GeminiScoringService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<AiEvaluationResult> EvaluateAsync(string studentAnswer, string rubricJson, string language)
    {
        var apiKey = _config["AiSettings:ApiKey"];
        var model = _config["AiSettings:Model"] ?? "gemini-1.5-flash";
        var url = $"[https://generativelanguage.googleapis.com/v1beta/models/](https://generativelanguage.googleapis.com/v1beta/models/){model}:generateContent?key={apiKey}";

        // Ép AI dùng đúng tên thuộc tính (Key) của sếp
        var systemPrompt = $@"You are a strict and highly accurate academic examiner.
            Evaluate the student's answer based ONLY on the provided JSON rubric.
            The student's language is: {language}. Provide the 'Reasoning' and 'OverallFeedback' in this language.

            CRITICAL INSTRUCTIONS:
            - You MUST output ONLY a raw, valid JSON object.
            - DO NOT wrap the response in markdown blocks like ```json ... ```.
            - You MUST use EXACTLY these property names (case-sensitive):
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
                ""ConfidenceScore"": (number)
            }}";

        var userPrompt = $"RUBRIC:\n{rubricJson}\n\nSTUDENT ANSWER:\n{studentAnswer}";

        var payload = new
        {
            contents = new[] { new { parts = new[] { new { text = $"{systemPrompt}\n\n{userPrompt}" } } } },
            generationConfig = new { responseMimeType = "application/json", temperature = 0.1 }
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);

        var textResult = doc.RootElement.GetProperty("candidates")[0]
            .GetProperty("content").GetProperty("parts")[0]
            .GetProperty("text").GetString();

        // 1. Cạo sạch rác Markdown (nếu AI ngoan cố trả về ```json)
        if (!string.IsNullOrEmpty(textResult))
        {
            textResult = textResult.Trim();
            if (textResult.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                textResult = textResult.Substring(7);
            }
            if (textResult.EndsWith("```"))
            {
                textResult = textResult.Substring(0, textResult.Length - 3);
            }
            textResult = textResult.Trim();
        }

        // 2. IN RA CONSOLE ĐỂ BẮT QUẢ TANG
        _logger.LogWarning("[RAW GEMINI RESPONSE] \n{TextResult}", textResult);

        // 3. Deserialize với cấu hình chuẩn
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true // Cứu cánh nếu AI lỡ phẩy dư ở cuối JSON
        };

        return JsonSerializer.Deserialize<AiEvaluationResult>(textResult!, options)!;
    }
}