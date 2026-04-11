using EduScoring.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduScoring.Features.Auth.Models
{
    [Table("UserTokens")]
    public class UserToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required, StringLength(100)]
        public string LoginProvider { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        public DateTimeOffset? ExpiresAt { get; set; }

        public User User { get; set; } = null!;
    }
}
