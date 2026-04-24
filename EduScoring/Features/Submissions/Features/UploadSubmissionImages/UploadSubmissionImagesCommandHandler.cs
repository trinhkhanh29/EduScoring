using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;
using EduScoring.Common.Messaging;

namespace EduScoring.Features.Submissions.Features.UploadSubmissionImages;

public class UploadSubmissionImagesCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IRabbitMQService _rabbitMQ;

    public UploadSubmissionImagesCommandHandler(AppDbContext db, IRabbitMQService rabbitMQ)
    {
        _db = db;
        _rabbitMQ = rabbitMQ;
    }

    public async Task<(bool IsSuccess, string Data, string ErrorMessage, int StatusCode)> Handle(UploadSubmissionImagesCommand command)
    {
        var tag = $"[UploadSubmissionImages | EntityId={command.SubmissionId}]";

        if (command.ImageUrls == null || !command.ImageUrls.Any())
        {
            return (false, string.Empty, "Danh sách URL ảnh không được để trống.", 400);
        }

        var submission = await _db.Submissions
            .Include(s => s.Exam)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId);

        if (submission == null)
        {
            return (false, string.Empty, "Không tìm thấy bài nộp.", 404);
        }

        if (submission.IsLocked)
        {
            Console.WriteLine($"{tag} THẤT BẠI — Cố gắng sửa đổi bài thi đã bị khóa.");
            return (false, string.Empty, "Bài thi đã bị khóa sổ, không thể thay đổi hoặc cập nhật dữ liệu!", 400);
        }

        bool isAuthorized = command.IsAdmin || 
                            submission.StudentId == command.UserId || 
                            (submission.Exam != null && submission.Exam.TeacherId == command.UserId);

        if (!isAuthorized)
        {
            return (false, string.Empty, "Bạn không có quyền tải ảnh cho bài nộp này.", 403);
        }

        var count = 0;
        foreach (var url in command.ImageUrls)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                _db.SubmissionImages.Add(new SubmissionImage
                {
                    SubmissionId = submission.Id,
                    ImageUrl = url,
                    UploadedAt = DateTimeOffset.UtcNow
                });
                count++;
            }
        }

        if (count == 0)
        {
            return (false, string.Empty, "Không tìm thấy URL hợp lệ trong danh sách.", 400);
        }

        submission.Status = "ImagesUploaded";

        await _db.SaveChangesAsync();

        await _rabbitMQ.PublishAsync("submission_images_uploaded_queue", new SubmissionImagesUploadedEvent(submission.Id));

        Console.WriteLine($"{tag} THÀNH CÔNG — {count} images uploaded");

        return (true, $"Đã upload thành công {count} ảnh.", string.Empty, 200);
    }
}