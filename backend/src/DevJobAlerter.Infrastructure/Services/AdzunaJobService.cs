using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevJobAlerter.Domain.Entities;
using DevJobAlerter.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DevJobAlerter.Infrastructure.Services;

/// <summary>
/// Service responsible for fetching job vacancies from the Adzuna REST API.
/// Implements the IJobService interface from the Domain layer.
/// </summary>
public class AdzunaJobService : IJobService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AdzunaJobService> _logger;
    private readonly string _appId;
    private readonly string _appKey;

    public AdzunaJobService(
        HttpClient httpClient, 
        ILogger<AdzunaJobService> logger, 
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;

        _appId = configuration["Adzuna:AppId"] ?? throw new ArgumentNullException(nameof(configuration), "Adzuna:AppId is not configured.");
        _appKey = configuration["Adzuna:AppKey"] ?? throw new ArgumentNullException(nameof(configuration), "Adzuna:AppKey is not configured.");
    }

    /// <summary>
    /// Fetches recent job vacancies from Adzuna based on a search keyword.
    /// </summary>
    public async Task<List<JobVacancy>> GetRecentJobsAsync(string keyword)
    {
        try
        {
            _logger.LogInformation("Searching for jobs on Adzuna with keyword: '{keyword}'...", keyword);

            var url = $"https://api.adzuna.com/v1/api/jobs/br/search/1" +
                      $"?app_id={_appId}" +
                      $"&app_key={_appKey}" +
                      $"&what={Uri.EscapeDataString(keyword)}" +
                      $"&results_per_page=5" +
                      $"&sort_by=date";

            var responseMessage = await _httpClient.GetAsync(url);
            responseMessage.EnsureSuccessStatusCode();

            // Read raw bytes to handle non-standard encoding response headers smoothly
            var rawBytes = await responseMessage.Content.ReadAsByteArrayAsync();
            var rawJson = Encoding.UTF8.GetString(rawBytes);

            var response = JsonSerializer.Deserialize<AdzunaResponse>(rawJson);

            if (response?.Results == null || response.Results.Count == 0)
            {
                _logger.LogWarning("No jobs found for keyword: '{keyword}'", keyword);
                return new List<JobVacancy>();
            }

            var vacancies = response.Results.Select(r => new JobVacancy
            {
                Title = r.Title?.Trim() ?? "No Title Specified",
                Company = r.Company?.DisplayName?.Trim() ?? "No Company Specified",
                Location = r.Location?.DisplayName?.Trim() ?? "No Location Specified",
                Url = r.RedirectUrl ?? "https://www.adzuna.com.br"
            }).ToList();

            _logger.LogInformation("Successfully found {count} jobs for keyword: '{keyword}'", vacancies.Count, keyword);
            return vacancies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching jobs from Adzuna for keyword: '{keyword}'", keyword);
            return new List<JobVacancy>();
        }
    }
}

#region API Response Contracts

public class AdzunaResponse
{
    [JsonPropertyName("results")]
    public List<AdzunaJobResult> Results { get; set; } = new();
}

public class AdzunaJobResult
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("company")]
    public AdzunaCompany? Company { get; set; }

    [JsonPropertyName("location")]
    public AdzunaLocation? Location { get; set; }

    [JsonPropertyName("redirect_url")]
    public string? RedirectUrl { get; set; }
}

public class AdzunaCompany
{
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}

public class AdzunaLocation
{
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}

#endregion