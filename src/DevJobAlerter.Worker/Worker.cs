using DevJobAlerter.Domain.Entities;
using DevJobAlerter.Domain.Interfaces;
using DevJobAlerter.Worker.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevJobAlerter.Worker;

// 1. Worker service class
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly INotificationService _notificationService;
    private readonly IJobService _jobService;
    private readonly JobSearchSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;

    // Constructor
    public Worker(
        ILogger<Worker> logger, 
        INotificationService notificationService, 
        IJobService jobService,
        IOptions<JobSearchSettings> options,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _notificationService = notificationService;
        _jobService = jobService;
        _settings = options.Value;
        _scopeFactory = scopeFactory;
    }

    // Override the ExecuteAsync method
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting DevJobAlerter Worker Service with search terms: {Terms}", string.Join(", ", _settings.SearchTerms));

        // Guard clauses
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

        // Configuring the check interval
        var checkInterval = TimeSpan.FromMinutes(_settings.SearchIntervalMinutes);

        // Main loop
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
                    var newJobs = new List<JobVacancy>();
                    
                    // Cria um escopo isolado para usar o repositório do banco de dados
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

                        // 1. Filtra as vagas verificando no SQLite se já foram enviadas antes
                        foreach (var job in jobs)
                        {
                            if (!await repository.ExistsAsync(job.Url))
                            {
                                newJobs.Add(job);
                            }
                        }

                        // 2. Se houver novas vagas, notifica e salva no banco
                        if (newJobs.Any())
                        {
                            await _notificationService.SendJobAlertAsync(_settings.TargetPhoneNumber, newJobs);

                            foreach (var job in newJobs)
                            {
                                await repository.AddAsync(new SentJob
                                {
                                    JobUrl = job.Url,
                                    Title = job.Title,
                                    Company = job.Company,
                                    SentAt = DateTime.UtcNow
                                });

                                _logger.LogInformation("Sent notification for job: {title} at {company}", job.Title, job.Company);
                            }

                            // Confirma as alterações no banco de dados SQLite
                            await repository.SaveChangesAsync();

                            _logger.LogInformation("Sent notification for {count} new job vacancies for term: {term}", newJobs.Count, term);
                        }
                        else
                        {
                            _logger.LogInformation("No new job vacancies found for search term: {term}", term);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking for new job vacancies.");
            }

            // Delay before the next cycle
            _logger.LogInformation("Job alert cycle completed. Waiting for {interval} before the next cycle...", checkInterval);

            // Wait for the check interval
            await Task.Delay(checkInterval, stoppingToken);
        }
    }
}