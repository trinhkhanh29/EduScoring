using EduScoring.Features.Exams.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace EduScoring.Features.Submissions.Models
{
    [Table("AiEvaluations")]
    public class AiEvaluation
    {
        [Key]
        public int Id { get; set; }

        public Guid SubmissionId { get; set; }
        public int? RubricId { get; set; }

        // Đã xóa AwardedScore và AiFeedback bị lặp logic

        public bool IsHumanAdjusted { get; set; } = false;

        public DateTimeOffset EvaluatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Submission Submission { get; set; } = null!;
        public Rubric? Rubric { get; set; }

        public double TotalScore { get; set; }
        public string OverallFeedback { get; set; } = string.Empty;

        public double ConfidenceScore { get; set; }
        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        [StringLength(100)]
        public string ModelName { get; set; } = string.Empty;
        [StringLength(50)]
        public string PromptVersion { get; set; } = string.Empty;

        // ĐÃ ĐỔI: Dùng JsonDocument thay vì string cho chuẩn EF Core + Postgres
        [Column(TypeName = "jsonb")]
        public JsonDocument? RawResponse { get; set; }

        public ICollection<AiEvaluationDetail> Details { get; set; } = new List<AiEvaluationDetail>();
    }
}
