using NUnit.Framework;
using CppRefactoring;

namespace CppRefactoring.Tests;

[TestFixture]
public class BlockFormatterTests
{
    /// <summary>
    /// Test 1. Стиль Allman: відкриваюча дужка переноситься на новий рядок.
    /// </summary>
    [Test]
    public void Apply_AllmanStyle_OpeningBraceOnNewLine()
    {
        var source    = new SourceCode("void foo() {\n    int x = 1;\n}");
        var formatter = new BlockFormatter(BraceStyle.Allman, 4);
        var result    = formatter.Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("void foo()\n{"));
    }

    /// <summary>
    /// Test 2. Стиль K&amp;R: відкриваюча дужка залишається в кінці рядка.
    /// </summary>
    [Test]
    public void Apply_KAndRStyle_OpeningBraceOnSameLine()
    {
        var source    = new SourceCode("void foo()\n{\n    int x = 1;\n}");
        var formatter = new BlockFormatter(BraceStyle.KAndR, 4);
        var result    = formatter.Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("void foo() {"));
    }

    /// <summary>
    /// Test 3. Нерівний відступ (3 пробіли) нормалізується до 4 пробілів.
    /// </summary>
    [Test]
    public void Apply_InconsistentIndentation_NormalizedTo4Spaces()
    {
        var source    = new SourceCode("void foo() {\n   int x = 1;\n}");
        var formatter = new BlockFormatter(BraceStyle.KAndR, 4);
        var result    = formatter.Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("    int x = 1;"));
    }

    /// <summary>
    /// Test 4. Порожній блок форматується коректно у стилі Allman.
    /// </summary>
    [Test]
    public void Apply_EmptyBlock_FormatsCorrectly()
    {
        var source    = new SourceCode("void foo(){}");
        var formatter = new BlockFormatter(BraceStyle.Allman, 4);
        var result    = formatter.Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("void foo()\n{\n}"));
    }

    /// <summary>
    /// Test 5. Блок if-else форматується правильно у стилі Allman.
    /// </summary>
    [Test]
    public void Apply_IfElseBlock_AllmanFormatted()
    {
        var source    = new SourceCode("if(x > 0){y=1;}else{y=-1;}");
        var formatter = new BlockFormatter(BraceStyle.Allman, 4);
        var result    = formatter.Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("if (x > 0)\n{"));
    }

    /// <summary>
    /// Test 6. Вкладений блок (цикл всередині функції) має подвійний відступ.
    /// </summary>
    [Test]
    public void Apply_NestedBlocks_DoubleIndented()
    {
        string code   = "void foo() {\nfor(int i=0;i<10;i++){\nint x=i;\n}\n}";
        var formatter = new BlockFormatter(BraceStyle.KAndR, 4);
        var result    = formatter.Apply(new SourceCode(code));

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("        int x=i;"));
    }

    /// <summary>
    /// Test 7. Застосування до вже відформатованого коду не змінює його.
    /// Метод є ідемпотентним.
    /// </summary>
    [Test]
    public void Apply_AlreadyFormatted_ReturnsUnchanged()
    {
        string code   = "void foo() {\n    int x = 1;\n}";
        var formatter = new BlockFormatter(BraceStyle.KAndR, 4);
        var result    = formatter.Apply(new SourceCode(code));

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Is.EqualTo(code));
    }

    /// <summary>
    /// Test 8. Вміст блоку (код всередині дужок) зберігається незміненим.
    /// </summary>
    [Test]
    public void Apply_PreservesInnerCode()
    {
        var source    = new SourceCode("int add(int a,int b){return a+b;}");
        var formatter = new BlockFormatter(BraceStyle.Allman, 4);
        var result    = formatter.Apply(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ResultCode, Does.Contain("return a+b;"));
    }

    /// <summary>
    /// Test 9. NormalizeIndentation замінює символ табуляції на IndentSize пробілів.
    /// </summary>
    [Test]
    public void NormalizeIndentation_TabToSpaces_Converted()
    {
        var formatter = new BlockFormatter(BraceStyle.KAndR, 4);
        string result = formatter.NormalizeIndentation("\tint x = 1;");

        Assert.That(result, Is.EqualTo("    int x = 1;"));
    }

    /// <summary>
    /// Test 10. Синтаксично некоректний код (незакрита дужка) призводить
    /// до Success = false з непорожнім повідомленням про помилку.
    /// </summary>
    [Test]
    public void Apply_MalformedCode_ReturnsFailure()
    {
        var source    = new SourceCode("void foo(");
        var formatter = new BlockFormatter(BraceStyle.KAndR, 4);
        var result    = formatter.Apply(source);

        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }
}
