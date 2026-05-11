using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace CppRefactoring.App;

public partial class MainWindow : Window
{
    private CppRefactoringEditor _editor        = new CppRefactoringEditor(string.Empty);
    private string               _lastInputCode = string.Empty;

    public MainWindow()
    {
        InitializeComponent();

        RefactoringSelector.SelectedIndex = 0;
        BraceStyleBox.SelectedIndex       = 0;
    }

    // ── Перемикання панелей параметрів ──────────────────────────────────

    private void OnRefactoringChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (RefactoringSelector.SelectedItem is not ComboBoxItem item) return;

        var tag = item.Tag?.ToString();
        RenamePanel.IsVisible      = tag == "Rename";
        FormatPanel.IsVisible      = tag == "Format";
        AddParamPanel.IsVisible    = tag == "AddParam";
        RemoveParamPanel.IsVisible = tag == "RemoveParam";
    }

    // ── Файлові операції ─────────────────────────────────────────────────

    private async void OnLoadFile(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title          = "Відкрити файл C++",
                AllowMultiple  = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("C++ файли")
                    {
                        Patterns = new[] { "*.cpp", "*.h", "*.c", "*.cc", "*.cxx" }
                    },
                    new FilePickerFileType("Усі файли") { Patterns = new[] { "*" } }
                }
            });

        if (files.Count == 0) return;

        await using var stream = await files[0].OpenReadAsync();
        using var reader       = new StreamReader(stream);
        var content            = await reader.ReadToEndAsync();

        InputCode.Text  = content;
        _editor         = new CppRefactoringEditor(content);
        _lastInputCode  = content;
        OutputCode.Text = string.Empty;
        UpdateUndoButton();
        SetStatus($"Файл відкрито: {files[0].Name}", success: true);
    }

    private async void OnSaveFile(object? sender, RoutedEventArgs e)
    {
        var code = string.IsNullOrWhiteSpace(OutputCode.Text)
            ? InputCode.Text
            : OutputCode.Text;

        if (string.IsNullOrWhiteSpace(code))
        {
            SetStatus("Нема коду для збереження.", success: false);
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this)!;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title            = "Зберегти файл C++",
                DefaultExtension = ".cpp",
                FileTypeChoices  = new[]
                {
                    new FilePickerFileType("C++ файли") { Patterns = new[] { "*.cpp", "*.h" } },
                    new FilePickerFileType("Усі файли") { Patterns = new[] { "*" } }
                }
            });

        if (file is null) return;

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(code);

        SetStatus($"Файл збережено: {file.Name}", success: true);
    }

    // ── Скасування ───────────────────────────────────────────────────────

    private void OnUndo(object? sender, RoutedEventArgs e)
    {
        _editor.Undo();
        OutputCode.Text = _editor.GetCurrentCode();
        UpdateUndoButton();
        SetStatus("Останню операцію скасовано.", success: true);
    }

    // ── Застосування рефакторингу ─────────────────────────────────────────

    private void OnApply(object? sender, RoutedEventArgs e)
    {
        var inputCode = InputCode.Text ?? string.Empty;

        // Recreate editor only when InputCode changed since last Apply
        if (inputCode != _lastInputCode)
        {
            _editor        = new CppRefactoringEditor(inputCode);
            _lastInputCode = inputCode;
        }

        IRefactoring? refactoring = BuildRefactoring();
        if (refactoring is null) return;

        var result = _editor.ApplyRefactoring(refactoring);

        if (result.Success)
        {
            OutputCode.Text = result.ResultCode;
            SetStatus("Рефакторинг успішно застосовано.", success: true);
        }
        else
        {
            OutputCode.Text = string.Empty;
            SetStatus($"Помилка: {result.ErrorMessage}", success: false);
        }

        UpdateUndoButton();
    }

    // ── Фабрика об'єктів рефакторингу ────────────────────────────────────

    private IRefactoring? BuildRefactoring()
    {
        if (RefactoringSelector.SelectedItem is not ComboBoxItem item) return null;

        switch (item.Tag?.ToString())
        {
            case "Rename":
            {
                var oldName = OldNameBox.Text?.Trim() ?? string.Empty;
                var newName = NewNameBox.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
                {
                    SetStatus("Заповніть поля «Стара назва» та «Нова назва».", success: false);
                    return null;
                }
                return new VariableRenamer(oldName, newName);
            }

            case "Format":
            {
                var style = BraceStyleBox.SelectedItem is ComboBoxItem si
                    && si.Tag?.ToString() == "KAndR"
                        ? BraceStyle.KAndR
                        : BraceStyle.Allman;

                int indent = (int)(IndentSizeBox.Value ?? 4);
                return new BlockFormatter(style, indent);
            }

            case "AddParam":
            {
                var funcName   = FuncNameBox.Text?.Trim()  ?? string.Empty;
                var paramType  = ParamTypeBox.Text?.Trim() ?? string.Empty;
                var paramName  = ParamNameBox.Text?.Trim() ?? string.Empty;
                var defaultVal = string.IsNullOrWhiteSpace(DefaultValBox.Text)
                                     ? null
                                     : DefaultValBox.Text.Trim();

                if (string.IsNullOrEmpty(funcName) || string.IsNullOrEmpty(paramType)
                    || string.IsNullOrEmpty(paramName))
                {
                    SetStatus("Заповніть поля «Функція», «Тип» та «Назва параметра».",
                              success: false);
                    return null;
                }
                return new ParameterAdder(funcName, paramType, paramName, defaultVal);
            }

            case "RemoveParam":
            {
                var funcName  = RemoveFuncNameBox.Text?.Trim()  ?? string.Empty;
                var paramName = RemoveParamNameBox.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(funcName) || string.IsNullOrEmpty(paramName))
                {
                    SetStatus("Заповніть поля «Назва функції» та «Назва параметра».",
                              success: false);
                    return null;
                }
                return new ParameterRemover(funcName, paramName);
            }

            default:
                return null;
        }
    }

    // ── Індикатор скасування ──────────────────────────────────────────────

    private void UpdateUndoButton()
    {
        var count = _editor.HistoryCount;
        BtnUndo.Content = count > 0
            ? $"↩ Скасувати ({count})"
            : "↩ Скасувати";
    }

    // ── Рядок стану ──────────────────────────────────────────────────────

    private void SetStatus(string message, bool success)
    {
        StatusText.Text = message;
        if (StatusText.Parent is Border border)
            border.Background = success
                ? Avalonia.Media.Brushes.DarkGreen
                : Avalonia.Media.Brushes.DarkRed;
    }
}
