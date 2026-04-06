using System.Collections.Concurrent;
using EduScoring.Application.Interfaces;
using EduScoring.Domain.Entities;

namespace EduScoring.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of <see cref="IGradingCriteriaRepository"/>.
/// Replace with a database-backed implementation (e.g., EF Core) for production use.
/// </summary>
public class InMemoryGradingCriteriaRepository : IGradingCriteriaRepository
{
    private readonly ConcurrentDictionary<Guid, GradingCriteria> _store = new();

    public Task<GradingCriteria?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var criteria);
        return Task.FromResult(criteria);
    }

    public Task<IEnumerable<GradingCriteria>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<GradingCriteria>>(_store.Values.ToList());
    }

    public Task AddAsync(GradingCriteria criteria, CancellationToken cancellationToken = default)
    {
        _store[criteria.Id] = criteria;
        return Task.CompletedTask;
    }
}
