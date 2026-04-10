using EduScoring.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Features.Exams.Models;

    [Table("Rubrics")]
    public class Rubric : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int ExamId { get; set; }

        [Required]
        public string CriteriaName { get; set; } = string.Empty;

        public decimal MaxScore { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public Exam Exam { get; set; } = null!;
    }

