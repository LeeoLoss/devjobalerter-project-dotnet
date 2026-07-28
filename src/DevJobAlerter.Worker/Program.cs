using DevJobAlerter.Domain.Interfaces;
using DevJobAlerter.Infrastructure.Data;
using DevJobAlerter.Infrastructure.Services;
using DevJobAlerter.Infrastructure.Repositories;
using DevJobAlerter.Worker;
using DevJobAlerter.Worker.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// 1. Mapping the section 'JobSearchSettings' from apsettings.json/UserSecrets
builder.Services.Configure<JobSearchSettings>(
    builder.Configuration.GetSection(JobSearchSettings.SectionName)
);

// 2. Register the WhatsAppNotificationService as the implementation for INotificationService
builder.Services.AddTransient<INotificationService, WhatsAppNotificationService>();

// 3. Register the AdzunaJobService with HttpClient
builder.Services.AddHttpClient<IJobService, AdzunaJobService>();

// 4. Registering the Worker class as a hosted service
builder.Services.AddHostedService<Worker>();

// 5. Registering the SQLite database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source = jobs.db")); // Using SQLite for simplicity

// 6. Registering the JobRepository
builder.Services.AddScoped<IJobRepository, JobRepository>(); // Registering the JobRepository

// 7. Build and run the host
var host = builder.Build();

// 8. Ensure the database is created
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// 9. Run the host
host.Run();