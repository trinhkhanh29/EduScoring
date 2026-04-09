using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Data.Entities
{
    [Table("Submissions")]
    public class Submission : BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int ExamId { get; set; }
        public Guid? StudentId { get; set; }

        public decimal? TotalScore { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

        public Exam Exam { get; set; } = null!;
        public User? Student { get; set; }

        public ICollection<SubmissionImage> Images { get; set; } = new List<SubmissionImage>();
        public ICollection<AiEvaluation> Evaluations { get; set; } = new List<AiEvaluation>();
    }
}
