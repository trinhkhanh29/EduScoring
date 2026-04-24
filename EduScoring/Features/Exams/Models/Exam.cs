using EduScoring.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Features.Exams.Models;

[Table("Exams")]
    public class Exam : BaseEntity
    {
        // ==========================================
        // 1. Keys
        // ==========================================
        [Key]
        public int Id { get; set; }

        public Guid? TeacherId { get; set; }

        // ==========================================
        // 2. Basic Info
        // ==========================================
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // ==========================================
        // 3. Exam Policies
        // Khu vực này quyết định luồng chạy (Workflow) của toàn bộ bài nộp bên trong đề thi này
        // ==========================================
        public bool AllowStudentSubmission { get; set; } = false;
        public bool RequireTeacherReview { get; set; } = true;
        public bool AllowAppeal { get; set; } = true;

        // ==========================================
        // 4. EF CORE NAVIGATION PROPERTIES
        // ==========================================
        public User? Teacher { get; set; }
        public ICollection<Rubric> Rubrics { get; set; } = new List<Rubric>();
}
