using System;

namespace EduScoring.Features.Submissions.Features.ResolveAppeal;

public record ResolveAppealCommand(
    int AppealId,
    bool IsAccepted,
    decimal? NewScore,
    string TeacherFeedback,
    Guid UserId,
    bool IsAdmin
);
