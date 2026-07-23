using DevJobAlerter.Domain.Interfaces;
using DevJobAlerter.Infrastructure.Services;
using DevJobAlerter.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.Clear();
Console.WriteLine("==================================================");
Console.WriteLine("DevJobAlerter Worker Service");
Console.WriteLine("==================================================");
Console.WriteLine("Enter search terms (separated by commas): ");

string input = Console.ReadLine() ?? "";

// Split the input into search terms, trim whitespace, and filter out empty terms
string[] searchTerms = input.Split(',')
    .Select(t => t.Trim())
    .Where(t => !string.IsNullOrWhiteSpace(t))
    .ToArray();


// If no valid search terms are provided, use a default filter
if (searchTerms.Length == 0)
{
    Console.WriteLine("No valid search terms provided. Using default filter: 'developer'.");
    searchTerms = new [] { "developer" };
}

// Create the app builder
var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);


// 1. Register the search terms as a singleton service
builder.Services.AddSingleton(searchTerms); 

// 2. Dependency Injection Setup
builder.Services.AddSingleton<INotificationService, WhatsAppNotificationService>();

// 3. Register the AdzunaJobService for fetching job vacancies
builder.Services.AddHttpClient<IJobService, AdzunaJobService>();

// 4. Configuration Setup: Load appsettings.json for Twilio credentials and other settings
builder.Configuration.SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true);

// 5. Register the Worker as a Hosted Service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();