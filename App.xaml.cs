using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using HabitTracker.Services;
using HabitTracker.ViewModels;
using HabitTracker.Views;
using Application = System.Windows.Application;

namespace HabitTracker;

public partial class App : Application
{
    private NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;
    private string _currentTheme = "system";
    private FileSystemWatcher? _settingsWatcher;

    private ToolStripMenuItem? _themeDarkItem;
    private ToolStripMenuItem? _themeLightItem;
    private ToolStripMenuItem? _themeSystemItem;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var appData = DataService.Load();
            _currentTheme = appData.Theme;
            ThemeService.ApplyTheme(_currentTheme);

            SetupTrayIcon();
            SetupSettingsWatcher();

            _mainWindow = new MainWindow();
            _mainWindow.Show();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка при запуске: {ex.Message}\n\n{ex.StackTrace}", 
                          "Ошибка", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void SetupTrayIcon()
    {
        _notifyIcon = new NotifyIcon();
        
        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clock.ico");
        if (File.Exists(iconPath))
        {
            _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
        }
        
        _notifyIcon.Text = "Habit Tracker";
        _notifyIcon.Visible = true;
        _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

        _themeDarkItem   = new ToolStripMenuItem("Тёмная")   { Tag = "dark" };
        _themeLightItem  = new ToolStripMenuItem("Светлая")  { Tag = "light" };
        _themeSystemItem = new ToolStripMenuItem("Системная") { Tag = "system" };

        _themeDarkItem.Click   += ThemeMenuItem_Click;
        _themeLightItem.Click  += ThemeMenuItem_Click;
        _themeSystemItem.Click += ThemeMenuItem_Click;

        var themeMenu = new ToolStripMenuItem("Тема");
        themeMenu.DropDownItems.Add(_themeDarkItem);
        themeMenu.DropDownItems.Add(_themeLightItem);
        themeMenu.DropDownItems.Add(_themeSystemItem);

        UpdateThemeMenuChecks();

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Показать", null, (s, e) => ShowMainWindow());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(themeMenu);
        contextMenu.Items.Add("Настройки", null, (s, e) => OpenSettings());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Выход", null, (s, e) => ExitApplication());

        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void ThemeMenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item || item.Tag is not string theme)
            return;

        _currentTheme = theme;

        Dispatcher.Invoke(() => ThemeService.ApplyTheme(theme));

        var appData = DataService.Load();
        appData.Theme = theme;
        DataService.Save(appData);

        UpdateThemeMenuChecks();
    }

    private void UpdateThemeMenuChecks()
    {
        if (_themeDarkItem   != null) _themeDarkItem.Checked   = _currentTheme == "dark";
        if (_themeLightItem  != null) _themeLightItem.Checked  = _currentTheme == "light";
        if (_themeSystemItem != null) _themeSystemItem.Checked = _currentTheme == "system";
    }

    private void OpenSettings()
    {
        var path = DataService.DataFilePath;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Не удалось открыть файл настроек:\n{ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetupSettingsWatcher()
    {
        var folder = Path.GetDirectoryName(DataService.DataFilePath)!;
        Directory.CreateDirectory(folder);

        _settingsWatcher = new FileSystemWatcher(folder, "data.json")
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        _settingsWatcher.Changed += OnSettingsFileChanged;
    }

    private DateTime _lastSettingsReload = DateTime.MinValue;

    private void OnSettingsFileChanged(object sender, FileSystemEventArgs e)
    {
        var now = DateTime.UtcNow;
        if ((now - _lastSettingsReload).TotalMilliseconds < 500)
            return;
        _lastSettingsReload = now;

        Task.Delay(300).ContinueWith(_ =>
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    var appData = DataService.Load();

                    _currentTheme = appData.Theme;
                    ThemeService.ApplyTheme(appData.Theme);
                    UpdateThemeMenuChecks();

                    if (_mainWindow?.DataContext is MainViewModel vm)
                        vm.Reload(appData);
                }
                catch { }
            });
        });
    }

    private void ShowMainWindow()
    {
        if (_mainWindow != null)
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }
    }

    private void ExitApplication()
    {
        _notifyIcon?.Dispose();
        _settingsWatcher?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _notifyIcon?.Dispose();
        _settingsWatcher?.Dispose();
        base.OnExit(e);
    }
}
