using EduScoring.Features.Exams.Models;
using EduScoring.Features.Submissions.Models;

namespace EduScoring.Features.Submissions.Services;

public interface IPromptBuilder
{
    string BuildScoringPrompt(string ocrText, Rubric rubric);
}