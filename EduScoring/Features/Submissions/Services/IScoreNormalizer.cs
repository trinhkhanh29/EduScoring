namespace EduScoring.Features.Submissions.Services;

public interface IScoreNormalizer
{
    decimal Normalize(decimal rawScore, decimal maxScore);
}