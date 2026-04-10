using System.Security.Claims;
using EduScoring.Common.Authentication;
using EduScoring.Common.Storage;
using EduScoring.Data.Entities;
using EduScoring.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.Submissions;

public static class SubmitExamEndpoint
{
    public static void MapSubmitExamEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/submissions/upload", async (
            [FromForm] int examId,   // FIX 1: Đổi từ Guid sang int cho khớp với Database
            [FromForm] Guid studentId,
            IFormFile file,
            AppDbContext db,
            ICloudinaryService cloudService,
            ClaimsPrincipal user) =>
        {
            // 1. Kiểm tra quyền
            if (!user.IsInRole(AppRoles.Teacher) && !user.IsInRole(AppRoles.Admin))
            {
                return Results.Forbid();
            }

            // 2. Kiểm tra File đầu vào & xem có đúng định dạng ảnh không
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { Message = "File ảnh không hợp lệ hoặc bị trống!" });
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                return Results.BadRequest(new { Message = "File upload phải là định dạng hình ảnh (jpeg, png...)!" });
            }

            // 3. Kiểm tra Đề thi (và Sinh viên) có tồn tại không
            var examExists = await db.Exams.AnyAsync(e => e.Id == examId);
            if (!examExists)
            {
                return Results.BadRequest(new { Message = $"Không tìm thấy kỳ thi có ID = {examId}" });
            }

            // MỞ TRANSACTION: Đảm bảo nếu Upload ảnh lên Cloudinary lỗi thì không bị rác dữ liệu ở DB
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 4. Lấy túi đựng bài cũ, hoặc tạo túi mới
                var submission = await db.Submissions
                    .FirstOrDefaultAsync(s => s.ExamId == examId && s.StudentId == studentId);

                if (submission is null)
                {
                    submission = new Submission
                    {
                        ExamId = examId,
                        StudentId = studentId,
                        Status = "Pending",
                        SubmittedAt = DateTimeOffset.UtcNow
                    };
                    db.Submissions.Add(submission);
                    await db.SaveChangesAsync(); // Lưu để EF Core sinh ra SubmissionId
                }

                // 5. Tính toán trang số mấy (PageNumber)
                var pageNumber = await db.SubmissionImages
                    .CountAsync(i => i.SubmissionId == submission.Id) + 1;

                // 6. Gọi Cloudinary (Thực hiện khi DB đã sẵn sàng, nếu lỗi thì Rollback)
                var imageUrl = await cloudService.UploadImageAsync(file);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return Results.BadRequest(new { Message = "Lỗi khi upload ảnh lên Cloudinary!" });
                }

                // 7. Lưu đường link ảnh vào Database
                var imageEntity = new SubmissionImage
                {
                    SubmissionId = submission.Id,
                    ImageUrl = imageUrl,
                    PageNumber = pageNumber
                };
                db.SubmissionImages.Add(imageEntity);

                await db.SaveChangesAsync();

                // 8. Commit Giao dịch: Mọi thứ đều OK
                await transaction.CommitAsync();

                return Results.Ok(new
                {
                    Message = "Tải ảnh bài thi thành công!",
                    SubmissionId = submission.Id,
                    ImageUrl = imageUrl,
                    PageNumber = imageEntity.PageNumber
                });
            }
            catch (Exception) // FIX 2: Bỏ chữ 'ex' đi để hết cảnh báo vàng
            {
                // Hủy thay đổi trong Database nếu có rủi ro (đặc biệt là lỗi Cloudinary)
                await transaction.RollbackAsync();

                // Trả về lỗi server tĩnh nếu muốn, log error tùy nhu cầu
                return Results.Problem("Xảy ra lỗi trong quá trình xử lý bài nộp.");
            }
        })
        .RequireAuthorization()
        .DisableAntiforgery()
        .WithTags("Submissions");
    }
}