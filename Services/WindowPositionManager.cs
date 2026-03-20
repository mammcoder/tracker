using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace HabitTracker.Services;

/// <summary>
/// Сохраняет и восстанавливает позицию окна с поддержкой нескольких мониторов.
///
/// Хранит два состояния:
///   - «намерение» (intended) — последнее положение, куда пользователь поставил вручную
///   - «текущее» — куда окно реально помещено (может отличаться при отключённом мониторе)
///
/// При подключении монитора обратно окно возвращается в «намерение», если оно снова видимо.
///
/// Использование:
///   protected override void OnSourceInitialized(EventArgs e)
///   {
///       base.OnSourceInitialized(e);
///       _positionManager = new WindowPositionManager(this, "/path/to/pos.json");
///       _positionManager.Attach();
///   }
/// </summary>
public sealed class WindowPositionManager : IDisposable
{
    private const int WM_DISPLAYCHANGE = 0x007E;

    private readonly Window _window;
    private readonly string _filePath;

    private double _intendedLeft;
    private double _intendedTop;

    private bool _isProgrammaticMove;
    private bool _attached;
    private HwndSource? _hwndSource;

    public WindowPositionManager(Window window, string filePath)
    {
        _window = window;
        _filePath = filePath;
    }

    /// <summary>
    /// Подключается к окну. Вызывать из OnSourceInitialized (HWND уже существует).
    /// </summary>
    public void Attach()
    {
        if (_attached) return;
        _attached = true;

        _hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(_window).Handle);
        _hwndSource?.AddHook(WndProc);

        var saved = Load();
        double windowWidth = _window.Width > 0 ? _window.Width : 320;

        if (saved.HasValue)
        {
            _intendedLeft = saved.Value.Left;
            _intendedTop  = saved.Value.Top;

            if (IsPositionVisible(_intendedLeft, _intendedTop, windowWidth))
                ApplyIntended();
            else
                CenterOnPrimary();
        }
        else
        {
            CenterOnPrimary();
            _intendedLeft = _window.Left;
            _intendedTop  = _window.Top;
        }

        _window.LocationChanged += OnLocationChanged;
        _window.Closed          += OnClosed;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_DISPLAYCHANGE)
            _window.Dispatcher.BeginInvoke(OnDisplayChanged);

        return IntPtr.Zero;
    }

    private void OnDisplayChanged()
    {
        double windowWidth = _window.ActualWidth > 0 ? _window.ActualWidth : _window.Width;

        if (IsPositionVisible(_intendedLeft, _intendedTop, windowWidth))
        {
            // Монитор вернулся — восстанавливаем намерение
            ApplyIntended();
        }
        else if (!IsPositionVisible(_window.Left, _window.Top, windowWidth))
        {
            // Текущая позиция тоже вне экрана — перемещаем на основной
            CenterOnPrimary();
        }
        // Иначе: текущая позиция ок, намерение недостижимо — ждём дальше
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        if (_isProgrammaticMove) return;

        _intendedLeft = _window.Left;
        _intendedTop  = _window.Top;
        Save();
    }

    private void OnClosed(object? sender, EventArgs e) => Save();

    private void ApplyIntended()
    {
        _isProgrammaticMove = true;
        _window.Left = _intendedLeft;
        _window.Top  = _intendedTop;
        _isProgrammaticMove = false;
    }

    private void CenterOnPrimary()
    {
        _isProgrammaticMove = true;
        var area = Screen.PrimaryScreen!.WorkingArea;
        _window.Left = area.Left + (area.Width  - _window.Width)  / 2;
        _window.Top  = area.Top  + (area.Height - _window.Height) / 2;
        _isProgrammaticMove = false;
    }

    /// <summary>
    /// Считает позицию видимой, если верхняя полоса окна (200×30 px) пересекается
    /// хотя бы с рабочей областью одного из подключённых мониторов.
    /// </summary>
    private static bool IsPositionVisible(double left, double top, double width)
    {
        var strip = new System.Drawing.Rectangle(
            (int)left, (int)top,
            (int)Math.Min(width, 200), 30);

        return Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(strip));
    }

    private (double Left, double Top)? Load()
    {
        try
        {
            if (!File.Exists(_filePath)) return null;
            var dto = JsonSerializer.Deserialize<PositionDto>(File.ReadAllText(_filePath));
            return dto is null ? null : (dto.Left, dto.Top);
        }
        catch { return null; }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var json = JsonSerializer.Serialize(
                new PositionDto(_intendedLeft, _intendedTop),
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch { }
    }

    public void Dispose()
    {
        _hwndSource?.RemoveHook(WndProc);
        _window.LocationChanged -= OnLocationChanged;
        _window.Closed          -= OnClosed;
    }

    private record PositionDto(double Left, double Top);
}
