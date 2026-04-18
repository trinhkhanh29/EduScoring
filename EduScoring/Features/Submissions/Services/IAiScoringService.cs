using System.Threading.Tasks;
using EduScoring.Features.Exams.Models;
using EduScoring.Features.Submissions.Models;

namespace EduScoring.Features.Submissions.Services;

public record AiEvaluationResultDto(decimal TotalScore, string OverallFeedback);

public interface IAiScoringService
{
    Task<AiEvaluationResultDto> EvaluateAsync(string ocrText, int rubricId, string language);
}