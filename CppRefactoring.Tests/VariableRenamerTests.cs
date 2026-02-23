using NUnit.Framework;
using CppRefactoring;

namespace CppRefactoring.Tests;

[TestFixture]
public class VariableRenamerTests
{
    /// <summary>
    /// Test 1. Базове перейменування в оголошенні змінної.
    /// Єдине входження замінюється на нову назву.
    /// </summary>
    [Test]
    public void Apply_SingleDeclaration_RenamesCorrectly()
    {
        var source  = new SourceCode("int x = 5;");
        var renamer = new VariableRenamer("x", "count");
        var result  = renamer.Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Is.EqualTo("int count = 5;"));
    }

    /// <summary>
    /// Test 2. Всі входження змінної у різних рядках замінюються одночасно.
    /// </summary>
    [Test]
    public void Apply_MultipleOccurrences_AllRenamed()
    {
        string code = "int x = 0;\nx = x + 1;\nreturn x;";
        var result  = new VariableRenamer("x", "count").Apply(new SourceCode(code));

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode,
            Is.EqualTo("int count = 0;\ncount = count + 1;\nreturn count;"));
    }

    /// <summary>
    /// Test 3. Ідентифікатор, що містить OldName як підрядок
    /// («xtra» при перейменуванні «x»), не повинен змінюватися.
    /// </summary>
    [Test]
    public void Apply_SubstringMatch_NotRenamed()
    {
        string code = "int x = 1;\nint xtra = 2;";
        var result  = new VariableRenamer("x", "val").Apply(new SourceCode(code));

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("xtra"));
        Assert.That(result.ResultCode, Does.Not.Contain("valtra"));
    }

    /// <summary>
    /// Test 4. Параметр функції перейменовується разом з усіма
    /// входженнями всередині тіла функції.
    /// </summary>
    [Test]
    public void Apply_FunctionParameter_RenamesInsideBody()
    {
        var source = new SourceCode("void foo(int x) { return x; }");
        var result = new VariableRenamer("x", "value").Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode,
            Is.EqualTo("void foo(int value) { return value; }"));
    }

    /// <summary>
    /// Test 5. Якщо змінна з OldName відсутня у коді,
    /// рефакторинг повертає Success = false.
    /// </summary>
    [Test]
    public void Apply_VariableNotFound_ReturnsFailure()
    {
        var result = new VariableRenamer("x", "count")
                         .Apply(new SourceCode("int y = 5;"));

        Assert.That(result.Success, Is.False);
    }

    /// <summary>
    /// Test 6. Нова назва, що починається з цифри, є неприпустимим
    /// ідентифікатором — рефакторинг повертає помилку.
    /// </summary>
    [Test]
    public void Apply_InvalidNewName_StartsWithDigit_ReturnsFailure()
    {
        var result = new VariableRenamer("x", "1count")
                         .Apply(new SourceCode("int x = 5;"));

        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    /// <summary>
    /// Test 7. Змінна циклу перейменовується в усіх частинах оператора for:
    /// ініціалізація, умова та інкремент.
    /// </summary>
    [Test]
    public void Apply_LoopVariable_RenamesInAllLoopParts()
    {
        var source = new SourceCode("for(int i = 0; i < 10; i++) {}");
        var result = new VariableRenamer("i", "index").Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode,
            Is.EqualTo("for(int index = 0; index < 10; index++) {}"));
    }

    /// <summary>
    /// Test 8. Змінна у виразі return перейменовується коректно.
    /// </summary>
    [Test]
    public void Apply_VariableInReturn_Renamed()
    {
        var source = new SourceCode("int func() { int x = 5; return x; }");
        var result = new VariableRenamer("x", "res").Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode,
            Is.EqualTo("int func() { int res = 5; return res; }"));
    }

    /// <summary>
    /// Test 9. IsValidIdentifier повертає true для коректних ідентифікаторів C++.
    /// </summary>
    [Test]
    public void IsValidIdentifier_ValidNames_ReturnsTrue()
    {
        var renamer = new VariableRenamer("x", "y");

        Assert.That(renamer.IsValidIdentifier("validName"), Is.True);
        Assert.That(renamer.IsValidIdentifier("_private"),  Is.True);
        Assert.That(renamer.IsValidIdentifier("var123"),    Is.True);
    }

    /// <summary>
    /// Test 10. IsValidIdentifier повертає false для неприпустимих назв.
    /// </summary>
    [Test]
    public void IsValidIdentifier_InvalidNames_ReturnsFalse()
    {
        var renamer = new VariableRenamer("x", "y");

        Assert.That(renamer.IsValidIdentifier("1bad"),      Is.False);
        Assert.That(renamer.IsValidIdentifier(""),          Is.False);
        Assert.That(renamer.IsValidIdentifier("bad-name"),  Is.False);
    }
}
