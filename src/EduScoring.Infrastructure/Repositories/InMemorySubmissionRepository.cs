using System.Collections.Concurrent;
using EduScoring.Application.Interfaces;
using EduScoring.Domain.Entities;

namespace EduScoring.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of <see cref="ISubmissionRepository"/>.
/// Replace with a database-backed implementation (e.g., EF Core) for production use.
/// </summary>
public class InMemorySubmissionRepository : ISubmissionRepository
{
    private readonly ConcurrentDictionary<Guid, StudentEssaySubmission> _store = new();

    public Task<StudentEssaySubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var submission);
        return Task.FromResult(submission);
    }

    public Task<IEnumerable<StudentEssaySubmission>> GetByStudentIdAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        var results = _store.Values.Where(s => s.StudentId == studentId);
        return Task.FromResult(results);
    }

    public Task<IEnumerable<StudentEssaySubmission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<StudentEssaySubmission>>(_store.Values.ToList());
    }

    public Task AddAsync(StudentEssaySubmission submission, CancellationToken cancellationToken = default)
    {
        _store[submission.Id] = submission;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(StudentEssaySubmission submission, CancellationToken cancellationToken = default)
    {
        _store[submission.Id] = submission;
        return Task.CompletedTask;
    }
}
