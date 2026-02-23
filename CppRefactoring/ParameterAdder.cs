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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Повертає true, якщо функція з назвою functionName присутня у коді.
    /// </summary>
    public bool FunctionExists(SourceCode source, string functionName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Повертає true, якщо параметр із назвою parameterName вже є у функції.
    /// </summary>
    public bool ParameterAlreadyExists(SourceCode source, string functionName,
                                       string parameterName)
    {
        throw new NotImplementedException();
    }
}
