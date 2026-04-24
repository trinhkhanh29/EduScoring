using EduScoring.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.Exams.Features.RestoreExam;

public class RestoreExamCommandHandler
{
    private readonly AppDbContext _db;
    public RestoreExamCommandHandler(AppDbContext db) => _db = db;

    public async Task<(bool IsSuccess, string SuccessMessage, string ErrorMessage, int StatusCode)> Handle(RestoreExamCommand command)
    {
        var tag = $"[RestoreExam | EntityId={command.Id}]";

        // 1. Tìm exam (bỏ qua soft-delete filter)
        var exam = await _db.Exams.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == command.Id);

        if (exam == null)
        {
            Console.WriteLine($"{tag} THẤT BẠI [404] — Không tìm thấy EntityId={command.Id} trong DB.");
            return (false, string.Empty, "Không tìm thấy đề thi này trong hệ thống.", 404);
        }

        // 2. Kiểm tra trạng thái — không cần phục hồi nếu đang active
        if (!exam.IsDeleted)
        {
            Console.WriteLine($"{tag} THẤT BẠI [400] — EntityId={command.Id} ('{exam.Title}') hiện đang active, không cần phục hồi.");
            return (false, string.Empty, "Đề thi này hiện đang hoạt động, không cần phục hồi.", 400);
        }

        // 3. Kiểm tra quyền
        bool isAuthorized = command.IsAdmin || exam.TeacherId == command.UserId;
        if (!isAuthorized)
        {
            Console.WriteLine($"{tag} THẤT BẠI [403] — UserId: {command.UserId} không phải chủ sở hữu (TeacherId: {exam.TeacherId}) và không phải Admin.");
            return (false, string.Empty, "Không có quyền phục hồi đề thi này.", 403);
        }

        // 4. Log trạng thái trước khi phục hồi
        Console.WriteLine($"{tag} Phục hồi — Đề thi: '{exam.Title}' | Đã xóa lúc: {exam.DeletedAt:yyyy-MM-dd HH:mm:ss} | Phục hồi bởi UserId: {command.UserId}");

        // 5. Phục hồi
        exam.IsDeleted = false;
        exam.DeletedAt = null;


        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"{tag} THẤT BẠI — Lỗi khi lưu DB. DbUpdateException: {ex.Message} | Inner: {ex.InnerException?.Message}");
            return (false, string.Empty, "Lỗi hệ thống khi phục hồi đề thi. Vui lòng thử lại.", 500);
        }

        var successMsg = $"Đã phục hồi đề thi '{exam.Title}' thành công!";
        Console.WriteLine($"{tag} THÀNH CÔNG — {successMsg}");
        return (true, successMsg, string.Empty, 200);
    }
}