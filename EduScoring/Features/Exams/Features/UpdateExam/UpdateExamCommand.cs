namespace EduScoring.Features.Exams.Features.UpdateExam;

public record UpdateExamRequest(
    string Title, 
    string? Description,
    Guid? TeacherId, // (Option) Dành cho Admin muốn chuyển chủ bài thi
    bool AllowStudentSubmission,
    bool RequireTeacherReview,
    bool AllowAppeal);

public record UpdateExamCommand(
    int Id, 
    string Title, 
    string? Description, 
    Guid? TeacherId,
    Guid UserId, 
    bool IsAdmin,
    bool AllowStudentSubmission,
    bool RequireTeacherReview,
    bool AllowAppeal);