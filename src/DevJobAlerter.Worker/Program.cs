using DevJobAlerter.Domain.Interfaces;
using DevJobAlerter.Infrastructure.Services;
using DevJobAlerter.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Create the app builder
var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

// 1. Dependency Injection Setup
builder.Services.AddSingleton<INotificationService, WhatsAppNotificationService>();

// 2. Register the AdzunaJobService for fetching job vacancies
builder.Services.AddHttpClient<IJobService, AdzunaJobService>();

// 3. Configuration Setup: Load appsettings.json for Twilio credentials and other settings
builder.Configuration.SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true);

// 4. Register the Worker as a Hosted Service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();