using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;

namespace EduScoring.Features.Submissions.Features.GetStudentResults;

public class GetStudentResultsQueryHandler
{
    private readonly AppReadDbContext _db;

    public GetStudentResultsQueryHandler(AppReadDbContext db)
    {
        _db = db;
    }

    public async Task<List<StudentResultDto>> Handle(GetStudentResultsQuery query)
    {

        var results = await _db.Submissions
            .Where(s => s.StudentId == query.StudentId)
            .Select(s => new StudentResultDto(
                s.Id,
                s.Exam.Title,
                s.SubmittedAt,
                s.Status,
                s.Status == "Finalized" ? s.FinalScore : null
            ))
            .ToListAsync();

        return results;
    }
}
