using EduScoring.Features.Exams.Models;
using EduScoring.Features.Submissions.Models;

namespace EduScoring.Features.Submissions.Services;

public interface IEvaluationValidator
{
    bool ValidateAiOutput(AiEvaluationResultDto result, Rubric rubric);
}