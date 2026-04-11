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

        // 3. Phát hiện thay đổi thực sự
        bool titleChanged = exam.Title != command.Title;
        bool descChanged = exam.Description != command.Description;

        if (!titleChanged && !descChanged)
        {
            Console.WriteLine($"{tag} Không có thay đổi nào — dữ liệu gửi lên giống DB hiện tại, bỏ qua SaveChanges.");
            return (true, string.Empty, 200);
        }

        Console.WriteLine($"{tag} Thay đổi phát hiện —{(titleChanged ? $" Title: '{exam.Title}' → '{command.Title}'" : "")}{(descChanged ? " | Description: đã thay đổi" : "")}");

        exam.Title = command.Title;
        exam.Description = command.Description;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"{tag} THẤT BẠI — Lỗi khi lưu DB. DbUpdateException: {ex.Message} | Inner: {ex.InnerException?.Message}");
            return (false, "Lỗi hệ thống khi cập nhật đề thi. Vui lòng thử lại.", 500);
        }

        Console.WriteLine($"{tag} THÀNH CÔNG — Đã lưu thay đổi vào DB.");
        return (true, string.Empty, 200);
    }
}