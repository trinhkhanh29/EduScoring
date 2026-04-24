using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;

namespace EduScoring.Features.Submissions.Features.OverrideScore;

public class OverrideScoreCommandHandler
{
    private readonly AppDbContext _db;

    public OverrideScoreCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(bool IsSuccess, string ErrorMessage, int StatusCode)> Handle(OverrideScoreCommand command)
    {
        var tag = $"[OverrideScore | EntityId={command.SubmissionId}]";

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
            return (false, "Bạn không có quyền sửa điểm bài nộp này.", 403);
        }

        var oldScore = submission.LatestAiScore?.ToString() ?? "N/A";

        submission.FinalScore = command.NewScore;
        submission.HumanScore = command.NewScore;
        submission.Status = "ScoreOverridden";

        await _db.SaveChangesAsync();

        Console.WriteLine($"{tag} THÀNH CÔNG — Điểm cũ: {oldScore} -> Điểm mới: {command.NewScore} | Lý do: {command.Reason}");

        return (true, string.Empty, 200);
    }
}