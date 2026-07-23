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
    private readonly string[] _searchTerms; // Store the search terms for job filtering
    private readonly HashSet<string> _sentJobIds = new();

    // Define the interval for checking new job vacancies (e.g., 1 hour).
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public Worker(
        ILogger<Worker> logger, 
        INotificationService notificationService, 
        IJobService jobService,
        string[] searchTerms) // Injected the search terms here
    {
        _logger = logger;
        _notificationService = notificationService;
        _jobService = jobService;
        _searchTerms = searchTerms;
    }

    // Core execution loop of the background worker.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting DevJobAlerter Worker Service with search terms: {Terms}", string.Join(", ", _searchTerms));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting job alert cycle at: {time}", DateTimeOffset.Now);

                // 2. Define the recipient's phone number
                var targetPhoneNumber = "+5511944017974"; 

                
                foreach (var term in _searchTerms)
                {
                    _logger.LogInformation("Checking for new job vacancies with search term: {term}", term);

                    // Fetch job vacancies based on the search term
                    var jobs = await _jobService.GetRecentJobsAsync(term);

                    var newJobs = jobs.Where(job => !_sentJobIds.Contains(job.Url)).ToList();

                    if (newJobs.Any())
                    {
                        // Send notification for the new job vacancies
                        await _notificationService.SendJobAlertAsync(targetPhoneNumber, newJobs);
                        
                        foreach (var job in newJobs)
                        {
                            _sentJobIds.Add(job.Url); // Mark the job as sent
                            
                            // Send notification for each job vacancy found
                            _logger.LogInformation("Sent notification for job: {title} at {company}", job.Title, job.Company);
                        }
                        
                        _logger.LogInformation("Sent notification for {count} new job vacancies.", newJobs.Count);
                    }
                    else
                    {
                        _logger.LogInformation("No new job vacancies found for search term: {term}", term);
                    }
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