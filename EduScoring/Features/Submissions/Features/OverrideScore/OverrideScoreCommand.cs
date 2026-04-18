using System;

namespace EduScoring.Features.Submissions.Features.OverrideScore;

public record OverrideScoreRequest(decimal NewScore, string Reason);

public record OverrideScoreCommand(Guid SubmissionId, decimal NewScore, string Reason, Guid UserId, bool IsAdmin);
