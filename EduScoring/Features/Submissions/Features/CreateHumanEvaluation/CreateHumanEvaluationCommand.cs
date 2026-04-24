using System;

namespace EduScoring.Features.Submissions.Features.CreateHumanEvaluation;

public record CreateHumanEvaluationCommand(
    Guid SubmissionId,
    decimal TeacherScore,
    string TeacherFeedback,
    Guid UserId,
    bool IsAdmin
);