namespace Bakfoo;

public class BakfooOptions
{
    public const string ConfigSectionName = "Bakfoo";

    public int MaxBacklogTasks { get; set; } = 3;
    public int MaxParallelTasks { get; set; } = 1;
    public int MaxHoursToDisplayCompletedTasks { get; set; } = 24;
}
