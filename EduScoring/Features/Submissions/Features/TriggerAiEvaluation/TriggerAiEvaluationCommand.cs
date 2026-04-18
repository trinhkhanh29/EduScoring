using System;

namespace EduScoring.Features.Submissions.Features.TriggerAiEvaluation;

public record TriggerAiEvaluationCommand(Guid SubmissionId, Guid UserId, bool IsAdmin);

public record AiEvaluationTriggeredEvent(Guid SubmissionId);