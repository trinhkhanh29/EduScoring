using EduScoring.Domain.Entities;

namespace EduScoring.Application.DTOs;

/// <summary>
/// Data transfer object for returning the evaluation result to the API consumer.
/// </summary>
public record EvaluationResultResponse(
    Guid EvaluationId,
    Guid SubmissionId,
    double TotalScore,
    double MaxPossibleScore,
    double ScorePercentage,
    IReadOnlyList<DimensionScoreResponse> DimensionScores,
    string OverallFeedback,
    string StrengthsSummary,
    string ImprovementSuggestions,
    string LlmModel,
    DateTime EvaluatedAt)
{
    public static EvaluationResultResponse FromDomain(EvaluationResult result)
    {
        return new EvaluationResultResponse(
            EvaluationId: result.Id,
            SubmissionId: result.SubmissionId,
            TotalScore: result.TotalScore,
            MaxPossibleScore: result.MaxPossibleScore,
            ScorePercentage: result.ScorePercentage,
            DimensionScores: result.DimensionScores
                .Select(d => new DimensionScoreResponse(d.DimensionName, d.Score, d.MaxPoints, d.Feedback))
                .ToList()
                .AsReadOnly(),
            OverallFeedback: result.OverallFeedback,
            StrengthsSummary: result.StrengthsSummary,
            ImprovementSuggestions: result.ImprovementSuggestions,
            LlmModel: result.LlmModel,
            EvaluatedAt: result.CreatedAt);
    }
}

public record DimensionScoreResponse(
    string DimensionName,
    double Score,
    double MaxPoints,
    string Feedback);
