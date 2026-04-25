using EduScoring.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EduScoring.Features.Exams.Features.GetExamDetail;

public class GetExamDetailQueryHandler
{
    private readonly AppReadDbContext _db;
    public GetExamDetailQueryHandler(AppReadDbContext db) => _db = db;

    public async Task<(bool IsSuccess, ExamDetailDto? Data, string ErrorMessage, int StatusCode)> Handle(GetExamDetailQuery query)
    {
        var tag = $"[GetExamDetail | EntityId={query.Id}]";

        // 1. Dùng AsNoTracking theo chuẩn CQRS và Select map tĩnh (hiệu năng cao nhất)
        var exam = await _db.Exams
            .IgnoreQueryFilters()
            .Where(e => e.Id == query.Id)
            .Select(e => new 
            {
                e.Id,
                e.Title,
                e.Description,
                e.TeacherId,
                TeacherName = e.Teacher != null ? e.Teacher.FullName : null,
                e.CreatedAt,
                RubricsCount = e.Rubrics.Count,
                e.AllowStudentSubmission,
                e.RequireTeacherReview,
                e.AllowAppeal,
                e.IsDeleted
            })
            .FirstOrDefaultAsync();

        if (exam == null)
        {
            Console.WriteLine($"{tag} THẤT BẠI [404] — Không tìm thấy EntityId={query.Id} trong DB (chưa từng tồn tại).");
            return (false, null, "Đề thi không tồn tại hoặc đã bị xóa tạm thời.", 404);
        }

        // 2. Phân biệt soft-deleted vs active
        if (exam.IsDeleted)
        {
            Console.WriteLine($"{tag} THẤT BẠI [404] — EntityId={query.Id} đã bị soft-delete. UserId: {query.UserId}");
            return (false, null, "Đề thi không tồn tại hoặc đã bị xóa tạm thời.", 404);
        }

        // 3. Kiểm tra quyền nâng cao
        bool isAuthorized = 
            (query.Role == EduScoring.Common.Authentication.AppRoles.Admin) || 
            (query.Role == EduScoring.Common.Authentication.AppRoles.Teacher && exam.TeacherId == query.UserId) || 
            (query.Role == EduScoring.Common.Authentication.AppRoles.Student && exam.AllowStudentSubmission);

        if (!isAuthorized)
        {
            Console.WriteLine($"{tag} THẤT BẠI [403] — UserId: {query.UserId} ({query.Role}) không có quyền xem chi tiết đề thi này.");
            return (false, null, "Không có quyền xem chi tiết đề thi này.", 403);
        }

        // 4. Map DTO
        var response = new ExamDetailDto(
            exam.Id,
            exam.Title,
            exam.Description,
            exam.TeacherName,
            exam.CreatedAt,
            exam.RubricsCount,
            exam.AllowStudentSubmission,
            exam.RequireTeacherReview,
            exam.AllowAppeal);

        Console.WriteLine($"{tag} THÀNH CÔNG — Trả về đề thi '{exam.Title}' | Rubrics: {exam.RubricsCount} | Teacher: '{exam.TeacherName ?? "N/A"}'");
        return (true, response, string.Empty, 200);
    }
}