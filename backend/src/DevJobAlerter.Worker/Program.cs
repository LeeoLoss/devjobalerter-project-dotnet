using DevJobAlerter.Domain.Interfaces;
using DevJobAlerter.Infrastructure.Data;
using DevJobAlerter.Infrastructure.Repositories;
using DevJobAlerter.Infrastructure.Services;
using DevJobAlerter.Worker;
using DevJobAlerter.Worker.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// 1. Map strongly-typed configuration section 'JobSearchSettings'
builder.Services.Configure<JobSearchSettings>(
    builder.Configuration.GetSection(JobSearchSettings.SectionName)
);

// 2. Register WhatsApp Notification Service with HttpClient
builder.Services.AddHttpClient<INotificationService, ApiWhatsAppNotificationService>();

// 3. Register AdzunaJobService with HttpClient
builder.Services.AddHttpClient<IJobService, AdzunaJobService>();

// 4. Configure SQLite Database using 'devjobalerter.db'
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=devjobalerter.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// 5. Register JobRepository as Scoped
builder.Services.AddScoped<IJobRepository, JobRepository>();

// 6. Register the Worker BackgroundService
builder.Services.AddHostedService<Worker>();

// 7. Build the Host
var host = builder.Build();

// 8. Ensure the SQLite database schema is created on startup
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// 9. Run the background application
host.Run();