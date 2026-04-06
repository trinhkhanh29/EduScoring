using EduScoring.Domain.Entities;

namespace EduScoring.Application.Interfaces;

/// <summary>
/// Repository contract for managing <see cref="StudentEssaySubmission"/> persistence.
/// </summary>
public interface ISubmissionRepository
{
    Task<StudentEssaySubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentEssaySubmission>> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentEssaySubmission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(StudentEssaySubmission submission, CancellationToken cancellationToken = default);
    Task UpdateAsync(StudentEssaySubmission submission, CancellationToken cancellationToken = default);
}
