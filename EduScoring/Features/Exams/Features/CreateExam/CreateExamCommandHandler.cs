using EduScoring.Features.Exams.Models;
using EduScoring.Data.Entities;
using EduScoring.Infrastructure;
using MediatR;

namespace EduScoring.Features.Exams.Features.CreateExam;

public class CreateExamCommandHandler
{
    private readonly AppDbContext _db;
    //private readonly IMediator _mediator;//

    public CreateExamCommandHandler(AppDbContext db, IMediator mediator)
    {
        _db = db;
        //_mediator = mediator;//
    }

    public async Task<(bool IsSuccess, CreateExamResponse? Data, string ErrorMessage)> Handle(CreateExamCommand command)
    {
        // 1. Khởi tạo Entity Exam
        var exam = new Exam
        {
            Title = command.Title,
            Description = command.Description,
            TeacherId = command.TeacherId,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        // 2. Lưu xuống DB
        _db.Exams.Add(exam);
        await _db.SaveChangesAsync();

        // 3. (Optional) Bắn Event nếu cần
        // await _mediator.Publish(new ExamCreatedEvent(exam.Id, exam.Title));//

        return (true, new CreateExamResponse(exam.Id, "Tạo đề thi thành công!"), string.Empty);
    }
}