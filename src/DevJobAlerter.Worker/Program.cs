using DevJobAlerter.Domain.Interfaces;
using DevJobAlerter.Infrastructure.Services;
using DevJobAlerter.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Alterado aqui para usar o caminho completo e evitar ambiguidades
var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

// 1. Dependency Injection Setup
builder.Services.AddSingleton<INotificationService, WhatsAppNotificationService>();

// 2. Configuration Setup: Load appsettings.json for Twilio credentials and other settings
builder.Configuration.SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 3. Register the Worker as a Hosted Service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();