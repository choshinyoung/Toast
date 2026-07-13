namespace Toast.Tests;

public class ControlFlowTests : BaseTest
{
    [Fact]
    public void TestConditionals()
    {
        AssertResult("if (true) { 100 } else { 200 }", 100);
        AssertResult("if (false) { 100 } else { 200 }", 200);

        var context = new Context(_toast);
        AssertResult("var cond = true\n if (cond) { 10 } else { 20 }", 10, context);
    }

    [Fact]
    public void TestConditionalSyntaxExtensions()
    {
        // 1. Basic if-else without braces
        AssertResult("if (true) 100 else 200", 100);
        AssertResult("if (false) 100 else 200", 200);

        // 2. else if chain without braces
        AssertResult("if (false) 100 else if (true) 200 else 300", 200);
        AssertResult("if (false) 100 else if (false) 200 else 300", 300);

        // 3. Verify inactive branch is NOT eagerly evaluated
        var context = new Context(_toast);
        Evaluate("var x = 0", context);
        // The true branch should run, but the false branch (which assigns x = 5) should NOT run!
        Evaluate("if (true) 10 else (x = 5)", context);
        Assert.Equal(new NumberValue(0), Evaluate("x", context));

        // The false branch should run, but the true branch (which assigns x = 5) should NOT run!
        Evaluate("if (false) (x = 5) else 20", context);
        Assert.Equal(new NumberValue(0), Evaluate("x", context));
    }

    [Fact]
    public void TestLoops()
    {
        var context = new Context(_toast);
        // Test while loop
        Evaluate("var x = 0", context);
        Evaluate("while (x < 5) { x = x + 1 }", context);
        Assert.Equal(new NumberValue(5), Evaluate("x", context));

        // Test for loop
        Evaluate("var sum = 0", context);
        Evaluate("for (1 to 4) ((i) => sum = sum + i)", context);
        Assert.Equal(new NumberValue(10), Evaluate("sum", context));
    }
}
