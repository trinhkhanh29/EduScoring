using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;
using EduScoring.Common.Messaging;

namespace EduScoring.Features.Submissions.Features.CreateAppeal;

public class CreateAppealCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IRabbitMQService _rabbitMQ;

    public CreateAppealCommandHandler(AppDbContext db, IRabbitMQService rabbitMQ)
    {
        _db = db;
        _rabbitMQ = rabbitMQ;
    }

    public async Task<(bool IsSuccess, string ErrorMessage, int StatusCode)> Handle(CreateAppealCommand command)
    {
        var tag = $"[CreateAppeal | EntityId={command.SubmissionId}]";

        var submission = await _db.Submissions
            .Include(s => s.Exam)
            .Include(s => s.Appeals)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId);

        if (submission == null)
        {
            return (false, "Không tìm thấy bài nộp.", 404);
        }

        // CHỐT CHẶN 1: Data Ownership
        if (submission.StudentId != command.UserId)
        {
            return (false, "Không được phúc khảo bài của người khác.", 403);
        }

        // CHỐT CHẶN 2: Bài phải đã khóa
        if (!submission.IsLocked)
        {
            return (false, "Bài chưa chốt điểm, không được phúc khảo.", 400);
        }

        // CHỐT CHẶN 3: Chính sách đề thi
        if (submission.Exam == null || !submission.Exam.AllowAppeal)
        {
            return (false, "Đề thi này không cho phép phúc khảo.", 400);
        }

        // CHỐT CHẶN 4: Chống spam
        if (submission.Appeals.Any(a => a.Status == "Pending"))
        {
            return (false, "Đang có đơn phúc khảo chờ duyệt, không được gửi thêm.", 400);
        }

        var appeal = new Appeal
        {
            SubmissionId = command.SubmissionId,
            StudentReason = command.Reason,
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Appeals.Add(appeal);
        await _db.SaveChangesAsync();

        await _rabbitMQ.PublishAsync("appeal_created_queue", new AppealCreatedEvent(appeal.Id));

        Console.WriteLine($"{tag} THÀNH CÔNG — SV đã gửi đơn phúc khảo.");

        return (true, string.Empty, 200);
    }
}
