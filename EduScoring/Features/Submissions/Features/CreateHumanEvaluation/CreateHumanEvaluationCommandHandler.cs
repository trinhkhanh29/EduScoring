using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;

namespace EduScoring.Features.Submissions.Features.CreateHumanEvaluation;

public class CreateHumanEvaluationCommandHandler
{
    private readonly AppDbContext _db;

    public CreateHumanEvaluationCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(bool IsSuccess, string ErrorMessage, int StatusCode)> Handle(CreateHumanEvaluationCommand command)
    {
        var tag = $"[CreateHumanEvaluation | EntityId={command.SubmissionId}]";

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
            return (false, "Bạn không có quyền chấm điểm đối chứng cho bài nộp này.", 403);
        }

        var eval = new HumanEvaluation
        {
            Id = Guid.NewGuid(),
            SubmissionId = command.SubmissionId,
            TeacherScore = command.TeacherScore,
            TeacherFeedback = command.TeacherFeedback,
            EvaluatedAt = DateTimeOffset.UtcNow
        };

        submission.HumanScore = command.TeacherScore;

        _db.HumanEvaluations.Add(eval);
        await _db.SaveChangesAsync();

        Console.WriteLine($"{tag} THÀNH CÔNG — GV đã chấm điểm đối chứng: {command.TeacherScore}");

        return (true, string.Empty, 200);
    }
}