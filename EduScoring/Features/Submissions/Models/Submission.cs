using EduScoring.Data.Entities;
using EduScoring.Features.Exams.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Features.Submissions.Models;

[Table("Submissions")]
public class Submission : BaseEntity
{
    // ==========================================
    // 1. Keys & Identity
    // ==========================================
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public int ExamId { get; set; }

    public Guid? StudentId { get; set; }

    // ==========================================
    // 2. Scoring Details
    // ==========================================
    public decimal? FinalScore { get; set; }
    public decimal? LatestAiScore { get; set; }
    public decimal? HumanScore { get; set; }

    // public decimal? TotalScore { get; set; }//

    // ==========================================
    // 3. LUỒNG NGHIỆP VỤ & TRẠNG THÁI
    // ==========================================
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    public string CreatedSource { get; set; } = "Teacher";
    public string SubmissionMode { get; set; } = "TeacherUpload";

    public bool IsLocked { get; set; } = false;

    // ==========================================
    // 4. LỊCH SỬ & DỮ LIỆU OCR
    // ==========================================
    public int EvaluationCount { get; set; }

    [StringLength(50)]
    public string LastEvaluationTrigger { get; set; } = "Auto";

    [StringLength(10)]
    public string Language { get; set; } = "vi";

    public string? CombinedOcrText { get; set; }

    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

    // ==========================================
    // 5. EF CORE NAVIGATION PROPERTIES
    // ==========================================
    public Exam Exam { get; set; } = null!;
    public User? Student { get; set; }

    public ICollection<SubmissionImage> Images { get; set; } = new List<SubmissionImage>();
    public ICollection<AiEvaluation> Evaluations { get; set; } = new List<AiEvaluation>();
    public ICollection<HumanEvaluation> HumanEvaluations { get; set; } = new List<HumanEvaluation>();
}