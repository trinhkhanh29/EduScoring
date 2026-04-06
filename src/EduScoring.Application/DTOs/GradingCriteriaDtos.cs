namespace EduScoring.Application.DTOs;

/// <summary>
/// Data transfer object for creating a new grading criteria (rubric).
/// </summary>
public record CreateGradingCriteriaRequest(
    string Name,
    string Description,
    IEnumerable<ScoringDimensionRequest> Dimensions,
    string PromptTemplate);

public record ScoringDimensionRequest(
    string Name,
    string Description,
    double MaxPoints);

/// <summary>
/// Data transfer object representing a grading criteria summary.
/// </summary>
public record GradingCriteriaResponse(
    Guid Id,
    string Name,
    string Description,
    double MaxScore,
    IReadOnlyList<ScoringDimensionRequest> Dimensions);
