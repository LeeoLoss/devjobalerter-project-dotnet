using DevJobAlerter.Domain.Entities;
using DevJobAlerter.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DevJobAlerter.Worker;

// Background service that runs continuously to check for new job vacancies and send alerts.
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly INotificationService _notificationService;
    private readonly IJobService _jobService; // Added to fetch real jobs

    // Define the interval for checking new job vacancies (e.g., 1 hour).
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public Worker(
        ILogger<Worker> logger, 
        INotificationService notificationService, 
        IJobService jobService) // Injected the IJobService here
    {
        _logger = logger;
        _notificationService = notificationService;
        _jobService = jobService;
    }

    // Core execution loop of the background worker.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting job alert cycle at: {time}", DateTimeOffset.Now);

                // 1. Fetch REAL vacancies from the Adzuna API
                var vacancies = await _jobService.GetRecentJobsAsync(".NET");

                // 2. Define the recipient's phone number
                var targetPhoneNumber = "+5511944017974"; 

                // 3. Trigger the WhatsApp Alert only if we actually found vacancies
                if (vacancies.Any())
                {
                    _logger.LogInformation("Found {count} new job vacancies. Sending alerts...", vacancies.Count);
                    await _notificationService.SendJobAlertAsync(targetPhoneNumber, vacancies);
                }
                else
                {
                    _logger.LogWarning("No vacancies were found during this cycle. Skipping notification.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking for new job vacancies.");
            }

            _logger.LogInformation("Job alert cycle completed. Waiting for {interval} before the next cycle...", _checkInterval);

            // Pause the execution for the defined interval before the next cycle.
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}