using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;
using EduScoring.Common.Messaging;

namespace EduScoring.Features.Submissions.Features.TriggerAiEvaluation;

public class TriggerAiEvaluationCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IRabbitMQService _rabbitMQ;

    public TriggerAiEvaluationCommandHandler(AppDbContext db, IRabbitMQService rabbitMQ)
    {
        _db = db;
        _rabbitMQ = rabbitMQ;
    }

    public async Task<(bool IsSuccess, string ErrorMessage, int StatusCode)> Handle(TriggerAiEvaluationCommand command)
    {
        var tag = $"[TriggerAiEvaluation | EntityId={command.SubmissionId}]";

        var submission = await _db.Submissions
            .Include(s => s.Exam)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId);

        if (submission == null)
        {
            return (false, "Không tìm thấy bài nộp.", 404);
        }

        if (submission.IsLocked)
        {
            Console.WriteLine($"{tag} THẤT BẠI — Cố gắng sửa đổi bài thi đã bị khóa.");
            return (false, "Bài thi đã bị khóa sổ, không thể thay đổi hoặc cập nhật dữ liệu!", 400);
        }

        bool isAuthorized = command.IsAdmin || (submission.Exam != null && submission.Exam.TeacherId == command.UserId);

        if (!isAuthorized)
        {
            return (false, "Bạn không có quyền chạy AI chấm điểm cho bài nộp này.", 403);
        }

        submission.LastEvaluationTrigger = "Manual";
        submission.Status = "Evaluating";

        await _db.SaveChangesAsync();

        await _rabbitMQ.PublishAsync("ai_evaluation_triggered_queue", new AiEvaluationTriggeredEvent(submission.Id));

        Console.WriteLine($"{tag} Đã đưa vào hàng đợi AI — trigger: Manual");

        return (true, string.Empty, 202);
    }
}