using DevJobAlerter.Infrastructure.Data;
using DevJobAlerter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS for the React application
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=devjobalerter.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await database.Database.EnsureCreatedAsync();

    if (app.Environment.IsDevelopment() && !await database.SentJobs.AnyAsync())
    {
        database.SentJobs.Add(new SentJob
        {
            Title = "Desenvolvedor(a) Front-end React — Pleno",
            Company = "DevJob Labs",
            JobUrl = "https://www.linkedin.com/jobs/",
            SentAt = DateTime.UtcNow
        });
        await database.SaveChangesAsync();
    }
}

app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    service = "DevJobAlerter API",
    timestamp = DateTimeOffset.UtcNow
}));
app.MapControllers();

app.Run();
