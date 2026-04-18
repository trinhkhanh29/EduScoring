using EduScoring.Features.Submissions.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Data.Entities
{
    [Table("Appeals")]
    public class Appeal : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public Guid SubmissionId { get; set; }

        [Required]
        public string StudentReason { get; set; } = string.Empty;

        public string? TeacherResponse { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Open";

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Submission Submission { get; set; } = null!;
    }
}
