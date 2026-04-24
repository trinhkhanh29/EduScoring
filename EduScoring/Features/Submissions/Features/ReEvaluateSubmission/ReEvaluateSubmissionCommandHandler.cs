using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;
using EduScoring.Common.Messaging;

namespace EduScoring.Features.Submissions.Features.ReEvaluateSubmission;

public class ReEvaluateSubmissionCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IRabbitMQService _rabbitMQ;

    public ReEvaluateSubmissionCommandHandler(AppDbContext db, IRabbitMQService rabbitMQ)
    {
        _db = db;
        _rabbitMQ = rabbitMQ;
    }

    public async Task<(bool IsSuccess, string ErrorMessage, int StatusCode)> Handle(ReEvaluateSubmissionCommand command)
    {
        var tag = $"[ReEvaluateSubmission | EntityId={command.SubmissionId}]";

        var submission = await _db.Submissions
            .Include(s => s.Exam)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId);

        if (submission == null)
        {
            return (false, "Không tìm thấy bài nộp.", 404);
        }

        if (submission.IsLocked)
        {
            return (false, "Bài đã bị khóa, không thể chấm lại.", 400);
        }

        bool isAuthorized = command.IsAdmin || (submission.Exam != null && submission.Exam.TeacherId == command.UserId);
        if (!isAuthorized)
        {
            return (false, "Bạn không có quyền chấm lại bài này.", 403);
        }

        submission.LatestAiScore = null;
        submission.Status = "Pending Evaluation";

        await _db.SaveChangesAsync();

        await _rabbitMQ.PublishAsync("submission_reevaluation_triggered_queue", new SubmissionReEvaluationTriggeredEvent(submission.Id));

        Console.WriteLine($"{tag} THÀNH CÔNG — Giáo viên yêu cầu chấm lại.");

        return (true, "Đã gửi yêu cầu chấm lại thành công.", 202);
    }
}
