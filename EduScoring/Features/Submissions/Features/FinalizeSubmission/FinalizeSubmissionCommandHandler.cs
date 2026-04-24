using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;
using EduScoring.Common.Messaging;

namespace EduScoring.Features.Submissions.Features.FinalizeSubmission;

public class FinalizeSubmissionCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IRabbitMQService _rabbitMQ;

    public FinalizeSubmissionCommandHandler(AppDbContext db, IRabbitMQService rabbitMQ)
    {
        _db = db;
        _rabbitMQ = rabbitMQ;
    }

    public async Task<(bool IsSuccess, string ErrorMessage, int StatusCode)> Handle(FinalizeSubmissionCommand command)
    {
        var tag = $"[FinalizeSubmission | EntityId={command.SubmissionId}]";

        var submission = await _db.Submissions
            .Include(s => s.Exam)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId);

        if (submission == null)
        {
            return (false, "Không tìm thấy bài nộp.", 404);
        }

        bool isAuthorized = command.IsAdmin || (submission.Exam != null && submission.Exam.TeacherId == command.UserId);

        if (!isAuthorized)
        {
            return (false, "Bạn không có quyền đóng băng bài thi này.", 403);
        }

        if (submission.FinalScore == null)
        {
            return (false, "Chưa có điểm chốt, không thể finalize.", 400);
        }

        submission.IsLocked = true;
        submission.Status = "Finalized";

        await _db.SaveChangesAsync();

        await _rabbitMQ.PublishAsync("submission_finalized_queue", new SubmissionFinalizedEvent(submission.Id));

        Console.WriteLine($"{tag} THÀNH CÔNG — Bài thi đã bị KHÓA.");

        return (true, string.Empty, 200);
    }
}