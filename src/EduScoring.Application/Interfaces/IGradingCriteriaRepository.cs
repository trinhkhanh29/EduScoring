using EduScoring.Domain.Entities;

namespace EduScoring.Application.Interfaces;

/// <summary>
/// Repository contract for managing <see cref="GradingCriteria"/> persistence.
/// </summary>
public interface IGradingCriteriaRepository
{
    Task<GradingCriteria?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<GradingCriteria>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(GradingCriteria criteria, CancellationToken cancellationToken = default);
}
