using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using HabitTracker.Services;
using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class MainWindow : Window
{
    private WindowPositionManager? _positionManager;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HabitTracker", "window-position.json");

        _positionManager = new WindowPositionManager(this, filePath);
        _positionManager.Attach();
    }

    private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is System.Windows.Controls.Primitives.Thumb)
            return;

        DragMove();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
            Hide();
    }
}
