namespace Bakhoo;

public class BakhooOptions
{
    public const string ConfigSectionName = "Bakhoo";

    public int MaxBacklogTasks { get; set; } = 3;
    public int MaxParallelTasks { get; set; } = 1;
    public int MaxHoursToDisplayCompletedTasks { get; set; } = 24;
}
