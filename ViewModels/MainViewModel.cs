using System;
using System.Collections.ObjectModel;
using System.Linq;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker.ViewModels;

public class MainViewModel
{
    public ObservableCollection<HabitViewModel> Habits { get; set; }

    public MainViewModel()
    {
        var appData = DataService.Load();
        
        if (appData.Habits.Count == 0)
        {
            var startDate = DateTime.Today.AddDays(-7);
            var defaultHabit = new Habit
            {
                Name = "Моя привычка",
                StartDate = startDate,
                EndDate = startDate.AddDays(365)
            };
            appData.Habits.Add(defaultHabit);
            DataService.Save(appData);
        }

        Habits = new ObservableCollection<HabitViewModel>(
            appData.Habits.Select(h => new HabitViewModel(h))
        );
    }

    public void Reload(AppData appData)
    {
        Habits.Clear();
        foreach (var h in appData.Habits)
            Habits.Add(new HabitViewModel(h));
    }
}
