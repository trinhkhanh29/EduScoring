namespace EduScoring.Features.Exams.Features.RestoreExam;

public record RestoreExamCommand(int Id, Guid UserId, bool IsAdmin);