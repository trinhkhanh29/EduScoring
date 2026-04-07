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
        // Phải có DisableAntiforgery() để Minimal API hỗ trợ nhận upload File qua form-data
        app.MapPost("/api/submissions/upload",
            [Authorize(Roles = AppRoles.Student)] // <--- Chỉ sinh viên mới được nộp bài
        async (
                [FromForm] int examId,
                IFormFile file,
                AppDbContext db,
                CloudinaryService cloudService,
                HttpContext httpContext) =>
            {
                // 1. Lấy StudentId từ Token
                var studentIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                   ?? httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (!Guid.TryParse(studentIdString, out Guid studentId))
                    return Results.Unauthorized();

                // 2. Kiểm tra File
                if (file == null || file.Length == 0)
                    return Results.BadRequest(new { Message = "File ảnh không hợp lệ hoặc bị trống!" });

                // 3. Kiểm tra Exam có tồn tại không
                var examExists = await db.Exams.AnyAsync(e => e.Id == examId);
                if (!examExists)
                    return Results.BadRequest(new { Message = $"Không tìm thấy kỳ thi có ID = {examId}" });

                // 4. Upload ảnh lên Cloudinary
                var imageUrl = await cloudService.UploadImageAsync(file);
                if (string.IsNullOrEmpty(imageUrl))
                    return Results.BadRequest(new { Message = "Lỗi khi upload ảnh lên Cloudinary!" });

                // 5. Lưu thông tin nộp bài (Submission)
                // Sinh viên có thể nộp nhiều trang (nhiều ảnh), ta kiểm tra xem đã có Submission cho bài thi này chưa
                var existingSubmission = await db.Submissions
                    .FirstOrDefaultAsync(s => s.ExamId == examId && s.StudentId == studentId);

                if (existingSubmission == null)
                {
                    existingSubmission = new Submission
                    {
                        ExamId = examId,
                        StudentId = studentId,
                        Status = "Pending", // Đang chờ AI chấm
                        SubmittedAt = DateTimeOffset.UtcNow
                    };
                    db.Submissions.Add(existingSubmission);
                    await db.SaveChangesAsync(); // Lưu để lấy ID
                }

                // 6. Lưu Link ảnh vào bảng SubmissionImages
                var imageEntity = new SubmissionImage
                {
                    SubmissionId = existingSubmission.Id,
                    ImageUrl = imageUrl,
                    PageNumber = await db.SubmissionImages.CountAsync(i => i.SubmissionId == existingSubmission.Id) + 1
                };
                db.SubmissionImages.Add(imageEntity);

                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    Message = "Nộp bài và tải ảnh thành công!",
                    SubmissionId = existingSubmission.Id,
                    ImageUrl = imageUrl,
                    PageNumber = imageEntity.PageNumber
                });
            })
        .DisableAntiforgery()
        .WithTags("Submissions");
    }
}