using System;

namespace EduScoring.Features.Submissions.Features.CreateSubmission;

public record CreateSubmissionRequest(int ExamId, Guid? TargetStudentId);

public record CreateSubmissionCommand(
    int ExamId, 
    Guid? TargetStudentId, 
    string CreatedSource, 
    Guid UserId, 
    string Role);

public record CreateSubmissionResponse(Guid SubmissionId, string Message);

public record SubmissionCreatedEvent(Guid SubmissionId);