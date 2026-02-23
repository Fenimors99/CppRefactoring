namespace CppRefactoring;

/// <summary>
/// Варіант 1. Рефакторинг «Перейменування змінної».
/// Замінює всі входження ідентифікатора в межах видимості,
/// не зачіпаючи ідентифікатори, що містять OldName як підрядок.
/// </summary>
public class VariableRenamer : IRefactoring
{
    public string OldName { get; }
    public string NewName { get; }

    public VariableRenamer(string oldName, string newName)
    {
        OldName = oldName;
        NewName = newName;
    }

    /// <summary>
    /// Замінює всі входження OldName на NewName у вихідному коді.
    /// Повертає RefactoringResult з Success = false, якщо OldName
    /// відсутній або NewName є неприпустимим ідентифікатором.
    /// </summary>
    public RefactoringResult Apply(SourceCode source)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Повертає true, якщо рядок є коректним ідентифікатором C++
    /// (починається з літери або '_', містить лише [A-Za-z0-9_]).
    /// </summary>
    public bool IsValidIdentifier(string name)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Повертає true, якщо змінна OldName присутня у вихідному коді.
    /// </summary>
    public bool VariableExists(SourceCode source)
    {
        throw new NotImplementedException();
    }
}
