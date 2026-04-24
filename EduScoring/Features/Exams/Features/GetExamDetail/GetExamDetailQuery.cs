namespace EduScoring.Features.Exams.Features.GetExamDetail;

public record GetExamDetailQuery(int Id, Guid UserId, string Role);

public record ExamDetailDto(
    int Id, 
    string Title, 
    string? Description, 
    string? TeacherName, 
    DateTimeOffset CreatedAt, 
    int RubricsCount,
    bool AllowStudentSubmission,
    bool RequireTeacherReview,
    bool AllowAppeal);