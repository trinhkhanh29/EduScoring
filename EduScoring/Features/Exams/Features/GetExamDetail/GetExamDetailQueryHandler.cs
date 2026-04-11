using EduScoring.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.Exams.Features.GetExamDetail;

public class GetExamDetailQueryHandler
{
    private readonly AppDbContext _db;
    public GetExamDetailQueryHandler(AppDbContext db) => _db = db;

    public async Task<(bool IsSuccess, GetExamDetailResponse? Data, string ErrorMessage, int StatusCode)> Handle(GetExamDetailQuery query)
    {
        var tag = $"[GetExamDetailHandler | ExamId={query.Id}]";

        // 1. Tìm exam — tách riêng soft-delete để log rõ lý do 404
        var exam = await _db.Exams
            .IgnoreQueryFilters()                  // bỏ qua global filter soft-delete nếu có
            .Include(e => e.Teacher)
            .Include(e => e.Rubrics)
            .FirstOrDefaultAsync(e => e.Id == query.Id);

        if (exam == null)
        {
            Console.WriteLine($"{tag} THẤT BẠI [404] — Không tìm thấy ExamId={query.Id} trong DB (chưa từng tồn tại).");
            return (false, null, "Đề thi không tồn tại hoặc đã bị xóa tạm thời.", 404);
        }

        // 2. Phân biệt soft-deleted vs active
        if (exam.IsDeleted)
        {
            Console.WriteLine($"{tag} THẤT BẠI [404] — ExamId={query.Id} đã bị soft-delete. UserId: {query.UserId}");
            return (false, null, "Đề thi không tồn tại hoặc đã bị xóa tạm thời.", 404);
        }

        // 3. Kiểm tra quyền — log rõ ai đang cố xem của ai
        bool isAuthorized = query.IsAdmin || exam.TeacherId == query.UserId;
        if (!isAuthorized)
        {
            Console.WriteLine($"{tag} THẤT BẠI [403] — UserId: {query.UserId} không phải chủ sở hữu (TeacherId: {exam.TeacherId}) và không phải Admin.");
            return (false, null, "Không có quyền xem chi tiết đề thi này.", 403);
        }

        // 4. Cảnh báo dữ liệu không toàn vẹn
        if (exam.Teacher == null)
            Console.WriteLine($"{tag} CẢNH BÁO — ExamId={query.Id} không có Teacher liên kết (TeacherId: {exam.TeacherId}). Dữ liệu có thể không toàn vẹn.");

        var response = new GetExamDetailResponse(
            exam.Id,
            exam.Title,
            exam.Description,
            exam.Teacher?.FullName,
            exam.CreatedAt,
            exam.Rubrics.Count);

        Console.WriteLine($"{tag} THÀNH CÔNG — Trả về đề thi '{exam.Title}' | Rubrics: {exam.Rubrics.Count} | Teacher: '{exam.Teacher?.FullName ?? "N/A"}'");
        return (true, response, string.Empty, 200);
    }
}