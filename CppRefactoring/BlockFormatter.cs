using System.Text.RegularExpressions;

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
        var code = source.Content;

        if (!IsBalanced(code))
            return new RefactoringResult
            {
                Success      = false,
                ErrorMessage = "Malformed code: unbalanced brackets"
            };

        code = NormalizeIndentation(code);
        code = ExpandBraces(code);
        code = ReIndent(code);
        code = AddKeywordSpaces(code);
        code = NormalizeBraces(code);
        code = code.TrimEnd('\n', '\r');

        return new RefactoringResult { Success = true, ResultCode = code };
    }

    /// <summary>
    /// Нормалізує відступи: замінює символ табуляції на IndentSize пробілів.
    /// </summary>
    public string NormalizeIndentation(string code)
    {
        return code.Replace("\t", new string(' ', IndentSize));
    }

    /// <summary>
    /// Переставляє фігурні дужки відповідно до обраного Style.
    /// Allman — { на окремому рядку; K&R — { наприкінці попереднього рядка.
    /// </summary>
    public string NormalizeBraces(string code)
    {
        if (Style == BraceStyle.Allman)
        {
            // Переносимо { на новий рядок, якщо стоїть не на початку рядка
            code = Regex.Replace(code, @"([^\n{])\s*\{", "$1\n{");
            return code;
        }
        else // K&R
        {
            // Зливаємо рядок «лише {» з попереднім рядком
            var lines  = code.Split('\n').ToList();
            var result = new List<string>();
            foreach (var line in lines)
            {
                if (line.Trim() == "{" && result.Count > 0)
                    result[result.Count - 1] = result[result.Count - 1].TrimEnd() + " {";
                else
                    result.Add(line);
            }
            return string.Join("\n", result);
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // Приватні допоміжні методи
    // ──────────────────────────────────────────────────────────────────────

    private bool IsBalanced(string code)
    {
        int curly = 0, paren = 0;
        foreach (char c in code)
        {
            if      (c == '{') curly++;
            else if (c == '}') curly--;
            else if (c == '(') paren++;
            else if (c == ')') paren--;
            if (curly < 0 || paren < 0) return false;
        }
        return curly == 0 && paren == 0;
    }

    /// <summary>Розбиває inline-блоки: кожна { та } виноситься на окремий рядок.</summary>
    private string ExpandBraces(string code)
    {
        // Перед { — обов'язково новий рядок
        code = Regex.Replace(code, @"\s*\{", "\n{");
        // Після { — обов'язково новий рядок
        code = Regex.Replace(code, @"\{([^\n])", "{\n$1");
        // Перед } — обов'язково новий рядок
        code = Regex.Replace(code, @"([^\n])\}", "$1\n}");
        // Після } — обов'язково новий рядок
        code = Regex.Replace(code, @"\}([^\n])", "}\n$1");
        return code;
    }

    /// <summary>
    /// Прибирає всі відступи, обчислює глибину вкладеності та
    /// додає правильний відступ до кожного рядка.
    /// </summary>
    private string ReIndent(string code)
    {
        var lines = code.Split('\n')
                        .Select(l => l.Trim())
                        .Where(l => l.Length > 0)
                        .ToList();

        int depth  = 0;
        var result = new List<string>();

        foreach (var line in lines)
        {
            if (line == "}")
            {
                depth = Math.Max(0, depth - 1);
                result.Add(new string(' ', depth * IndentSize) + "}");
                if (depth == 0)
                    result.Add(string.Empty);
            }
            else if (line == "{")
            {
                result.Add(new string(' ', depth * IndentSize) + "{");
                depth++;
            }
            else
            {
                result.Add(new string(' ', depth * IndentSize) + line);
            }
        }

        return string.Join("\n", result);
    }

    /// <summary>Додає пробіл між ключовим словом та дужкою: if( → if (.</summary>
    private string AddKeywordSpaces(string code)
    {
        return Regex.Replace(code, @"\b(if|for|while|switch)\s*\(", "$1 (");
    }
}
