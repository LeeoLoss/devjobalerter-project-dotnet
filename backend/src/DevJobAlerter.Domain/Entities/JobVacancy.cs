namespace DevJobAlerter.Domain.Entities;

public class JobVacancy
{
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime DateFound { get; set; } = DateTime.UtcNow;
}