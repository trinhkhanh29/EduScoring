namespace EduScoring.Features.Exams.Features.DeleteExam;

public record DeleteExamCommand(int Id, Guid UserId, bool IsAdmin);