using System;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker.ViewModels;

public class HabitViewModel
{
    private readonly Habit _habit;

    public HabitViewModel(Habit habit)
    {
        _habit = habit;
    }

    public string Name => _habit.Name;
    public DateTime StartDate => _habit.StartDate.ToDateTime(TimeOnly.MinValue);
    public string TotalDays => ProgressService.FormatDaysAsMonthsAndDays(ProgressService.GetTotalDays(_habit));
    public double ProgressPercent => ProgressService.GetPercent(_habit);
}
