using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;

namespace EduScoring.Features.Submissions.Features.GetSubmissionDetail;

public class GetSubmissionDetailQueryHandler
{
    private readonly AppReadDbContext _db;

    public GetSubmissionDetailQueryHandler(AppReadDbContext db)
    {
        _db = db;
    }

    public async Task<(bool IsSuccess, SubmissionDetailDto? Data, string ErrorMessage, int StatusCode)> Handle(GetSubmissionDetailQuery query)
    {
        var tag = $"[GetSubmissionDetail | EntityId={query.SubmissionId}]";

        var submissionData = await _db.Submissions
            .Where(s => s.Id == query.SubmissionId)
            .Select(s => new
            {
                s.Id,
                ExamTitle = s.Exam.Title,
                s.StudentId,
                TeacherId = s.Exam.TeacherId, // Lấy TeacherId
                s.FinalScore,
                s.LatestAiScore,
                s.Status,
                Images = s.Images.Select(i => new ImageDto(i.Id, i.ImageUrl, i.UploadedAt)).ToList(),
                Evaluations = s.Evaluations.Select(e => new EvaluationDto(e.Id, e.TotalScore, e.OverallFeedback, e.EvaluatedAt)).ToList()
            })
            .FirstOrDefaultAsync();

        if (submissionData == null)
        {
            return (false, null, "Không tìm thấy bài nộp.", 404);
        }

        bool isOwner = submissionData.StudentId == query.UserId;
        bool isTeacher = submissionData.TeacherId == query.UserId;
        bool isAdmin = query.IsAdmin;

        if (!(isAdmin || (query.Role == EduScoring.Common.Authentication.AppRoles.Teacher && isTeacher) || (query.Role == EduScoring.Common.Authentication.AppRoles.Student && isOwner)))
        {
            return (false, null, "Bạn không có quyền xem bài nộp này.", 403);
        }

        List<EvaluationDto>? evaluationsToReturn = submissionData.Evaluations;
        if (query.Role == EduScoring.Common.Authentication.AppRoles.Student && submissionData.Status != "Finalized")
        {
            evaluationsToReturn = null;
        }

        var dto = new SubmissionDetailDto(
            submissionData.Id,
            submissionData.ExamTitle,
            submissionData.StudentId,
            submissionData.FinalScore,
            submissionData.LatestAiScore,
            submissionData.Status,
            submissionData.Images,
            evaluationsToReturn
        );

        Console.WriteLine($"{tag} THÀNH CÔNG — Role: {query.Role} | Status: {submissionData.Status}");

        return (true, dto, string.Empty, 200);
    }
}