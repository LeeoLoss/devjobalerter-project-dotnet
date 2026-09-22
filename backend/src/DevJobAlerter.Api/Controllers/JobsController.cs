using DevJobAlerter.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevJobAlerter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _database;

    public JobsController(AppDbContext database)
    {
        _database = database;
    }

    [HttpGet]
    public async Task<ActionResult> GetJobs(CancellationToken cancellationToken)
    {
        var jobs = await _database.SentJobs
            .AsNoTracking()
            .OrderByDescending(job => job.SentAt)
            .Select(job => new
            {
                job.Id,
                job.Title,
                job.Company,
                location = "Localização não informada",
                url = job.JobUrl,
                dateFound = job.SentAt
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            items = jobs,
            total = jobs.Count,
            lastUpdated = DateTimeOffset.UtcNow
        });
    }
}
