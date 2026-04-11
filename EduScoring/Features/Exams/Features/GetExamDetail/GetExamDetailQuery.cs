namespace EduScoring.Features.Exams.Features.GetExamDetail;

public record GetExamDetailQuery(int Id, Guid UserId, bool IsAdmin);

public record GetExamDetailResponse(int Id, string Title, string? Description, string? TeacherName, DateTimeOffset CreatedAt, int RubricsCount);