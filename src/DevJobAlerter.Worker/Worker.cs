using DevJobAlerter.Domain.Entities;
using DevJobAlerter.Domain.Interfaces;
using DevJobAlerter.Worker.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevJobAlerter.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger; // Logger for logging information and errors
    private readonly INotificationService _notificationService; // Service to send job alerts
    private readonly IJobService _jobService; // Service to fetch job vacancies
    private readonly JobSearchSettings _settings; // Settings from appsettings.json
    private readonly HashSet<string> _sentJobIds = new();


    // 1. Constructor to inject dependencies and settings
    public Worker(
        ILogger<Worker> logger, 
        INotificationService notificationService, 
        IJobService jobService,
        IOptions<JobSearchSettings> options)
    {
        _logger = logger;
        _notificationService = notificationService;
        _jobService = jobService;
        _settings = options.Value;
    }

    // 2. The main execution loop of the worker service
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting DevJobAlerter Worker Service with search terms: {Terms}", string.Join(", ", _settings.SearchTerms));

        if (string.IsNullOrWhiteSpace(_settings.TargetPhoneNumber))
        {
            _logger.LogError("Target phone number is not configured in appsettings or User Secrets.");
            return;
        }

        if (!_settings.SearchTerms.Any())
        {
            _logger.LogWarning("No search terms configured in JobSearchSettings.");
            return;
        }

        var checkInterval = TimeSpan.FromMinutes(_settings.SearchIntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting job alert cycle at: {time}", DateTimeOffset.Now);

                foreach (var term in _settings.SearchTerms)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    _logger.LogInformation("Checking for new job vacancies with search term: {term}", term);

                    var jobs = await _jobService.GetRecentJobsAsync(term);
                    var newJobs = jobs.Where(job => !_sentJobIds.Contains(job.Url)).ToList();

                    if (newJobs.Any())
                    {
                        await _notificationService.SendJobAlertAsync(_settings.TargetPhoneNumber, newJobs);
                        
                        foreach (var job in newJobs)
                        {
                            _sentJobIds.Add(job.Url);
                            _logger.LogInformation("Sent notification for job: {title} at {company}", job.Title, job.Company);
                        }
                        
                        _logger.LogInformation("Sent notification for {count} new job vacancies for term: {term}", newJobs.Count, term);
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

            _logger.LogInformation("Job alert cycle completed. Waiting for {interval} before the next cycle...", checkInterval);

            await Task.Delay(checkInterval, stoppingToken);
        }
    }
}