using System;

namespace EduScoring.Features.Submissions.Features.CreateAppeal;

public record CreateAppealCommand(Guid SubmissionId, string Reason, Guid UserId);

public record AppealCreatedEvent(int AppealId);
