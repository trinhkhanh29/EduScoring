using EduScoring.Domain.Entities;

namespace EduScoring.Application.Interfaces;

/// <summary>
/// Repository contract for managing <see cref="EvaluationResult"/> persistence.
/// </summary>
public interface IEvaluationResultRepository
{
    Task<EvaluationResult?> GetBySubmissionIdAsync(Guid submissionId, CancellationToken cancellationToken = default);
    Task AddAsync(EvaluationResult result, CancellationToken cancellationToken = default);
}
