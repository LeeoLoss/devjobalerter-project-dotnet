using System.Text;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevJobAlerter.Domain.Entities;
using DevJobAlerter.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DevJobAlerter.Infrastructure.Services;

// 1. Service responsible for fetching job vacancies from the Adzuna API.
// It implements the IJobService interface from the Domain layer.
public class AdzunaJobService : IJobService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AdzunaJobService> _logger;
    private readonly string _appId;
    private readonly string _appKey;


    public AdzunaJobService(HttpClient httpClient, ILogger<AdzunaJobService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configuring retrievies the Adzuna API credentials from User Secrets.
        _appId = configuration["Adzuna:AppId"] ?? throw new ArgumentNullException("Adzuna: App ID is not configured.");
        _appKey = configuration["Adzuna:AppKey"] ?? throw new ArgumentNullException("Adzuna: App Key is not configured.");
    }

    // 2. Fetches recent job vacancies from the Adzuna API based on a keyword.
    public async Task<List<JobVacancy>> GetRecentJobsAsync(string keyword)
    {
        try
        {
            _logger.LogInformation("Searching for real jobs on Adzuna with keyword: '{keyword}'...", keyword);
        
            // Build the API URL targeting Brazil (br), limiting results, and sorting by publication date
            var url = $"https://api.adzuna.com/v1/api/jobs/br/search/1" +
                      $"?app_id={_appId}" +
                      $"&app_key={_appKey}" +
                      $"&what={Uri.EscapeDataString(keyword)}" +
                      $"&results_per_page=5" +
                      $"&sort_by=date";

            // 1. Get the HTTP response first
            var responseMessage = await _httpClient.GetAsync(url);
            responseMessage.EnsureSuccessStatusCode();

            // 2. Read raw response content as a byte array to bypass the invalid 'utf8' charset validation
            var rawBytes = await responseMessage.Content.ReadAsByteArrayAsync();
            
            // 3. Decode the bytes manually using standard UTF-8 encoding
            var rawJson = Encoding.UTF8.GetString(rawBytes);

            // 4. Deserialize manually using System.Text.Json
            var response = JsonSerializer.Deserialize<AdzunaResponse>(rawJson);

            if (response?.Results == null || response.Results.Count == 0)
            {
                _logger.LogWarning("No jobs found for keyword: '{keyword}'", keyword);
                return new List<JobVacancy>();
            }

            // Map the Adzuna API response contracts to the Domain entity (JobVacancy)
            var vacancies = response.Results.Select(r => new JobVacancy
            {
                Title = r.Title?.Trim() ?? "No Title",
                Company = r.Company?.DisplayName?.Trim() ?? "No Company",
                Location = r.Location?.DisplayName?.Trim() ?? "No Location",
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

// 3. Auxiliary DTO classes to deserialize the Adzuna API JSON response payload
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