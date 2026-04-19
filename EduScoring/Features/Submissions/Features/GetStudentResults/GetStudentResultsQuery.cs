using System;

namespace EduScoring.Features.Submissions.Features.GetStudentResults;

public record GetStudentResultsQuery(Guid StudentId);

public record StudentResultDto(Guid SubmissionId, string ExamTitle, DateTimeOffset SubmittedAt, string Status, decimal? FinalScore);
