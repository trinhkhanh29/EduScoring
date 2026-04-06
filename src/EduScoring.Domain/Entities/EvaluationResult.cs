using EduScoring.Domain.Common;

namespace EduScoring.Domain.Entities;

/// <summary>
/// Stores the automated score and constructive feedback produced by the LLM evaluation.
/// </summary>
public class EvaluationResult : BaseEntity
{
    public Guid SubmissionId { get; private set; }
    public double TotalScore { get; private set; }
    public double MaxPossibleScore { get; private set; }
    public IReadOnlyList<DimensionScore> DimensionScores { get; private set; } = [];
    public string OverallFeedback { get; private set; } = string.Empty;
    public string StrengthsSummary { get; private set; } = string.Empty;
    public string ImprovementSuggestions { get; private set; } = string.Empty;
    public string LlmModel { get; private set; } = string.Empty;
    public string RawLlmResponse { get; private set; } = string.Empty;

    // Derived property: percentage score
    public double ScorePercentage =>
        MaxPossibleScore > 0 ? Math.Round(TotalScore / MaxPossibleScore * 100, 2) : 0;

    // Navigation property
    public StudentEssaySubmission? Submission { get; private set; }

    private EvaluationResult() { }

    public static EvaluationResult Create(
        Guid submissionId,
        double totalScore,
        double maxPossibleScore,
        IEnumerable<DimensionScore> dimensionScores,
        string overallFeedback,
        string strengthsSummary,
        string improvementSuggestions,
        string llmModel,
        string rawLlmResponse)
    {
        if (submissionId == Guid.Empty)
            throw new ArgumentException("Submission ID cannot be empty.", nameof(submissionId));
        if (totalScore < 0)
            throw new ArgumentOutOfRangeException(nameof(totalScore), "Score cannot be negative.");
        if (maxPossibleScore <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPossibleScore), "Max possible score must be positive.");

        return new EvaluationResult
        {
            SubmissionId = submissionId,
            TotalScore = totalScore,
            MaxPossibleScore = maxPossibleScore,
            DimensionScores = dimensionScores.ToList().AsReadOnly(),
            OverallFeedback = overallFeedback,
            StrengthsSummary = strengthsSummary,
            ImprovementSuggestions = improvementSuggestions,
            LlmModel = llmModel,
            RawLlmResponse = rawLlmResponse
        };
    }
}

/// <summary>
/// The score and feedback for an individual rubric dimension.
/// </summary>
public record DimensionScore(
    string DimensionName,
    double Score,
    double MaxPoints,
    string Feedback);
