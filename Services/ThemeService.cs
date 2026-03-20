using System;
using Microsoft.Win32;

namespace HabitTracker.Services;

public static class ThemeService
{
    public static void ApplyTheme(string theme)
    {
        string actualTheme = theme;
        
        if (theme == "system")
        {
            actualTheme = GetSystemTheme();
        }

        var app = System.Windows.Application.Current;
        var dictionaries = app.Resources.MergedDictionaries;
        
        dictionaries.Clear();

        var themeUri = new Uri($"pack://application:,,,/Themes/{(actualTheme == "light" ? "LightTheme" : "DarkTheme")}.xaml");
        var themeDict = new System.Windows.ResourceDictionary { Source = themeUri };
        
        dictionaries.Add(themeDict);
    }

    public static string GetSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            
            if (value is int intValue)
            {
                return intValue == 1 ? "light" : "dark";
            }
        }
        catch
        {
        }
        
        return "dark";
    }
}
