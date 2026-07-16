using DevJobAlerter.Domain.Entities;
using DevJobAlerter.Domain.Interfaces;
using Microsoft.Extensions.Hosting; // Adicionado para reconhecer o BackgroundService
using Microsoft.Extensions.Logging;

namespace DevJobAlerter.Worker;

// Background service that runs continuously to check for new job vacancies and send alerts.
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly INotificationService _notificationService;

    // Define the interval for checking new job vacancies (e.g., 1 hour).
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public Worker(ILogger<Worker> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    // Core execution loop of the background worker.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting job alert cycle at: {time}", DateTimeOffset.Now);

                // 1. Fetch vacancies (Using mock/test data for now)
                var vacancies = GetMockVacancies();

                // 2. Define the recipient`s phone number (configured with country code, e.g., "+351912345678" or "+5511999999999")
                // For safety and testing, we can hardcore or later pull this from configuration
                var targetPhoneNumber = "+5511944017974"; // Replace with the actual phone number

                // 3. Trigger the WhatsApp Alert
                _logger.LogInformation("Found {count} new job vacancies. Sending alerts...", vacancies.Count);
                await _notificationService.SendJobAlertAsync(targetPhoneNumber, vacancies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking for new job vacancies.");
            }

            // CORREÇÃO 1: Alterado de _executionInterval para _checkInterval
            _logger.LogInformation("Job alert cycle completed. Waiting for {interval} before the next cycle...", _checkInterval);

            // Pause the execution for the defined interval before the next cycle.
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    /// Temporary helper method to generate mock job vacancies for end-to-end testing.
    private List<JobVacancy> GetMockVacancies()
    {
        return new List<JobVacancy>
        {
            new JobVacancy
            {
                Title = "Junior C# .NET Developer",
                Company = "Gerdau S.A.",
                Location = "Remote (Portugal or Brazil)",
                Url = "https://www.gerdau.com/careers"
            },
            new JobVacancy
            {
                Title = "Backend Engineer Intern",
                Company = "Stefanini",
                Location = "New York, NY / Hybrid",
                Url = "https://careers.stefanini.com/jobs/"
            } 
        }; 
    }
}