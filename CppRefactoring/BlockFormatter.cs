namespace CppRefactoring;

/// <summary>
/// Варіант 3. Рефакторинг «Приведення формату блоку».
/// Нормалізує відступи та розташування фігурних дужок
/// відповідно до обраного стилю.
/// </summary>
public class BlockFormatter : IRefactoring
{
    public BraceStyle Style      { get; }
    public int        IndentSize { get; }

    public BlockFormatter(BraceStyle style, int indentSize = 4)
    {
        Style      = style;
        IndentSize = indentSize;
    }

    /// <summary>
    /// Нормалізує форматування всіх блоків відповідно до Style та IndentSize.
    /// Повертає RefactoringResult з Success = false при некоректній структурі блоків.
    /// </summary>
    public RefactoringResult Apply(SourceCode source)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Нормалізує відступи: замінює символ табуляції на IndentSize пробілів,
    /// вирівнює кількість пробілів відповідно до рівня вкладеності.
    /// </summary>
    public string NormalizeIndentation(string code)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Переставляє фігурні дужки відповідно до обраного Style.
    /// </summary>
    public string NormalizeBraces(string code)
    {
        throw new NotImplementedException();
    }
}
