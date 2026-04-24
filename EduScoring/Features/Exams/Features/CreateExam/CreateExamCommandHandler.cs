using EduScoring.Features.Exams.Models;
using EduScoring.Data.Entities;
using EduScoring.Infrastructure;
using MediatR;

namespace EduScoring.Features.Exams.Features.CreateExam;

public class CreateExamCommandHandler
{
    private readonly AppDbContext _db;

    public CreateExamCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(bool IsSuccess, CreateExamResponse? Data, string ErrorMessage)> Handle(CreateExamCommand command)
    {
        // 0. Validate cơ bản
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return (false, null, "Tiêu đề đề thi không được để trống!");
        }

        // 1. Khởi tạo Entity Exam
        var exam = new Exam
        {
            Title = command.Title,
            Description = command.Description,
            TeacherId = command.TeacherId,
            AllowStudentSubmission = command.AllowStudentSubmission,
            RequireTeacherReview = command.RequireTeacherReview,
            AllowAppeal = command.AllowAppeal,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // 2. Lưu xuống DB
        _db.Exams.Add(exam);
        await _db.SaveChangesAsync();

        Console.WriteLine($"[CreateExam] THÀNH CÔNG — ExamId: {exam.Id} | CreatedBy: {exam.TeacherId}");

        return (true, new CreateExamResponse(exam.Id, "Tạo đề thi thành công!"), string.Empty);
    }
}