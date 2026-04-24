using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Features.Submissions.Models
{
    [Table("HumanEvaluations")]
    public class HumanEvaluation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SubmissionId { get; set; }

        public decimal TeacherScore { get; set; }
        public string TeacherFeedback { get; set; } = string.Empty;

        public DateTimeOffset EvaluatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Submission Submission { get; set; } = null!;
    }
}
