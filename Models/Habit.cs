namespace HabitTracker.Models;

public class Habit
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public int GoalDays { get; set; } = 365;
    public List<DateTime> FailedDays { get; set; } = new();
}
