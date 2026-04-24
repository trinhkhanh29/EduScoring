using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;

namespace EduScoring.Features.Submissions.Features.ReviewAiEvaluation;

public class ReviewAiEvaluationCommandHandler
{
    private readonly AppDbContext _db;

    public ReviewAiEvaluationCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(bool IsSuccess, string ErrorMessage, int StatusCode)> Handle(ReviewAiEvaluationCommand command)
    {
        var tag = $"[ReviewAiEvaluation | EntityId={command.SubmissionId}]";

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
            return (false, "Bạn không có quyền duyệt bài nộp này.", 403);
        }

        if (submission.LatestAiScore == null)
        {
            return (false, "AI chưa chấm xong, không thể duyệt.", 400);
        }

        submission.FinalScore = submission.HumanScore ?? submission.LatestAiScore;
        var source = submission.HumanScore.HasValue ? "Human" : "AI";
        Console.WriteLine($"[ReviewAiEvaluation] Ưu tiên lấy điểm từ: {source}");

        submission.Status = "Reviewed";

        await _db.SaveChangesAsync();

        Console.WriteLine($"{tag} THÀNH CÔNG — Điểm chốt: {submission.FinalScore}");

        return (true, "Đã duyệt điểm AI thành công.", 200);
    }
}