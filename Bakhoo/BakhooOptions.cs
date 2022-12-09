namespace Bakhoo;

public class BakhooOptions
{
    public const string ConfigSectionName = "Bakhoo";

    public int MaxBacklogJobs { get; set; } = 3;
    public int MaxParallelJobs { get; set; } = 1;
    public int MaxHoursToDisplayCompletedJobs { get; set; } = 24;
}
