using DevJobAlerter.Domain.Entities;

namespace DevJobAlerter.Domain.Interfaces;

/// <summary>
/// Defines the contract for persisting and checking sent job records.
/// </summary>
public interface IJobRepository
{
    /// <summary>
    /// Checks if a job vacancy URL has already been processed and sent.
    /// </summary>
    Task<bool> ExistsAsync(string jobUrl);

    /// <summary>
    /// Adds a new sent job record to the repository tracking.
    /// </summary>
    Task AddAsync(SentJob sentJob);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}