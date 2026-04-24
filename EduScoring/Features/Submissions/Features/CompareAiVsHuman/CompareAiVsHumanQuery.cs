using System;

namespace EduScoring.Features.Submissions.Features.CompareAiVsHuman;

public record CompareAiVsHumanQuery(int ExamId, Guid UserId, bool IsAdmin);

public record CompareResultDto(
    int TotalEvaluated,
    double MeanAbsoluteError,
    double AccuracyWithinHalfPoint,
    List<DataPointDto> DataPoints
);

public record DataPointDto(Guid SubmissionId, decimal AiScore, decimal HumanScore);
