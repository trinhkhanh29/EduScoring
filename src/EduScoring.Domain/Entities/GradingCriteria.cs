using EduScoring.Domain.Common;

namespace EduScoring.Domain.Entities;

/// <summary>
/// Holds standardized scoring rubrics used to evaluate essay submissions.
/// </summary>
public class GradingCriteria : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public IReadOnlyList<ScoringDimension> Dimensions { get; private set; } = [];
    public double MaxScore { get; private set; }
    public string PromptTemplate { get; private set; } = string.Empty;

    private GradingCriteria() { }

    public static GradingCriteria Create(
        string name,
        string description,
        IEnumerable<ScoringDimension> dimensions,
        string promptTemplate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Criteria name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(promptTemplate))
            throw new ArgumentException("Prompt template cannot be empty.", nameof(promptTemplate));

        var dimensionList = dimensions.ToList();
        if (dimensionList.Count == 0)
            throw new ArgumentException("At least one scoring dimension is required.", nameof(dimensions));

        var maxScore = dimensionList.Sum(d => d.MaxPoints);

        return new GradingCriteria
        {
            Name = name,
            Description = description,
            Dimensions = dimensionList.AsReadOnly(),
            MaxScore = maxScore,
            PromptTemplate = promptTemplate
        };
    }
}

/// <summary>
/// Represents a single dimension within a grading rubric (e.g., Grammar, Coherence).
/// </summary>
public record ScoringDimension(
    string Name,
    string Description,
    double MaxPoints);
