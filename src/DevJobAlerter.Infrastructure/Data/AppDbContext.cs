using DevJobAlerter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevJobAlerter.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the DevJobAlerter application.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="AppDbContext"/> with the specified options.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the DbSet representing sent job records.
    /// </summary>
    public DbSet<SentJob> SentJobs { get; set; } = null!;

    /// <summary>
    /// Configures entity mappings and database constraints using Fluent API.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SentJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Ensures job URL uniqueness at the database level to prevent duplicate alerts
            entity.HasIndex(e => e.JobUrl).IsUnique(); 
        });
    }
}