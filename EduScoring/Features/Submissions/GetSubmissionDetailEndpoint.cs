using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EduScoring.Common.Authentication;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;

namespace EduScoring.Features.Submissions;

public static class GetSubmissionDetailEndpoint
{
    public static void MapGetSubmissionDetailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/submissions/{id:guid}", async (Guid id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Results.Unauthorized();

            var submission = await db.Submissions
                .Include(s => s.Exam)
                .Include(s => s.Images)
                .Include(s => s.Evaluations)
                // THÊM DÒNG NÀY ĐỂ INCLUDE CHI TIẾT ĐIỂM
                .ThenInclude(e => e.Details)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
                return Results.NotFound("Không tìm thấy bài nộp.");

            // ==========================================
            // 3. DATA OWNERSHIP
            // ==========================================
            var teacherId = submission.Exam?.TeacherId;

            bool isAuthorized = user.IsInRole(AppRoles.Admin) ||
                               (user.IsInRole(AppRoles.Teacher) && teacherId == userId) ||
                               (user.IsInRole(AppRoles.Student) && submission.StudentId == userId);

            if (!isAuthorized)
            {
                return Results.Forbid();
            }

            // ==========================================
            // 4. TRẢ VỀ KẾT QUẢ
            // ==========================================
            var response = new
            {
                submission.Id,
                ExamTitle = submission.Exam?.Title ?? "Đề thi không xác định",
                StudentId = submission.StudentId,
                // ĐÃ SỬA: Dùng FinalScore theo Model mới của Submission
                TotalScore = submission.FinalScore,
                submission.Status,
                Images = submission.Images.Select(img => new { img.Id, img.ImageUrl }),
                Feedbacks = submission.Evaluations.Select(eval => new
                {
                    eval.RubricId,
                    // ĐÃ SỬA: Thay thế bằng TotalScore và OverallFeedback
                    AwardedScore = eval.TotalScore,
                    AiFeedback = eval.OverallFeedback,
                    eval.Status,
                    eval.ConfidenceScore,
                    // Bổ sung thêm trả về chi tiết các tiêu chí (Criteria)
                    Details = eval.Details.Select(d => new
                    {
                        d.CriteriaName,
                        d.CriteriaGroup,
                        d.Score,
                        d.MaxScore,
                        d.Reasoning,
                        d.Evidence
                    })
                })
            };

            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithTags("Submissions");
    }
}