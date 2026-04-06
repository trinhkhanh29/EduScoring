using EduScoring.Domain.Entities;

namespace EduScoring.Application.DTOs;

/// <summary>
/// Data transfer object representing a submission summary.
/// </summary>
public record SubmissionResponse(
    Guid SubmissionId,
    string StudentId,
    string StudentName,
    string Title,
    SubmissionStatus Status,
    DateTime SubmittedAt)
{
    public static SubmissionResponse FromDomain(StudentEssaySubmission submission)
    {
        return new SubmissionResponse(
            SubmissionId: submission.Id,
            StudentId: submission.StudentId,
            StudentName: submission.StudentName,
            Title: submission.Title,
            Status: submission.Status,
            SubmittedAt: submission.CreatedAt);
    }
}
