namespace Toast.Tests;

public class InterpolationTests : BaseTest
{
    [Fact]
    public void TestBasicStringInterpolation()
    {
        var context = new Context(_toast);
        Evaluate("var name = \"World\"", context);
        AssertResult("\"Hello, {name}!\"", "Hello, World!", context);
    }

    [Fact]
    public void TestExpressionStringInterpolation()
    {
        var context = new Context(_toast);
        Evaluate("var a = 10", context);
        Evaluate("var b = 20", context);
        AssertResult("\"Result: {a + b}\"", "Result: 30", context);
    }

    [Fact]
    public void TestMultipleInterpolatedExpressions()
    {
        var context = new Context(_toast);
        Evaluate("var user = \"Alice\"", context);
        Evaluate("var count = 3", context);
        AssertResult("\"{user} has {count} items\"", "Alice has 3 items", context);
    }

    [Fact]
    public void TestEscapedBraces()
    {
        var context = new Context(_toast);
        Evaluate("var x = 5", context);
        AssertResult("\"Escaped: \\{x\\} = {x}\"", "Escaped: {x} = 5", context);
    }

    [Fact]
    public void TestNoInterpolationPlainString()
    {
        var context = new Context(_toast);
        AssertResult("\"Plain string without braces\"", "Plain string without braces", context);
    }

    [Fact]
    public void TestEscapeSequences()
    {
        var context = new Context(_toast);
        AssertResult("\"line1\\nline2\"", "line1\nline2", context);
        AssertResult("\"quote: \\\" \\' \\\\\"", "quote: \" ' \\", context);
        AssertResult("\"braces: \\{ \\}\"", "braces: { }", context);

        // Invalid escape sequence like \t should throw
        Assert.Throws<InvalidOperationException>(() => Evaluate("\"invalid \\t\"", context));
    }

    [Fact]
    public void TestNestedStringInterpolation()
    {
        var context = new Context(_toast);
        AssertResult("\"Hello, {\"world!\"}\"", "Hello, world!", context);
    }
}
