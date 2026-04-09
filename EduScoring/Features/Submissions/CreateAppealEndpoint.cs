using EduScoring.Data;
using EduScoring.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EduScoring.Common.Authentication;
using EduScoring.Common.Messaging;

namespace EduScoring.Features.Submissions.Appeals;

public static class CreateAppealEndpoint
{
    public record CreateAppealRequest(Guid SubmissionId, string Reason);

    public static void MapCreateAppealEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/appeals", async (CreateAppealRequest request, AppDbContext db, ClaimsPrincipal user, IRabbitMQService rabbitMq) =>
        {
            var studentIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(studentIdString, out Guid studentId))
                return Results.Unauthorized();

            var submission = await db.Submissions.FirstOrDefaultAsync(s => s.Id == request.SubmissionId);

            if (submission == null) return Results.NotFound("Không tìm thấy bài nộp.");
            if (submission.StudentId != studentId) return Results.Forbid();
            if (submission.Status != "Graded") return Results.BadRequest("Chỉ có thể phúc khảo bài đã có điểm.");

            var existingAppeal = await db.Appeals.AnyAsync(a => a.SubmissionId == request.SubmissionId);
            if (existingAppeal) return Results.BadRequest("Bạn đã gửi yêu cầu phúc khảo cho bài này rồi.");

            var appeal = new Appeal
            {
                SubmissionId = request.SubmissionId,
                StudentReason = request.Reason,
                Status = "Open",
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Appeals.Add(appeal);

            submission.Status = "Appealed";

            db.ActivityLogs.Add(new ActivityLog
            {
                UserId = studentId,
                ActionType = "CREATE_APPEAL",
                EntityName = "Appeals",
                EntityId = appeal.Id.ToString(),
                Details = $"Sinh viên yêu cầu phúc khảo bài thi {submission.ExamId}. Lý do: {request.Reason}"
            });

            await db.SaveChangesAsync();

            var notificationEvent = new
            {
                EventType = "NewAppeal",
                SubmissionId = submission.Id,
                Message = $"Sinh viên vừa gửi yêu cầu phúc khảo. Lý do: {request.Reason}"
            };

            await rabbitMq.PublishAsync("send-notification", notificationEvent);

            return Results.Ok(new { Message = "Gửi yêu cầu phúc khảo thành công!", AppealId = appeal.Id });
        })
        .RequireAuthorization(AppRoles.Student);
    }
}