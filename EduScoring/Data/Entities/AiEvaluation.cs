using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Data.Entities
{
    [Table("AiEvaluations")]
    public class AiEvaluation
    {
        [Key]
        public int Id { get; set; }

        public Guid SubmissionId { get; set; }
        public int? RubricId { get; set; }

        public decimal AwardedScore { get; set; }

        [Required]
        public string AiFeedback { get; set; } = string.Empty;

        public bool IsHumanAdjusted { get; set; } = false;

        public DateTimeOffset EvaluatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Submission Submission { get; set; } = null!;
        public Rubric? Rubric { get; set; }
    }
}
