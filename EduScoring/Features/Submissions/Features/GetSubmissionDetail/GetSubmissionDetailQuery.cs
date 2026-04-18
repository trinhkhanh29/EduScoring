using System;
using System.Collections.Generic;

namespace EduScoring.Features.Submissions.Features.GetSubmissionDetail;

public record GetSubmissionDetailQuery(Guid SubmissionId, Guid UserId, string Role, bool IsAdmin);

public record ImageDto(int Id, string ImageUrl, DateTimeOffset UploadedAt);

public record EvaluationDto(int Id, double TotalScore, string OverallFeedback, DateTimeOffset EvaluatedAt);

public record SubmissionDetailDto(
    Guid Id, 
    string ExamTitle, 
    Guid? StudentId, 
    decimal? FinalScore, 
    decimal? LatestAiScore, 
    string Status, 
    List<ImageDto> Images, 
    List<EvaluationDto>? Evaluations);