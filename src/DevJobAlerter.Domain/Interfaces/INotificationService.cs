using DevJobAlerter.Domain.Entities;

namespace DevJobAlerter.Domain.Interfaces;

public interface INotificationService
{
    Task SendJobAlertAsync(string toPhoneNumber, List<JobVacancy> vacancies);
}