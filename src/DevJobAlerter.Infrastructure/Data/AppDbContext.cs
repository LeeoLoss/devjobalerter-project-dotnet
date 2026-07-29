using System.Reflection.Metadata;
using DevJobAlerter.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace DevJobAlerter.Infrastructure.Data;

// 1. Database context for the application
public class AppDbContext : DbContext
{
    // Constructor for dependency injection
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Database table
    public DbSet<SentJob> SentJobs { get; set; } = null!;

    // Fluent API
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SentJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JobUrl).IsUnique(); // No duplicates for the same job URL
        });
    }
}
