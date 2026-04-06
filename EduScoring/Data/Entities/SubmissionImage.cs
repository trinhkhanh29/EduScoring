using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Data.Entities
{
    [Table("SubmissionImages")]
    public class SubmissionImage
    {
        [Key]
        public int Id { get; set; }

        public Guid SubmissionId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public int PageNumber { get; set; } = 1;

        public string? OcrRawText { get; set; }

        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

        public Submission Submission { get; set; } = null!;
    }
}
