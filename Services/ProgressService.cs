using HabitTracker.Models;

namespace HabitTracker.Services;

public static class ProgressService
{
    public static int GetTotalDays(Habit habit)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (today < habit.StartDate)
            return 0;
        
        return today.DayNumber - habit.StartDate.DayNumber;
    }

    public static int GetStreak(Habit habit)
    {
        return GetTotalDays(habit);
    }

    public static double GetPercent(Habit habit)
    {
        var start = habit.StartDate;
        var end = habit.EndDate;
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (end <= start)
            return today >= end ? 100 : 0;

        if (today <= start)
            return 0;

        if (today >= end)
            return 100;

        var totalDays = end.DayNumber - start.DayNumber;
        var elapsedDays = today.DayNumber - start.DayNumber;
        return Math.Clamp((double)elapsedDays / totalDays * 100, 0, 100);
    }

    public static string FormatDaysAsMonthsAndDays(int days)
    {
        if (days == 0)
            return "0 дней";

        int months = days / 30;
        int remainingDays = days % 30;

        if (months == 0)
            return $"{days} {GetDaysForm(days)}";

        if (remainingDays == 0)
            return $"{months} {GetMonthsForm(months)}";

        return $"{months} {GetMonthsForm(months)} {remainingDays} {GetDaysForm(remainingDays)}";
    }

    private static string GetMonthsForm(int months)
    {
        if (months % 10 == 1 && months % 100 != 11)
            return "месяц";
        if (months % 10 >= 2 && months % 10 <= 4 && (months % 100 < 10 || months % 100 >= 20))
            return "месяца";
        return "месяцев";
    }

    private static string GetDaysForm(int days)
    {
        if (days % 10 == 1 && days % 100 != 11)
            return "день";
        if (days % 10 >= 2 && days % 10 <= 4 && (days % 100 < 10 || days % 100 >= 20))
            return "дня";
        return "дней";
    }
}
