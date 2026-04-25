using Microsoft.EntityFrameworkCore;

namespace EduScoring.Infrastructure;

public class AppReadDbContext : AppDbContext
{
    public AppReadDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }
}