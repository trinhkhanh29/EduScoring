using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Data.Entities
{
    [Table("Exams")]
    public class Exam
    {
        [Key]
        public int Id { get; set; }

        public Guid? TeacherId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public User? Teacher { get; set; }

        public ICollection<Rubric> Rubrics { get; set; } = new List<Rubric>();
    }
}
