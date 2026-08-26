using DevJobAlerter.Domain.Entities;

namespace DevJobAlerter.Domain.Interfaces;

/// <summary>
/// Defines the contract for dispatching job notifications.
/// </summary>
public interface INotificationService
{
    Task SendJobAlertAsync(IEnumerable<JobVacancy> vacancies, CancellationToken cancellationToken = default);
}