using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EduScoring.Application.Interfaces;
using EduScoring.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduScoring.Infrastructure.Services;

/// <summary>
/// Implements <see cref="ILLMEvaluationService"/> by calling an OpenAI-compatible Chat Completions API.
/// This implementation works with OpenAI (GPT models) and other compatible providers.
/// </summary>
public class LLMEvaluationService : ILLMEvaluationService
{
    private readonly HttpClient _httpClient;
    private readonly LlmOptions _options;
    private readonly ILogger<LLMEvaluationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public LLMEvaluationService(
        HttpClient httpClient,
        IOptions<LlmOptions> options,
        ILogger<LLMEvaluationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<EvaluationResult> EvaluateAsync(
        StudentEssaySubmission submission,
        GradingCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting LLM evaluation for submission {SubmissionId} using model {Model}",
            submission.Id, _options.Model);

        var prompt = BuildEvaluationPrompt(submission, criteria);
        var rawResponse = await CallLlmAsync(prompt, cancellationToken);

        var result = ParseEvaluationResponse(submission.Id, criteria, rawResponse);

        _logger.LogInformation(
            "Completed LLM evaluation for submission {SubmissionId}: score {Score}/{MaxScore}",
            submission.Id, result.TotalScore, result.MaxPossibleScore);

        return result;
    }

    /// <inheritdoc />
    public async Task<string> AnalyzeTextContextAsync(
        string textContent,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        var prompt = string.IsNullOrWhiteSpace(context)
            ? $"Analyze the following text for key themes, writing quality, and coherence:\n\n{textContent}"
            : $"{context}\n\nText to analyze:\n\n{textContent}";

        return await CallLlmAsync(prompt, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private string BuildEvaluationPrompt(StudentEssaySubmission submission, GradingCriteria criteria)
    {
        var dimensionDescriptions = string.Join(
            "\n",
            criteria.Dimensions.Select(d => $"- {d.Name} (max {d.MaxPoints} points): {d.Description}"));

        var template = criteria.PromptTemplate
            .Replace("{EssayContent}", submission.EssayContent)
            .Replace("{EssayTitle}", submission.Title)
            .Replace("{StudentName}", submission.StudentName)
            .Replace("{Dimensions}", dimensionDescriptions)
            .Replace("{MaxScore}", criteria.MaxScore.ToString());

        // Append strict JSON output requirement
        return template + """


Please respond ONLY with a valid JSON object in the following format (no markdown, no extra text):
{
  "total_score": <number>,
  "dimension_scores": [
    { "dimension_name": "<name>", "score": <number>, "max_points": <number>, "feedback": "<text>" }
  ],
  "overall_feedback": "<constructive overall feedback>",
  "strengths_summary": "<summary of essay strengths>",
  "improvement_suggestions": "<specific, actionable improvement suggestions>"
}
""";
    }

    private async Task<string> CallLlmAsync(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = new ChatCompletionRequest
        {
            Model = _options.Model,
            Messages = [new ChatMessage { Role = "user", Content = prompt }],
            MaxTokens = _options.MaxTokens,
            Temperature = _options.Temperature
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/chat/completions");
        request.Content = content;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize LLM response.");

        return completion.Choices?.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("LLM returned an empty response.");
    }

    private EvaluationResult ParseEvaluationResponse(
        Guid submissionId,
        GradingCriteria criteria,
        string rawResponse)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<EvaluationResponsePayload>(rawResponse, JsonOptions)
                ?? throw new InvalidOperationException("Could not parse LLM evaluation response.");

            var dimensionScores = (parsed.DimensionScores ?? []).Select(d =>
                new DimensionScore(d.DimensionName ?? string.Empty, d.Score, d.MaxPoints, d.Feedback ?? string.Empty));

            return EvaluationResult.Create(
                submissionId,
                parsed.TotalScore,
                criteria.MaxScore,
                dimensionScores,
                parsed.OverallFeedback ?? string.Empty,
                parsed.StrengthsSummary ?? string.Empty,
                parsed.ImprovementSuggestions ?? string.Empty,
                _options.Model,
                rawResponse);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse structured LLM response; storing raw response as feedback.");

            // Graceful fallback: store raw response as overall feedback
            return EvaluationResult.Create(
                submissionId,
                0,
                criteria.MaxScore,
                [],
                rawResponse,
                string.Empty,
                "Unable to parse structured response from LLM. Please review the raw feedback above.",
                _options.Model,
                rawResponse);
        }
    }

    // -------------------------------------------------------------------------
    // Private DTOs for OpenAI Chat Completions API
    // -------------------------------------------------------------------------

    private sealed class ChatCompletionRequest
    {
        public string Model { get; set; } = string.Empty;
        public IList<ChatMessage> Messages { get; set; } = [];
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
    }

    private sealed class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private sealed class ChatCompletionResponse
    {
        public IList<ChatChoice>? Choices { get; set; }
    }

    private sealed class ChatChoice
    {
        public ChatMessage? Message { get; set; }
    }

    private sealed class EvaluationResponsePayload
    {
        public double TotalScore { get; set; }
        public IList<DimensionScorePayload>? DimensionScores { get; set; }
        public string? OverallFeedback { get; set; }
        public string? StrengthsSummary { get; set; }
        public string? ImprovementSuggestions { get; set; }
    }

    private sealed class DimensionScorePayload
    {
        public string? DimensionName { get; set; }
        public double Score { get; set; }
        public double MaxPoints { get; set; }
        public string? Feedback { get; set; }
    }
}
