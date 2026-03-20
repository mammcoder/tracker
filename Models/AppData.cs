namespace HabitTracker.Models;

public class AppData
{
    public string Theme { get; set; } = "system";
    public List<Habit> Habits { get; set; } = new();
}
