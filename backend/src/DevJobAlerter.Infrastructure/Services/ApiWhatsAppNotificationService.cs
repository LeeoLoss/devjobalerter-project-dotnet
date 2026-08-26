using System.Net.Http.Json;
using System.Text;
using DevJobAlerter.Domain.Entities;
using DevJobAlerter.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DevJobAlerter.Infrastructure.Services;

/// <summary>
/// Service responsible for sending job alert notifications via WhatsApp API.
/// Implements the INotificationService interface from the Domain layer.
/// </summary>
public class ApiWhatsAppNotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiWhatsAppNotificationService> _logger;
    private readonly IConfiguration _configuration;

    public ApiWhatsAppNotificationService(
        HttpClient httpClient,
        ILogger<ApiWhatsAppNotificationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Formats and dispatches a list of job vacancies via WhatsApp HTTP API.
    /// </summary>
    public async Task SendJobAlertAsync(IEnumerable<JobVacancy> vacancies, CancellationToken cancellationToken = default)
    {
        var jobList = vacancies?.ToList() ?? new List<JobVacancy>();

        if (!jobList.Any())
        {
            _logger.LogWarning("No job vacancies to send in the WhatsApp notification.");
            return;
        }

        var baseUrl = (_configuration["WhatsApp:ApiUrl"] ?? "http://whatsapp-api:8080").TrimEnd('/');
        var apiKey = _configuration["WhatsApp:ApiKey"] ?? "akxp0209";
        var rawPhoneNumber = _configuration["WhatsApp:ToPhoneNumber"] ?? "";

        // Sanitize phone number to keep digits only
        var targetPhoneNumber = new string(rawPhoneNumber.Where(char.IsDigit).ToArray());

        if (string.IsNullOrWhiteSpace(targetPhoneNumber))
        {
            _logger.LogError("Target phone number 'WhatsApp:ToPhoneNumber' is missing or invalid.");
            throw new InvalidOperationException("Target phone number is missing or invalid.");
        }

        var messageText = BuildMessageBody(jobList);

        var payload = new
        {
            number = targetPhoneNumber,
            text = messageText
        };

        var requestUrl = _httpClient.BaseAddress != null 
            ? "message/sendText/devjobalerter" 
            : $"{baseUrl}/message/sendText/devjobalerter";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.TryAddWithoutValidation("apikey", apiKey);
        }

        request.Content = JsonContent.Create(payload);

        _logger.LogInformation("Sending {Count} job alert(s) to {Phone} via WhatsApp API...", jobList.Count, targetPhoneNumber);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("WhatsApp API returned error status {StatusCode}: {Response}", response.StatusCode, responseContent);
            
            // Lança exceção para interromper o salvamento no banco de dados caso o envio falhe
            throw new HttpRequestException($"Failed to send WhatsApp message. Status: {response.StatusCode}, Response: {responseContent}");
        }

        _logger.LogInformation("WhatsApp notification dispatched successfully. Response: {Response}", responseContent);
    }

    /// <summary>
    /// Helper method to build a clean Markdown-formatted WhatsApp message body in English.
    /// </summary>
    private string BuildMessageBody(List<JobVacancy> vacancies)
    {
        var builder = new StringBuilder();

        builder.AppendLine("🔔 *DevJobAlerter - New Job Openings Available!*");
        builder.AppendLine($"\nWe found *{vacancies.Count}* recent vacancy(ies):\n");
        builder.AppendLine("-------------------------------");

        foreach (var job in vacancies)
        {
            builder.AppendLine($"💼 *{job.Title}*");
            builder.AppendLine($"🏢 Company: {job.Company}");
            builder.AppendLine($"📍 Location: {job.Location}");
            builder.AppendLine($"🔗 Link: {job.Url}");
            builder.AppendLine("-------------------------------");
        }

        builder.AppendLine("\nGood luck!\n\nWe believe in you! 💪\n\n*DevJobAlerter Team*");
        builder.AppendLine("*DevJobAlerter Worker Service - Your daily dose of job opportunities!*");

        return builder.ToString();
    }
}