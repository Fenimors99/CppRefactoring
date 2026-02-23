namespace CppRefactoring;

public interface IRefactoring
{
    RefactoringResult Apply(SourceCode source);
}
