using System;

namespace EduScoring.Features.Submissions.Features.ReviewAiEvaluation;

public record ReviewAiEvaluationCommand(Guid SubmissionId, Guid UserId, bool IsAdmin);
