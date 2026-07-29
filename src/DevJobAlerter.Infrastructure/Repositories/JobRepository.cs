using DevJobAlerter.Domain.Entities;
using DevJobAlerter.Domain.Interfaces;
using DevJobAlerter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevJobAlerter.Infrastructure.Repositories;

// 1. Implementation of IJobRepository
public class JobRepository : IJobRepository
{
    // Dependency injection
    private readonly AppDbContext _context;

    // Constructor for dependency injection
    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    // Method to check if a job has already been sent
    public async Task<bool> ExistsAsync(string jobUrl)
    {
        return await _context.SentJobs.AnyAsync(j => j.JobUrl == jobUrl);
    }

    // Method to add a job to the database
    public async Task AddAsync(SentJob sentJob)
    {
        await _context.SentJobs.AddAsync(sentJob);
    }

    // Method to save changes to the database
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}