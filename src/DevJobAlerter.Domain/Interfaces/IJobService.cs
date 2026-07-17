using DevJobAlerter.Domain.Entities;

namespace DevJobAlerter.Domain.Interfaces;


/// 1. Interface defining the contract for fetching job vacancies.

public interface IJobService
{
    Task<List<JobVacancy>> GetRecentJobsAsync(string keyword);
}