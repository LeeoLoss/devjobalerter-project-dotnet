using DevJobAlerter.Domain.Entities;

namespace DevJobAlerter.Domain.Interfaces;

/// <summary>
/// Defines the contract for fetching job vacancies from external providers.
/// </summary>
public interface IJobService
{
    Task<List<JobVacancy>> GetRecentJobsAsync(string keyword);
}