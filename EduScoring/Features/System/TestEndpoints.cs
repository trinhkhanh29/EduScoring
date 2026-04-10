using EduScoring.Data.Entities;
using EduScoring.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.System;

public static class TestEndpoints
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder app)
    {
        // Test Database Connection
        app.MapGet("/test-db", async (AppDbContext dbContext) =>
        {
            try
            {
                bool canConnect = await dbContext.Database.CanConnectAsync();
                return canConnect
                    ? Results.Ok(new { status = "Thành công!", message = "Đã kết nối mượt mà tới Supabase PostgreSQL 🚀" })
                    : Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "Kết nối thất bại ❌");
            }
        }).WithTags("System Tests");

        // Test Submission Entity Creation
        app.MapPost("/create-test-submission", async (AppDbContext db) =>
        {
            var exam = await db.Exams.FirstOrDefaultAsync();

            if (exam == null)
            {
                exam = new Exam
                {
                    Title = "Đề thi NCKH Test",
                    Description = "Dùng để test tính năng upload ảnh",
                    CreatedAt = DateTimeOffset.UtcNow
                };
                db.Exams.Add(exam);
                await db.SaveChangesAsync();
            }

            var submission = new Submission
            {
                ExamId = exam.Id,
                StudentId = Guid.Empty
            };

            db.Submissions.Add(submission);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                Message = "Tạo Submission nháp thành công!",
                SubmissionId = submission.Id,
                ExamId = exam.Id
            });
        }).WithTags("System Tests");
    }
}