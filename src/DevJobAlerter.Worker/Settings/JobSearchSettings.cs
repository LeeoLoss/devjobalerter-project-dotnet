namespace DevJobAlerter.Worker.Settings;

/// <summary>
/// Configuration settings for job search criteria and background service execution intervals.
/// </summary>
public class JobSearchSettings
{
    public const string SectionName = "JobSearchSettings";

    /// <summary>
    /// List of keywords or job titles to search for.
    /// </summary>
    public List<string> SearchTerms { get; set; } = new();

    /// <summary>
    /// Interval in minutes between each execution cycle.
    /// </summary>
    public int SearchIntervalMinutes { get; set; } = 60;
}