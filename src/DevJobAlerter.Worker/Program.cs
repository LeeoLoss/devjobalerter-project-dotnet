using DevJobAlerter.Domain.Interfaces;
using DevJobAlerter.Infrastructure.Services;
using DevJobAlerter.Worker;
using DevJobAlerter.Worker.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// 1. Mapping the section 'JobSearchSettings' from appsettings.json/UserSecrets
builder.Services.Configure<JobSearchSettings>(
    builder.Configuration.GetSection(JobSearchSettings.SectionName)
);

// 2. Register the WhatsAppNotificationService as the implementation for INotificationService
builder.Services.AddTransient<INotificationService, WhatsAppNotificationService>();

// 3. Register the AdzunaJobService with HttpClient
builder.Services.AddHttpClient<IJobService, AdzunaJobService>();

// 4. Registering the Worker class as a hosted service
builder.Services.AddHostedService<Worker>();

// 5. Build and run the host
var host = builder.Build();
host.Run();