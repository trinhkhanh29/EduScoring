namespace EduScoring.Features.Exams.Features.UpdateExam;

public record UpdateExamRequest(string Title, string Description);

public record UpdateExamCommand(int Id, string Title, string Description, Guid UserId, bool IsAdmin);