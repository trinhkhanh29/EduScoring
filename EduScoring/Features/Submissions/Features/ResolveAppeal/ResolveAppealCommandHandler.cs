using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;

namespace EduScoring.Features.Submissions.Features.ResolveAppeal;

public class ResolveAppealCommandHandler
{
    private readonly AppDbContext _db;

    public ResolveAppealCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(bool IsSuccess, string ErrorMessage, int StatusCode)> Handle(ResolveAppealCommand command)
    {
        var tag = $"[ResolveAppeal | AppealId={command.AppealId}]";

        var appeal = await _db.Appeals
            .Include(a => a.Submission)
            .ThenInclude(s => s.Exam)
            .FirstOrDefaultAsync(a => a.Id == command.AppealId);

        if (appeal == null)
        {
            return (false, "Không tìm thấy đơn phúc khảo.", 404);
        }

        if (appeal.Status != "Pending")
        {
            return (false, "Đơn này đã được xử lý rồi.", 400);
        }

        bool isAuthorized = command.IsAdmin || (appeal.Submission?.Exam != null && appeal.Submission.Exam.TeacherId == command.UserId);
        if (!isAuthorized)
        {
            return (false, "Bạn không có quyền xử lý đơn phúc khảo này.", 403);
        }

        // CHỐT CHẶN 3: Nếu duyệt mà không có điểm mới
        if (command.IsAccepted && !command.NewScore.HasValue)
        {
            return (false, "Chấp nhận phúc khảo thì phải nhập điểm mới.", 400);
        }

        // Lưu điểm cũ
        appeal.PreviousScore = appeal.Submission?.FinalScore;
        appeal.ResolvedBy = command.UserId;
        appeal.ResolvedAt = DateTimeOffset.UtcNow;
        appeal.TeacherResponse = command.TeacherFeedback;

        if (command.IsAccepted)
        {
            // Audit: Lưu lịch sử chấm điểm phúc khảo
            var audit = new HumanEvaluation
            {
                Id = Guid.NewGuid(),
                SubmissionId = appeal.SubmissionId,
                TeacherScore = command.NewScore!.Value,
                TeacherFeedback = "[Phúc khảo] " + command.TeacherFeedback,
                EvaluatedAt = DateTimeOffset.UtcNow
            };
            _db.HumanEvaluations.Add(audit);

            appeal.ResolutionType = "Approved";
            appeal.NewScore = command.NewScore;
            appeal.Status = "Resolved";
            if (appeal.Submission != null)
            {
                appeal.Submission.FinalScore = command.NewScore;
            }
        }
        else
        {
            appeal.ResolutionType = "Rejected";
            appeal.NewScore = null;
            appeal.Status = "Resolved";
        }

        await _db.SaveChangesAsync();

        Console.WriteLine($"{tag} THÀNH CÔNG — Kết quả: {appeal.ResolutionType} | Lịch sử điểm: {appeal.PreviousScore} -> {appeal.NewScore}");

        return (true, $"Đã xử lý phúc khảo: {appeal.ResolutionType}", 200);
    }
}
