using DevJobAlerter.Domain.Entities;

namespace DevJobAlerter.Domain.Interfaces;

public interface IJobRepository
{
    Task<bool> ExistsAsync(string jobUrl); // Method to check if a job has already been sent
    Task AddAsync(SentJob sentJob); // Method to add a job to the database
    Task SaveChangesAsync(CancellationToken cancellationToken = default); // Method to save changes to the database
}