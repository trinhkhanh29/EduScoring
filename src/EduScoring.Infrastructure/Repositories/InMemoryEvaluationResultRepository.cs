using System.Collections.Concurrent;
using EduScoring.Application.Interfaces;
using EduScoring.Domain.Entities;

namespace EduScoring.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of <see cref="IEvaluationResultRepository"/>.
/// Replace with a database-backed implementation (e.g., EF Core) for production use.
/// </summary>
public class InMemoryEvaluationResultRepository : IEvaluationResultRepository
{
    private readonly ConcurrentDictionary<Guid, EvaluationResult> _storeById = new();
    private readonly ConcurrentDictionary<Guid, EvaluationResult> _storeBySubmissionId = new();

    public Task<EvaluationResult?> GetBySubmissionIdAsync(
        Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        _storeBySubmissionId.TryGetValue(submissionId, out var result);
        return Task.FromResult(result);
    }

    public Task AddAsync(EvaluationResult result, CancellationToken cancellationToken = default)
    {
        _storeById[result.Id] = result;
        _storeBySubmissionId[result.SubmissionId] = result;
        return Task.CompletedTask;
    }
}
