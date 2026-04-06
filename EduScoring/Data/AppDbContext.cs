using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EduScoring.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntry> TestEntries { get; set; }

}