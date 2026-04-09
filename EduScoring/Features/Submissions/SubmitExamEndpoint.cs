using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using EduScoring.Common.Authentication;
using EduScoring.Common.Storage;
using EduScoring.Data;
using EduScoring.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.Submissions;

public static class SubmitExamEndpoint
{
    public static void MapSubmitExamEndpoint(this IEndpointRouteBuilder app)
    {
        // Minimal API hỗ trợ nhận upload File qua form-data với DisableAntiforgery
        app.MapPost("/api/submissions/upload", UploadSubmissionAsync)
           .DisableAntiforgery()
           .WithTags("Submissions");
    }

    [Authorize(Roles = AppRoles.Teacher + "," + AppRoles.Admin)] // <--- Chỉ Teacher/Admin mới được nộp bài
    private static async Task<IResult> UploadSubmissionAsync(
        [FromForm] int examId,
        [FromForm] Guid studentId, // <- Nhận ID học sinh từ form vì Teacher/Admin nộp thay/nộp bài thi giấy
        IFormFile file,
        AppDbContext db,
        CloudinaryService cloudService,
        HttpContext httpContext)
    {
        //(Tuỳ chọn) Bạn vẫn có thể lấy thông tin Teacher/ Admin từ token nếu cần log lại lịch sử người nào đã tải ảnh lên:
        //var uploaderId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);//

        // 1. Kiểm tra File
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { Message = "File ảnh không hợp lệ hoặc bị trống!" });
        }

        // 2. Kiểm tra Exam có tồn tại không
        var examExists = await db.Exams.AnyAsync(e => e.Id == examId);
        if (!examExists)
        {
            return Results.BadRequest(new { Message = $"Không tìm thấy kỳ thi có ID = {examId}" });
        }

        // (Có thể thêm bước kiểm tra StudentId có tồn tại trong hệ thống hay không tại đây nếu cần)

        // 3. Lấy hoặc tạo mới thông tin nộp bài (Submission)
        var submission = await db.Submissions
            .FirstOrDefaultAsync(s => s.ExamId == examId && s.StudentId == studentId);

        if (submission is null)
        {
            submission = new Submission
            {
                ExamId = examId,
                StudentId = studentId,
                Status = "Pending", // Đang chờ AI chấm
                SubmittedAt = DateTimeOffset.UtcNow
            };
            db.Submissions.Add(submission);
            await db.SaveChangesAsync(); // Lưu để sinh ra ID
        }

        // 4. Tính toán số trang (PageNumber)
        var pageNumber = await db.SubmissionImages
            .CountAsync(i => i.SubmissionId == submission.Id) + 1;

        // 5. Upload ảnh lên Cloudinary
        var imageUrl = await cloudService.UploadImageAsync(file);
        if (string.IsNullOrEmpty(imageUrl))
        {
            return Results.BadRequest(new { Message = "Lỗi khi upload ảnh lên Cloudinary!" });
        }

        // 6. Lưu Link ảnh vào bảng SubmissionImages
        var imageEntity = new SubmissionImage
        {
            SubmissionId = submission.Id,
            ImageUrl = imageUrl,
            PageNumber = pageNumber
        };
        db.SubmissionImages.Add(imageEntity);

        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            Message = "Tải ảnh bài thi thành công!",
            SubmissionId = submission.Id,
            ImageUrl = imageUrl,
            PageNumber = imageEntity.PageNumber
        });
    }
}