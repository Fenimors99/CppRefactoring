using NUnit.Framework;
using CppRefactoring;

namespace CppRefactoring.Tests;

[TestFixture]
public class ParameterAdderTests
{
    /// <summary>
    /// Test 1. Параметр додається до функції, яка раніше не мала параметрів.
    /// </summary>
    [Test]
    public void Apply_FunctionWithNoParams_ParameterAdded()
    {
        var source = new SourceCode("void foo() {}");
        var adder  = new ParameterAdder("foo", "int", "value");
        var result = adder.Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("void foo(int value)"));
    }

    /// <summary>
    /// Test 2. Новий параметр додається після вже існуючого.
    /// </summary>
    [Test]
    public void Apply_FunctionWithOneParam_NewParamAppended()
    {
        var source = new SourceCode("void foo(int x) {}");
        var adder  = new ParameterAdder("foo", "int", "y");
        var result = adder.Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("void foo(int x, int y)"));
    }

    /// <summary>
    /// Test 3. Всі місця виклику функції оновлюються: вставляється
    /// значення за замовчуванням як аргумент.
    /// </summary>
    [Test]
    public void Apply_UpdatesAllCallSites()
    {
        string code = "void foo(int x) {}\nvoid bar() { foo(1); }";
        var adder   = new ParameterAdder("foo", "int", "y", "0");
        var result  = adder.Apply(new SourceCode(code));

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("foo(1, 0)"));
    }

    /// <summary>
    /// Test 4. Якщо функція з вказаною назвою відсутня у коді,
    /// рефакторинг повертає Success = false.
    /// </summary>
    [Test]
    public void Apply_FunctionNotFound_ReturnsFailure()
    {
        var result = new ParameterAdder("bar", "int", "y")
                         .Apply(new SourceCode("void foo(int x) {}"));

        Assert.That(result.Success, Is.False);
    }

    /// <summary>
    /// Test 5. Якщо параметр із такою ж назвою вже присутній у функції,
    /// рефакторинг повертає Success = false.
    /// </summary>
    [Test]
    public void Apply_DuplicateParameterName_ReturnsFailure()
    {
        var result = new ParameterAdder("foo", "double", "x")
                         .Apply(new SourceCode("void foo(int x) {}"));

        Assert.That(result.Success, Is.False);
    }

    /// <summary>
    /// Test 6. Параметр зі значенням за замовчуванням додається
    /// з правильним синтаксисом «тип назва = значення».
    /// </summary>
    [Test]
    public void Apply_ParameterWithDefault_CorrectSyntax()
    {
        var source = new SourceCode("void foo(int x) {}");
        var result = new ParameterAdder("foo", "int", "y", "42").Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("void foo(int x, int y = 42)"));
    }

    /// <summary>
    /// Test 7. Оновлюються і оголошення, і визначення функції одночасно.
    /// </summary>
    [Test]
    public void Apply_UpdatesDeclarationAndDefinition()
    {
        string code = "void foo(int x);\nvoid foo(int x) { return; }";
        var result  = new ParameterAdder("foo", "int", "y")
                          .Apply(new SourceCode(code));

        Assert.That(result.Success, Is.True);
        int count = System.Text.RegularExpressions.Regex
                        .Matches(result.ResultCode, @"foo\(int x, int y\)").Count;
        Assert.That(count, Is.EqualTo(2));
    }

    /// <summary>
    /// Test 8. FunctionExists повертає true, якщо функція присутня у коді.
    /// </summary>
    [Test]
    public void FunctionExists_ExistingFunction_ReturnsTrue()
    {
        var adder  = new ParameterAdder("foo", "int", "y");
        var source = new SourceCode("void foo(int x) {}");

        Assert.That(adder.FunctionExists(source, "foo"), Is.True);
    }

    /// <summary>
    /// Test 9. FunctionExists повертає false для відсутньої функції.
    /// </summary>
    [Test]
    public void FunctionExists_NonExistentFunction_ReturnsFalse()
    {
        var adder  = new ParameterAdder("foo", "int", "y");
        var source = new SourceCode("void foo(int x) {}");

        Assert.That(adder.FunctionExists(source, "bar"), Is.False);
    }

    /// <summary>
    /// Test 10. Порожній тип параметра є неприпустимим —
    /// рефакторинг повертає Success = false.
    /// </summary>
    [Test]
    public void Apply_EmptyParameterType_ReturnsFailure()
    {
        var result = new ParameterAdder("foo", "", "y")
                         .Apply(new SourceCode("void foo() {}"));

        Assert.That(result.Success, Is.False);
    }
}
