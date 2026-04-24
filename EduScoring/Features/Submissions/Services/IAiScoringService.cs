using System.Threading.Tasks;
using EduScoring.Features.Exams.Models;
using EduScoring.Features.Submissions.Models;

namespace EduScoring.Features.Submissions.Services;

public record AiEvaluationResultDto(decimal TotalScore, string OverallFeedback);
public record AiEvaluationResult(decimal TotalScore, List<AiCriteriaScore> CriteriaScores, string OverallFeedback, decimal ConfidenceScore);
public record AiCriteriaScore(string CriteriaName, decimal Score, decimal MaxScore, string Reasoning);
public interface IAiScoringService
{
    Task<AiEvaluationResult> EvaluateAsync(string studentAnswer, string rubricJson, string language);
}