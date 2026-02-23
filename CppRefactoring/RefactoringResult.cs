namespace CppRefactoring;

public class RefactoringResult
{
    public bool   Success      { get; set; }
    public string ResultCode   { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
