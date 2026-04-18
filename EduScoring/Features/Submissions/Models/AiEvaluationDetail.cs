using EduScoring.Features.Exams.Models;

namespace EduScoring.Features.Submissions.Models;

public class AiEvaluationDetail
{
    public Guid Id { get; set; }

    public int AiEvaluationId { get; set; }

    public int? RubricId { get; set; }
    public Rubric? Rubric { get; set; }

    public string CriteriaKey { get; set; } = string.Empty;

    public string CriteriaName { get; set; } = string.Empty;

    public string CriteriaGroup { get; set; } = "General";

    public double Score { get; set; }
    public double MaxScore { get; set; }

    public double Weight { get; set; } = 1.0;

    public string Reasoning { get; set; } = string.Empty;

    public string Evidence { get; set; } = string.Empty;

    public double Confidence { get; set; }

    public string? MetadataJson { get; set; }

    public AiEvaluation AiEvaluation { get; set; } = null!;
}