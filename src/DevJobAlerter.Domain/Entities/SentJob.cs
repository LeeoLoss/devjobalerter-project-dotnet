namespace DevJobAlerter.Domain.Entities;

public class SentJob
{
    public int Id { get; set; } // Unique identifier for the sent job
    public string JobUrl { get; set; } = string.Empty; // URL of the job vacancy
    public string Title { get; set; } = string.Empty; // Title of the job vacancy
    public string Company { get; set; } = string.Empty; // Company offering the job vacancy
    public DateTime SentAt { get; set; } = DateTime.UtcNow; // Timestamp when the job alert was sent
}