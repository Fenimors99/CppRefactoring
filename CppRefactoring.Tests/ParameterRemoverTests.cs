using NUnit.Framework;
using CppRefactoring;

namespace CppRefactoring.Tests;

[TestFixture]
public class ParameterRemoverTests
{
    /// <summary>
    /// Test 1. Видаляється останній параметр із двох.
    /// </summary>
    [Test]
    public void Apply_LastParameter_ParameterRemoved()
    {
        var source = new SourceCode("void foo(int x, int y) {}");
        var result = new ParameterRemover("foo", "y").Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("void foo(int x)"));
    }

    /// <summary>
    /// Test 2. Видаляється перший параметр із двох.
    /// </summary>
    [Test]
    public void Apply_FirstParameter_ParameterRemoved()
    {
        var source = new SourceCode("void foo(int x, int y) {}");
        var result = new ParameterRemover("foo", "x").Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("void foo(int y)"));
    }

    /// <summary>
    /// Test 3. Видаляється середній параметр із трьох.
    /// </summary>
    [Test]
    public void Apply_MiddleParameter_ParameterRemoved()
    {
        var source = new SourceCode("void foo(int x, int y, int z) {}");
        var result = new ParameterRemover("foo", "y").Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("void foo(int x, int z)"));
    }

    /// <summary>
    /// Test 4. Видаляється єдиний параметр — список стає порожнім.
    /// </summary>
    [Test]
    public void Apply_OnlyParameter_ParameterRemoved()
    {
        var source = new SourceCode("void foo(int x) {}");
        var result = new ParameterRemover("foo", "x").Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("void foo()"));
    }

    /// <summary>
    /// Test 5. Якщо функції немає — повертається Success = false.
    /// </summary>
    [Test]
    public void Apply_FunctionNotFound_ReturnsFailure()
    {
        var result = new ParameterRemover("bar", "x")
            .Apply(new SourceCode("void foo(int x) {}"));

        Assert.That(result.Success, Is.False);
    }

    /// <summary>
    /// Test 6. Якщо параметра немає у функції — повертається Success = false.
    /// </summary>
    [Test]
    public void Apply_ParameterNotFound_ReturnsFailure()
    {
        var result = new ParameterRemover("foo", "z")
            .Apply(new SourceCode("void foo(int x, int y) {}"));

        Assert.That(result.Success, Is.False);
    }

    /// <summary>
    /// Test 7. Оновлюються і оголошення, і визначення функції одночасно.
    /// </summary>
    [Test]
    public void Apply_UpdatesDeclarationAndDefinition()
    {
        string code = "void foo(int x, int y);\nvoid foo(int x, int y) { return; }";
        var result  = new ParameterRemover("foo", "y").Apply(new SourceCode(code));

        Assert.That(result.Success, Is.True);
        int count = System.Text.RegularExpressions.Regex
            .Matches(result.ResultCode, @"foo\(int x\)").Count;
        Assert.That(count, Is.EqualTo(2));
    }

    /// <summary>
    /// Test 8. Метод з типом повернення void та кількома параметрами — параметр видаляється.
    /// </summary>
    [Test]
    public void Apply_VoidReturnType_ParameterRemoved()
    {
        var source = new SourceCode("void print(int a, int b) {}");
        var result = new ParameterRemover("print", "b").Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("void print(int a)"));
    }

    /// <summary>
    /// Test 9. FunctionExists повертає true для наявної функції.
    /// </summary>
    [Test]
    public void FunctionExists_ExistingFunction_ReturnsTrue()
    {
        var remover = new ParameterRemover("foo", "x");
        var source  = new SourceCode("void foo(int x) {}");

        Assert.That(remover.FunctionExists(source, "foo"), Is.True);
    }

    /// <summary>
    /// Test 10. ParameterExists повертає false для відсутнього параметра.
    /// </summary>
    [Test]
    public void ParameterExists_NonExistentParameter_ReturnsFalse()
    {
        var remover = new ParameterRemover("foo", "z");
        var source  = new SourceCode("void foo(int x, int y) {}");

        Assert.That(remover.ParameterExists(source, "foo", "z"), Is.False);
    }
}
