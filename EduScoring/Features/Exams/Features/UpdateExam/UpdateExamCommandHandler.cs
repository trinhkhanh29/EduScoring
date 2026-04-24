using EduScoring.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.Exams.Features.UpdateExam;

public class UpdateExamCommandHandler
{
    private readonly AppDbContext _db;
    public UpdateExamCommandHandler(AppDbContext db) => _db = db;

    public async Task<(bool IsSuccess, string ErrorMessage, int StatusCode)> Handle(UpdateExamCommand command)
    {
        var tag = $"[UpdateExamHandler | ExamId={command.Id}]";

        // 0. Validate lõi
        if (string.IsNullOrWhiteSpace(command.Title))
            return (false, "Title không được để trống!", 400);

        // 1. Tìm exam
        var exam = await _db.Exams.FirstOrDefaultAsync(e => e.Id == command.Id);
        if (exam == null)
        {
            Console.WriteLine($"{tag} THẤT BẠI [404] — Không tìm thấy đề thi trong DB.");
            return (false, "Không tìm thấy đề thi để cập nhật!", 404);
        }

        // 2. Kiểm tra quyền
        bool isAuthorized = command.IsAdmin || exam.TeacherId == command.UserId;
        if (!isAuthorized)
        {
            Console.WriteLine($"{tag} THẤT BẠI [403] — UserId: {command.UserId} không phải chủ sở hữu (TeacherId: {exam.TeacherId}) và không phải Admin.");
            return (false, "Không có quyền sửa đề thi của giảng viên khác!", 403);
        }

        // 3. Cập nhật các trường thông tin và Policy
        exam.Title = command.Title;
        exam.Description = command.Description;
        exam.AllowStudentSubmission = command.AllowStudentSubmission;
        exam.RequireTeacherReview = command.RequireTeacherReview;
        exam.AllowAppeal = command.AllowAppeal;

        // Nếu là Admin và muốn đổi người gác thi/sở hữu đề
        if (command.IsAdmin && command.TeacherId.HasValue)
        {
            exam.TeacherId = command.TeacherId.Value;
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"{tag} THẤT BẠI — Lỗi khi lưu DB. DbUpdateException: {ex.Message} | Inner: {ex.InnerException?.Message}");
            return (false, "Lỗi hệ thống khi cập nhật đề thi. Vui lòng thử lại.", 500);
        }

        Console.WriteLine($"[UpdateExam | EntityId={command.Id}] THÀNH CÔNG — Cập nhật cấu hình đề thi | UserId: {command.UserId}");
        return (true, string.Empty, 200);
    }
}