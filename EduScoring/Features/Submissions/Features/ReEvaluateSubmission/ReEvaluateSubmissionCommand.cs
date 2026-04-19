using System;

namespace EduScoring.Features.Submissions.Features.ReEvaluateSubmission;

public record ReEvaluateSubmissionCommand(Guid SubmissionId, Guid UserId, bool IsAdmin);

public record SubmissionReEvaluationTriggeredEvent(Guid SubmissionId);
