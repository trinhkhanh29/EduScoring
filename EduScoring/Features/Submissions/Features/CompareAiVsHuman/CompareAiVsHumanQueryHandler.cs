using EduScoring.Features.Submissions.Models;
using EduScoring.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduScoring.Features.Submissions.Features.CompareAiVsHuman;

public class CompareAiVsHumanQueryHandler
{
    private readonly AppDbContext _db;

    public CompareAiVsHumanQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(bool IsSuccess, CompareResultDto? Data, string ErrorMessage, int StatusCode)> Handle(CompareAiVsHumanQuery query)
    {
        var exam = await _db.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == query.ExamId);
        if (exam == null)
            return (false, null, "Không tìm thấy đề thi.", 404);

        if (!query.IsAdmin && exam.TeacherId != query.UserId)
            return (false, null, "Bạn không có quyền xem thống kê này.", 403);

        var validSubmissions = await _db.Submissions
            .AsNoTracking()
            .Where(s => s.ExamId == query.ExamId && s.LatestAiScore.HasValue && s.HumanScore.HasValue)
            .ToListAsync();

        var totalEvaluated = validSubmissions.Count;

        if (totalEvaluated == 0)
        {
            return (true, new CompareResultDto(0, 0, 0, new List<DataPointDto>()), string.Empty, 200);
        }

        var totalError = validSubmissions.Sum(s =>
            Math.Abs(s.LatestAiScore!.Value - s.HumanScore!.Value));

        var meanAbsoluteError = totalError / totalEvaluated;

        var accurateCount = validSubmissions.Count(s =>
            Math.Abs(s.LatestAiScore!.Value - s.HumanScore!.Value) <= 0.5m);

        var accuracyPercent = ((decimal)accurateCount / totalEvaluated) * 100;

        var dataPoints = validSubmissions.Select(s => new DataPointDto(
            s.Id,
            s.LatestAiScore!.Value,
            s.HumanScore!.Value
        )).ToList();

        return (true,
            new CompareResultDto(
                totalEvaluated,
                (double)meanAbsoluteError,
                (double)accuracyPercent,
                dataPoints
            ),
            string.Empty,
            200
        );
    }
}
