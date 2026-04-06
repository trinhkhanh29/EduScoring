using EduScoring.Domain.Common;

namespace EduScoring.Domain.Entities;

/// <summary>
/// Represents a student's essay submission for automated grading.
/// </summary>
public class StudentEssaySubmission : BaseEntity
{
    public string StudentId { get; private set; } = string.Empty;
    public string StudentName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string EssayContent { get; private set; } = string.Empty;
    public Guid GradingCriteriaId { get; private set; }
    public SubmissionStatus Status { get; private set; } = SubmissionStatus.Pending;

    // Navigation property
    public GradingCriteria? GradingCriteria { get; private set; }
    public EvaluationResult? EvaluationResult { get; private set; }

    private StudentEssaySubmission() { }

    public static StudentEssaySubmission Create(
        string studentId,
        string studentName,
        string title,
        string essayContent,
        Guid gradingCriteriaId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be empty.", nameof(studentId));
        if (string.IsNullOrWhiteSpace(essayContent))
            throw new ArgumentException("Essay content cannot be empty.", nameof(essayContent));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Essay title cannot be empty.", nameof(title));

        return new StudentEssaySubmission
        {
            StudentId = studentId,
            StudentName = studentName,
            Title = title,
            EssayContent = essayContent,
            GradingCriteriaId = gradingCriteriaId,
            Status = SubmissionStatus.Pending
        };
    }

    public void MarkAsProcessing() => Status = SubmissionStatus.Processing;

    public void MarkAsCompleted() => Status = SubmissionStatus.Completed;

    public void MarkAsFailed() => Status = SubmissionStatus.Failed;
}

public enum SubmissionStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
