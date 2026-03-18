using System.Text.RegularExpressions;

namespace CppRefactoring;

/// <summary>
/// Варіант 9. Рефакторинг «Додавання параметра».
/// Додає новий параметр до сигнатури функції та оновлює
/// всі місця її виклику у вихідному коді.
/// </summary>
public class ParameterAdder : IRefactoring
{
    public string  FunctionName  { get; }
    public string  ParameterType { get; }
    public string  ParameterName { get; }
    public string? DefaultValue  { get; }   // null — без значення за замовчуванням

    public ParameterAdder(string functionName, string parameterType,
                          string parameterName, string? defaultValue = null)
    {
        FunctionName  = functionName;
        ParameterType = parameterType;
        ParameterName = parameterName;
        DefaultValue  = defaultValue;
    }

    /// <summary>
    /// Додає параметр до оголошення/визначення функції та всіх її викликів.
    /// Повертає RefactoringResult з Success = false при порушенні передумов.
    /// </summary>
    public RefactoringResult Apply(SourceCode source)
    {
        if (string.IsNullOrWhiteSpace(ParameterType))
            return new RefactoringResult
            {
                Success      = false,
                ErrorMessage = "Parameter type cannot be empty"
            };

        if (!FunctionExists(source, FunctionName))
            return new RefactoringResult
            {
                Success      = false,
                ErrorMessage = $"Function '{FunctionName}' not found in source code"
            };

        if (ParameterAlreadyExists(source, FunctionName, ParameterName))
            return new RefactoringResult
            {
                Success      = false,
                ErrorMessage = $"Parameter '{ParameterName}' already exists in '{FunctionName}'"
            };

        // Формуємо рядок нового параметра для сигнатури
        string newParam = DefaultValue != null
            ? $"{ParameterType} {ParameterName} = {DefaultValue}"
            : $"{ParameterType} {ParameterName}";

        var code = source.Content;

        // 1. Оновлюємо оголошення та визначення функції
        code = UpdateFunctionSignatures(code, newParam);

        // 2. Оновлюємо місця виклику (лише якщо задано значення за замовчуванням)
        if (DefaultValue != null)
            code = UpdateCallSites(code, DefaultValue);

        return new RefactoringResult { Success = true, ResultCode = code };
    }

    /// <summary>
    /// Повертає true, якщо функція з назвою functionName присутня у коді.
    /// </summary>
    public bool FunctionExists(SourceCode source, string functionName)
    {
        return Regex.IsMatch(source.Content,
            $@"\b{Regex.Escape(functionName)}\s*\(");
    }

    /// <summary>
    /// Повертає true, якщо параметр із назвою parameterName вже є у функції.
    /// </summary>
    public bool ParameterAlreadyExists(SourceCode source, string functionName,
                                       string parameterName)
    {
        return Regex.IsMatch(source.Content,
            $@"\b{Regex.Escape(functionName)}\s*\([^)]*\b{Regex.Escape(parameterName)}\b[^)]*\)");
    }

    // ──────────────────────────────────────────────────────────────────────
    // Приватні допоміжні методи
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Оновлює всі оголошення та визначення функції (з типом повернення перед іменем).
    /// </summary>
    private string UpdateFunctionSignatures(string code, string newParam)
    {
        // Сигнатура: returnType functionName(params)
        // returnType = одне або більше слів (з урахуванням *, &, ::)
        var pattern = new Regex(
            $@"(\b[\w:]+(?:\s+[\w:*&]+)*\s+){Regex.Escape(FunctionName)}\s*\(([^)]*)\)");

        return pattern.Replace(code, m =>
        {
            var prefix   = m.Groups[1].Value;
            var existing = m.Groups[2].Value.Trim();
            var updated  = string.IsNullOrEmpty(existing)
                ? newParam
                : $"{existing}, {newParam}";
            return $"{prefix}{FunctionName}({updated})";
        });
    }

    /// <summary>
    /// Оновлює місця виклику функції: додає defaultValue як аргумент.
    /// Пропускає рядки, де FunctionName стоїть після типу повернення (сигнатура).
    /// </summary>
    private string UpdateCallSites(string code, string defaultValue)
    {
        // Виклик: functionName(args), де перед ім'ям НЕ стоїть тип_повернення+пробіл
        var pattern = new Regex(
            $@"(?<![\w]\s+){Regex.Escape(FunctionName)}\s*\(([^)]*)\)");

        return pattern.Replace(code, m =>
        {
            var existing = m.Groups[1].Value.Trim();
            var updated  = string.IsNullOrEmpty(existing)
                ? defaultValue
                : $"{existing}, {defaultValue}";
            return $"{FunctionName}({updated})";
        });
    }
}
