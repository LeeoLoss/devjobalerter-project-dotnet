using DevJobAlerter.Domain.Entities;
using DevJobAlerter.Domain.Interfaces;
using DevJobAlerter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevJobAlerter.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing sent job records using Entity Framework Core.
/// Implements the IJobRepository interface from the Domain layer.
/// </summary>
public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Checks whether a job URL has already been stored in the database.
    /// </summary>
    public async Task<bool> ExistsAsync(string jobUrl)
    {
        return await _context.SentJobs.AnyAsync(j => j.JobUrl == jobUrl);
    }

    /// <summary>
    /// Adds a new sent job record to the DbContext tracking queue.
    /// </summary>
    public async Task AddAsync(SentJob sentJob)
    {
        await _context.SentJobs.AddAsync(sentJob);
    }

    /// <summary>
    /// Persists all tracked database changes asynchronously.
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}