namespace EduScoring.Application.DTOs;

/// <summary>
/// Data transfer object for submitting a new essay for evaluation.
/// </summary>
public record SubmitEssayRequest(
    string StudentId,
    string StudentName,
    string Title,
    string EssayContent,
    Guid GradingCriteriaId);
