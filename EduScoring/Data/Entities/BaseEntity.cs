namespace EduScoring.Data.Entities;

public abstract class BaseEntity
{
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }
}