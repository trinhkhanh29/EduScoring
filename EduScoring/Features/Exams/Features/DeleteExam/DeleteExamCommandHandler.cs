using EduScoring.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.Exams.Features.DeleteExam;

public class DeleteExamCommandHandler
{
    private readonly AppDbContext _db;

    public DeleteExamCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(bool IsSuccess, string ErrorMessage, int StatusCode)> Handle(DeleteExamCommand command)
    {
        var exam = await _db.Exams.FirstOrDefaultAsync(e => e.Id == command.Id);

        if (exam == null)
            return (false, "Không tìm thấy đề thi để xóa!", 404);

        // Kiểm tra quyền (Admin hoặc Giáo viên sở hữu)
        bool isAuthorized = command.IsAdmin || exam.TeacherId == command.UserId;
        if (!isAuthorized)
            return (false, $"Tài khoản {command.UserId} không được phép xóa đề thi này!", 403);

        try
        {
            _db.Exams.Remove(exam);
            await _db.SaveChangesAsync();
            return (true, string.Empty, 200);
        }
        catch (DbUpdateException)
        {
            return (false, "Không thể xóa đề này vì đã có dữ liệu sinh viên nộp bài liên quan!", 400);
        }
    }
}