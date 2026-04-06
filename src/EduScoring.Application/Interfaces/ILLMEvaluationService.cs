using EduScoring.Domain.Entities;

namespace EduScoring.Application.Interfaces;

/// <summary>
/// Defines the contract for an LLM-based evaluation service that analyzes essay text
/// and assesses content quality according to the provided grading criteria.
/// </summary>
public interface ILLMEvaluationService
{
    /// <summary>
    /// Evaluates the given essay submission against the specified grading criteria
    /// and returns a structured evaluation result with scores and feedback.
    /// </summary>
    /// <param name="submission">The student essay submission to evaluate.</param>
    /// <param name="criteria">The grading rubric to apply during evaluation.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing scores and constructive feedback.</returns>
    Task<EvaluationResult> EvaluateAsync(
        StudentEssaySubmission submission,
        GradingCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes raw text content for key themes, writing quality, and coherence,
    /// returning a plain-text analysis report.
    /// </summary>
    /// <param name="textContent">The text content to analyze.</param>
    /// <param name="context">Optional context or instructions to guide the analysis.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A plain-text analysis report produced by the LLM.</returns>
    Task<string> AnalyzeTextContextAsync(
        string textContent,
        string? context = null,
        CancellationToken cancellationToken = default);
}
