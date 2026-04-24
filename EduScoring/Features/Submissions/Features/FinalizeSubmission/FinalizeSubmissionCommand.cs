using System;

namespace EduScoring.Features.Submissions.Features.FinalizeSubmission;

public record FinalizeSubmissionCommand(Guid SubmissionId, Guid UserId, bool IsAdmin);

public record SubmissionFinalizedEvent(Guid SubmissionId);