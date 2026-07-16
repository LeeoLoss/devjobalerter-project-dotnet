using DevJobAlerter.Domain.Entities; // Required to recognize the JobVacancy domain entity
using DevJobAlerter.Domain.Interfaces;
using Microsoft.Extensions.Configuration; // Required to access appsettings.json configurations
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types; // Required to use the Twilio PhoneNumber wrapper class

namespace DevJobAlerter.Infrastructure.Services;

/// Service responsible for sending job alerts via WhatsApp using the Twilio API.
/// It implements the INotificationService interface from the Domain layer.
public class WhatsAppNotificationService : INotificationService
{
    private readonly ILogger<WhatsAppNotificationService> _logger;
    private readonly IConfiguration _configuration;

    // Constructor: Injecting standard logger and configuration dependencies
    public WhatsAppNotificationService(
        ILogger<WhatsAppNotificationService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    /// Sends a formatted WhatsApp message to a recipient containing a list of job vacancies.
    public async Task SendJobAlertAsync(string toPhoneNumber, List<JobVacancy> vacancies)
    {
        // 1. Guard Clause: Prevents sending an empty message if there are no vacancies
        if (vacancies == null || vacancies.Count == 0)
        {
            _logger.LogWarning("No job vacancies to send in the WhatsApp notification.");
            return;
        }

        // 2. Configuration Loading: Retrieves Twilio credentials from appsettings.json safely
        var accountSid = _configuration["Twilio:AccountSid"];
        var authToken = _configuration["Twilio:AuthToken"];
        var fromWhatsAppNumber = _configuration["Twilio:FromWhatsAppNumber"] ?? "whatsapp:+14155238886";
    
        // 3. Validation: Ensures the application has the necessary credentials before calling the API
        if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))     
        {
            _logger.LogError("Twilio credentials are not configured.");
            return;
        }

        try
        {
            // 4. Initialization: Authenticates the Twilio SDK with your Account SID and Auth Token
            TwilioClient.Init(accountSid, authToken);

            // 5. Message Formatting: Builds the localized/styled string body containing all jobs
            var messageBody = BuildMessageBody(vacancies);

            _logger.LogInformation("Sending job alert to {phone} via WhatsApp...", toPhoneNumber);
        
            // 6. API Dispatch: Calls Twilio's HTTP client to send the WhatsApp message
            var message = await MessageResource.CreateAsync(
                body: messageBody,
                from: new PhoneNumber(fromWhatsAppNumber),
                to: new PhoneNumber($"whatsapp:{toPhoneNumber}")
            );

            // Logs the success alongside the Twilio Message SID for tracking purposes
            _logger.LogInformation("WhatsApp notification sent successfully. Message SID: {sid}", message.Sid); 
        }
        catch (Exception ex)
        {
            // 7. Error Handling: Captures and logs any API/network failure without crashing the app
            _logger.LogError(ex, "Failed to send WhatsApp message via Twilio.");
        }
    }

    /// Private helper method to format the list of job vacancies into a single, structured, clean WhatsApp message.
    /// Uses Markdown formatting (e.g. *bold*, emojis) supported natively by WhatsApp.
    private string BuildMessageBody(List<JobVacancy> vacancies)
    {
        // Setup the header of the notification
        var body = $"🔔 *DevJobAlerter - New Job Openings Available!*\n\nWe found {vacancies.Count} recent vacancy(ies):\n\n";
    
        // Dynamically append each vacancy found
        foreach (var job in vacancies)
        {
            body += $"💼 *{job.Title}*\n";
            body += $"🏢 Company: {job.Company}\n";
            body += $"📍 Location: {job.Location}\n";
            body += $"🔗 Link: {job.Url}\n\n";
        }
        
        // Setup the professional/encouraging footer
        body += "Good luck!\n\nWe believe in you! 💪\n\n*DevJobAlerter Team*";
        return body;
    }
}