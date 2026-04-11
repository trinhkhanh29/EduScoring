namespace EduScoring.Features.Exams.Features.CreateExam;

// Dữ liệu từ Frontend gửi lên (Chưa có TeacherId vì Token giữ)
public record CreateExamRequest(string Title, string Description);

// Command thực tế đẩy xuống Handler (Đã nhét thêm TeacherId từ Token vào)
public record CreateExamCommand(string Title, string Description, Guid TeacherId);

// Dữ liệu trả về
public record CreateExamResponse(int ExamId, string Message);