using System.IO;
using System.Text.Json;
using HabitTracker.Models;

namespace HabitTracker.Services;

public class DataService
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HabitTracker"
    );
    
    public static readonly string DataFilePath = Path.Combine(AppDataFolder, "data.json");

    public static AppData Load()
    {
        if (!File.Exists(DataFilePath))
        {
            return new AppData();
        }

        try
        {
            var json = File.ReadAllText(DataFilePath);
            var data = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
            var wasMigrated = NormalizeHabits(data, json);
            if (wasMigrated)
                Save(data);
            return data;
        }
        catch
        {
            return new AppData();
        }
    }

    public static void Save(AppData data)
    {
        Directory.CreateDirectory(AppDataFolder);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(DataFilePath, json);
    }

    private static bool NormalizeHabits(AppData data, string rawJson)
    {
        var migrated = false;
        var legacyGoalDaysByIndex = TryReadLegacyGoalDays(rawJson);

        for (int i = 0; i < data.Habits.Count; i++)
        {
            var habit = data.Habits[i];
            if (habit.EndDate == default)
            {
                var goalDays = legacyGoalDaysByIndex.TryGetValue(i, out var legacyGoalDays) && legacyGoalDays > 0
                    ? legacyGoalDays
                    : 365;

                var startDate = habit.StartDate == default ? DateTime.Today : habit.StartDate;
                habit.EndDate = startDate.AddDays(goalDays);
                migrated = true;
            }
        }

        return migrated;
    }

    private static Dictionary<int, int> TryReadLegacyGoalDays(string rawJson)
    {
        var goalDaysByIndex = new Dictionary<int, int>();

        try
        {
            using var document = JsonDocument.Parse(rawJson);
            if (!document.RootElement.TryGetProperty("Habits", out var habitsElement) || habitsElement.ValueKind != JsonValueKind.Array)
                return goalDaysByIndex;

            int index = 0;
            foreach (var habitElement in habitsElement.EnumerateArray())
            {
                if (habitElement.TryGetProperty("GoalDays", out var goalDaysElement) &&
                    goalDaysElement.ValueKind == JsonValueKind.Number &&
                    goalDaysElement.TryGetInt32(out var goalDays))
                {
                    goalDaysByIndex[index] = goalDays;
                }

                index++;
            }
        }
        catch
        {
            // Ignore legacy parsing failures and fall back to default migration rules.
        }

        return goalDaysByIndex;
    }
}
