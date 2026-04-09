using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Data.Entities;

[Table("ActivityLogs")]
public class ActivityLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Mã của người thực hiện hành động (Teacher, Admin, hoặc chính Student)
    public Guid UserId { get; set; }

    [StringLength(100)]
    public string ActionType { get; set; } = string.Empty; // VD: "UPLOAD_SUBMISSION", "EDIT_SCORE"

    // Tên bảng hoặc Entity bị tác động (VD: "Submissions", "Users")
    [StringLength(50)]
    public string EntityName { get; set; } = string.Empty;

    [StringLength(50)]
    public string EntityId { get; set; } = string.Empty; // ID của bài nộp hoặc ảnh vừa lưu

    // Chứa chi tiết dạng JSON nếu cần (Ví dụ: lưu IP máy tính, hoặc trước khi sửa điểm là mấy, sau khi sửa là mấy)
    public string? Details { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;
}