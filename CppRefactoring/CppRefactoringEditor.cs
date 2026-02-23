namespace CppRefactoring;

/// <summary>
/// Фасад текстового редактора. Зберігає поточний стан коду,
/// підтримує операцію скасування та делегує виконання рефакторингу
/// конкретним реалізаціям IRefactoring.
/// </summary>
public class CppRefactoringEditor
{
    private SourceCode        _current;
    private Stack<SourceCode> _history;

    public CppRefactoringEditor(string sourceCode)
    {
        _current = new SourceCode(sourceCode);
        _history = new Stack<SourceCode>();
    }

    /// <summary>Повертає поточний вміст редагованого файлу.</summary>
    public string GetCurrentCode()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Застосовує рефакторинг до поточного коду.
    /// При успіху зберігає попередній стан у стеку для Undo.
    /// </summary>
    public RefactoringResult ApplyRefactoring(IRefactoring refactoring)
    {
        throw new NotImplementedException();
    }

    /// <summary>Скасовує останню операцію рефакторингу.</summary>
    public void Undo()
    {
        throw new NotImplementedException();
    }

    /// <summary>Завантажує вихідний код із файлу.</summary>
    public void LoadFromFile(string filePath)
    {
        throw new NotImplementedException();
    }

    /// <summary>Зберігає поточний код у файл.</summary>
    public void SaveToFile(string filePath)
    {
        throw new NotImplementedException();
    }
}
