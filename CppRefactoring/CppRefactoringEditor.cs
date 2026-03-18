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
    public string GetCurrentCode() => _current.Content;

    /// <summary>
    /// Застосовує рефакторинг до поточного коду.
    /// При успіху зберігає попередній стан у стеку для Undo.
    /// </summary>
    public RefactoringResult ApplyRefactoring(IRefactoring refactoring)
    {
        var result = refactoring.Apply(_current);
        if (result.Success)
        {
            _history.Push(_current);
            _current = new SourceCode(result.ResultCode);
        }
        return result;
    }

    /// <summary>Скасовує останню операцію рефакторингу.</summary>
    public void Undo()
    {
        if (_history.Count > 0)
            _current = _history.Pop();
    }

    /// <summary>Завантажує вихідний код із файлу.</summary>
    public void LoadFromFile(string filePath)
    {
        var content = File.ReadAllText(filePath);
        _history.Clear();
        _current = new SourceCode(content);
    }

    /// <summary>Зберігає поточний код у файл.</summary>
    public void SaveToFile(string filePath)
    {
        File.WriteAllText(filePath, _current.Content);
    }
}
