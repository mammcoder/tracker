# Habit Tracker

Минималистичный трекер привычек на WPF.
Компактный виджет с отображением статистики в System Tray.

## Требования

- .NET 8.0 SDK
- Windows OS

## Запуск

```powershell
dotnet run
```

## Сборка

```powershell
dotnet build -c Release
```

## Функции

- Компактное окно со статистикой всех привычек
- Поддержка нескольких привычек через `data.json`
- Прогресс к цели (по диапазону от даты начала до даты окончания)
- System Tray иконка с меню:
  - Показать — открыть окно
  - Тема — тёмная / светлая / системная
  - Выход — закрыть приложение
- Окно сворачивается в трей при минимизации
- Сохранение позиции окна между запусками, с корректной обработкой нескольких мониторов

## Внешний вид

Для каждой привычки показывается:
- **"X дней с DD.MM.YYYY"** — всего дней с даты начала
- **Прогресс-бар** с процентом выполнения цели

## Структура проекта

- `Models/` — данные (Habit, AppData)
- `Services/` — бизнес-логика (DataService, ProgressService, ThemeService, WindowPositionManager)
- `ViewModels/` — MVVM (MainViewModel, HabitViewModel)
- `Views/` — UI (MainWindow)
- `Themes/` — темы оформления (DarkTheme.xaml, LightTheme.xaml)
- `Converters/` — конвертеры XAML
- `clock.ico` — иконка для System Tray

## Хранение данных

JSON в `%AppData%\HabitTracker\data.json`

Позиция окна хранится отдельно в `%AppData%\HabitTracker\window-position.json`.

Формат:
```json
{
  "theme": "system",
  "habits": [
    {
      "name": "ЗОЖ",
      "startDate": "2026-03-08",
      "endDate": "2027-03-08"
    }
  ]
}
```

Формат позиции окна:
```json
{
  "left": 1200.0,
  "top": 120.0
}
```

## Примечания

- Если файл `clock.ico` отсутствует, иконка в трее будет стандартной
- При первом запуске приложение создаёт привычку по умолчанию, если список пуст
