namespace EduScoring.Features.Exams.Features.CreateExam;

// Dữ liệu từ Frontend gửi lên
public record CreateExamRequest(
    string Title, 
    string? Description, 
    Guid? TeacherId, // (Tùy chọn) Chỉ dành cho Admin khi muốn tạo hộ giáo viên khác
    bool AllowStudentSubmission, 
    bool RequireTeacherReview, 
    bool AllowAppeal);

// Command thực tế đẩy xuống Handler (Đã nhét thêm TeacherId từ Token vào)
public record CreateExamCommand(
    string Title, 
    string? Description, 
    Guid TeacherId, 
    bool AllowStudentSubmission, 
    bool RequireTeacherReview, 
    bool AllowAppeal);

// Dữ liệu trả về
public record CreateExamResponse(int ExamId, string Message);