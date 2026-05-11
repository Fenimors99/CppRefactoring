using System.Text.RegularExpressions;

namespace CppRefactoring;

/// <summary>
/// Варіант 10. Рефакторинг «Видалення параметра».
/// Видаляє параметр із сигнатури функції (оголошення та визначення).
/// </summary>
public class ParameterRemover : IRefactoring
{
    public string FunctionName  { get; }
    public string ParameterName { get; }

    public ParameterRemover(string functionName, string parameterName)
    {
        FunctionName  = functionName;
        ParameterName = parameterName;
    }

    public RefactoringResult Apply(SourceCode source)
    {
        if (!FunctionExists(source, FunctionName))
            return new RefactoringResult
            {
                Success      = false,
                ErrorMessage = $"Function '{FunctionName}' not found in source code"
            };

        if (!ParameterExists(source, FunctionName, ParameterName))
            return new RefactoringResult
            {
                Success      = false,
                ErrorMessage = $"Parameter '{ParameterName}' not found in '{FunctionName}'"
            };

        var code = RemoveFromSignatures(source.Content);
        return new RefactoringResult { Success = true, ResultCode = code };
    }

    public bool FunctionExists(SourceCode source, string functionName)
    {
        return Regex.IsMatch(source.Content,
            $@"\b{Regex.Escape(functionName)}\s*\(");
    }

    public bool ParameterExists(SourceCode source, string functionName, string parameterName)
    {
        return Regex.IsMatch(source.Content,
            $@"\b{Regex.Escape(functionName)}\s*\([^)]*\b{Regex.Escape(parameterName)}\b[^)]*\)");
    }

    private string RemoveFromSignatures(string code)
    {
        var pattern = new Regex(
            $@"(\b[\w:]+(?:\s+[\w:*&]+)*\s+){Regex.Escape(FunctionName)}\s*\(([^)]*)\)",
            RegexOptions.Singleline);

        return pattern.Replace(code, m =>
        {
            var prefix    = m.Groups[1].Value;
            var paramList = m.Groups[2].Value;

            int currentIndex = 0;

            while (currentIndex < paramList.Length)
            {
                int commaIndex = paramList.IndexOf(',', currentIndex);
                int endIndex   = commaIndex >= 0 ? commaIndex : paramList.Length;

                string[] parts = paramList
                    .Substring(currentIndex, endIndex - currentIndex)
                    .Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0 && parts[^1] == ParameterName)
                {
                    bool isFirst     = currentIndex == 0;
                    int  removeStart = currentIndex;
                    int  removeLen   = endIndex - currentIndex;

                    if (commaIndex >= 0)
                    {
                        removeLen++; // include trailing comma
                        // Remove the space after comma when deleting the first parameter
                        if (isFirst && removeStart + removeLen < paramList.Length
                            && paramList[removeStart + removeLen] == ' ')
                            removeLen++;
                    }
                    else if (currentIndex > 0 && paramList[currentIndex - 1] == ',')
                    {
                        removeStart--;
                        removeLen++; // include preceding comma for last param
                    }

                    paramList = paramList.Remove(removeStart, removeLen);
                    break;
                }

                currentIndex = endIndex + 1;
            }

            return $"{prefix}{FunctionName}({paramList})";
        });
    }
}
