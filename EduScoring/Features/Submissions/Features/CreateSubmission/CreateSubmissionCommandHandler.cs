using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;
using EduScoring.Features.Exams.Models;
using EduScoring.Common.Messaging;

namespace EduScoring.Features.Submissions.Features.CreateSubmission;

public class CreateSubmissionCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IRabbitMQService _rabbitMQ;

    public CreateSubmissionCommandHandler(AppDbContext db, IRabbitMQService rabbitMQ)
    {
        _db = db;
        _rabbitMQ = rabbitMQ;
    }

    public async Task<(bool IsSuccess, CreateSubmissionResponse? Data, string ErrorMessage, int StatusCode)> Handle(CreateSubmissionCommand command)
    {
        var exam = await _db.Exams.FirstOrDefaultAsync(e => e.Id == command.ExamId);
        if (exam == null)
        {
            return (false, null, "Không tìm thấy đề thi.", 404);
        }

        if (command.CreatedSource == "StudentSelfSubmit" && !exam.AllowStudentSubmission)
        {
            return (false, null, "Đề thi này không cho phép sinh viên tự nộp bài.", 403);
        }

        Guid studentId = command.TargetStudentId ?? command.UserId;

        var submission = new Submission
        {
            ExamId = command.ExamId,
            StudentId = studentId,
            Status = "Pending",
            SubmissionMode = command.CreatedSource
        };

        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync();

        // Publish SubmissionCreatedEvent
        await _rabbitMQ.PublishAsync("submission_created_queue", new SubmissionCreatedEvent(submission.Id));

        Console.WriteLine($"[CreateSubmission | EntityId={submission.Id}] THÀNH CÔNG — Source: {command.CreatedSource}");

        return (true, new CreateSubmissionResponse(submission.Id, "Tạo bài nộp thành công!"), string.Empty, 201);
    }
}