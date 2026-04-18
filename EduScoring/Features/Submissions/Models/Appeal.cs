using EduScoring.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Features.Submissions.Models
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

        public Guid? ResolvedBy { get; set; }
        public DateTimeOffset? ResolvedAt { get; set; }
        public string? ResolutionType { get; set; }
        public decimal? PreviousScore { get; set; }
        public decimal? NewScore { get; set; }

        public Submission Submission { get; set; } = null!;
    }
}
