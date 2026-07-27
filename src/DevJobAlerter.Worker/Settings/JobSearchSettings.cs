namespace DevJobAlerter.Worker.Settings;

public class JobSearchSettings
{
    public const string SectionName = "JobSearchSettings"; // Section name in appsettings.json

    public List<string> SearchTerms { get; set; } = new(); // List of search terms for job vacancies
    public int SearchIntervalMinutes { get; set; } = 60; // Interval in minutes to check for new job vacancies
    public string TargetPhoneNumber { get; set; } = string.Empty; // Phone number to send job alerts to
}