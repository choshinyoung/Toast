namespace Toast.Tests;

public class ToastTests
{
    private readonly Toaster _toast = new(useBuiltIn: true);

    private object? Evaluate(string source, Context context)
    {
        var tokens = Lexer.Tokenize(source);
        var ast = Parser.Parse(tokens, _toast.GetInfixInfo, _toast.IsPrefix);
        return _toast.Evaluate(ast, context);
    }

    private void AssertResult(string source, object? expected, Context? context = null)
    {
        context ??= new Context();
        var result = Evaluate(source, context);

        if (expected is string s && s == "MemoryAddress")
        {
            Assert.IsType<MemoryAddress>(result);
        }
        else if (expected is System.Collections.IEnumerable enumerable && expected is not string)
        {
            Assert.NotNull(result);
            var expectedList = enumerable.Cast<object>().ToList();
            var resultEnumerable = Assert.IsType<System.Collections.IEnumerable>(
                result,
                exactMatch: false
            );
            var resultList = resultEnumerable.Cast<object>().ToList();
            Assert.Equal(expectedList, resultList);
        }
        else
        {
            Assert.Equal(expected, result);
        }
    }

    [Fact]
    public void TestOperatorPrecedence()
    {
        AssertResult("1 + 2 * 3", 7);
        AssertResult("1 * 2 + 3", 5);
        AssertResult("10 - 2 - 3", 5.0);
    }

    [Fact]
    public void TestVariableAssignment()
    {
        var context = new Context();

        // var x
        var tokens = Lexer.Tokenize("var x");
        var ast = Parser.Parse(tokens, _toast.GetInfixInfo, _toast.IsPrefix);
        var xAddr = _toast.Evaluate(ast, context);
        Assert.IsType<MemoryAddress>(xAddr);

        // Assignment & Retrieval
        var ctxAss = new Context();
        AssertResult("var x = 42", 42, ctxAss);
        AssertResult("x", 42, ctxAss);
    }

    [Fact]
    public void TestPointersAndDereferences()
    {
        var context = new Context();
        AssertResult("var c = 10\n var a = (var c)", "MemoryAddress", context);
        AssertResult("a = 20", 20, context);
        AssertResult("*a", 20, context);
        AssertResult("c", 20, context);
    }

    [Fact]
    public void TestConditionals()
    {
        AssertResult("if (true) { 100 } else { 200 }", 100);
        AssertResult("if (false) { 100 } else { 200 }", 200);

        var context = new Context();
        AssertResult("var cond = true\n if (cond) { 10 } else { 20 }", 10, context);
    }

    [Fact]
    public void TestInfixIdentifiers()
    {
        AssertResult("1 to 5", new List<int> { 1, 2, 3, 4, 5 });

        var context = new Context();
        AssertResult("var r = 1 to 5\n 3 in r", true, context);
        AssertResult("6 in (1 to 5)", false);
    }

    [Fact]
    public void TestExplicitCasting()
    {
        AssertResult("1 as float", 1.0);
        AssertResult("true as string", "True");
    }
}
