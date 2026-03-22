using System.IO;
using System.Text.Encodings.Web;
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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static AppData Load()
    {
        if (!File.Exists(DataFilePath))
        {
            return new AppData();
        }

        try
        {
            var json = File.ReadAllText(DataFilePath);
            return JsonSerializer.Deserialize<AppData>(json, JsonOptions) ?? new AppData();
        }
        catch
        {
            return new AppData();
        }
    }

    public static void Save(AppData data)
    {
        Directory.CreateDirectory(AppDataFolder);
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(DataFilePath, json);
    }
}
