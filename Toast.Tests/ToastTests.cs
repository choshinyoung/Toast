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

    [Fact]
    public void TestParameterlessFunctionExecution()
    {
        var context = new Context();

        // Define parameterless function: var a = () => 1
        var tokens = Lexer.Tokenize("var a = () => 1");
        var ast = Parser.Parse(tokens, _toast.GetInfixInfo, _toast.IsPrefix);
        _toast.Evaluate(ast, context);

        // 1. Direct variable call 'a' should evaluate and return 1
        var valA = Evaluate("a", context);
        Assert.Equal(1, valA);

        // 2. Parenthesized '(a)' should also evaluate to 1 (rollback of suppression)
        var valParenA = Evaluate("(a)", context);
        Assert.Equal(1, valParenA);

        // 3. Quoted '`a' should evaluate to the FunctionValue itself
        var valQuoteA = Evaluate("`a", context);
        Assert.IsType<FunctionValue>(valQuoteA);

        // 4. Quoted with explicit call '`a()' should evaluate and return 1
        var valQuoteACall = Evaluate("`a()", context);
        Assert.Equal(1, valQuoteACall);

        // 5. Direct call 'a()' should evaluate and return 1
        var valACall = Evaluate("a()", context);
        Assert.Equal(1, valACall);
    }

    [Fact]
    public void TestMultipleArgumentsCalls()
    {
        var context = new Context();

        // Define function with multiple parameters: var b = (x, y) => x + y
        var tokens = Lexer.Tokenize("var b = (x, y) => x + y");
        var ast = Parser.Parse(tokens, _toast.GetInfixInfo, _toast.IsPrefix);
        _toast.Evaluate(ast, context);

        // 1. Calling with space-separated args 'b 1 2' should evaluate to 3
        var valSpace = Evaluate("b 1 2", context);
        Assert.Equal(3, valSpace);

        // 2. Calling with parenthesized comma-separated args 'b(1, 2)' should also evaluate to 3
        var valParen = Evaluate("b(1, 2)", context);
        Assert.Equal(3, valParen);
    }

    [Fact]
    public void TestBuiltInCommandsAsFunctions()
    {
        var context = new Context();

        // 1. Quoted operator '`(+)' should evaluate to a Command object
        var valQuotePlus = Evaluate("`(+)", context);
        Assert.IsType<Command>(valQuotePlus);

        // 2. Quoted operator '`(+)' invoked with two space-separated arguments should return 3
        var valSpacePlus = Evaluate("`(+) 1 2", context);
        Assert.Equal(3, valSpacePlus);

        // 3. Quoted operator '`(+)' invoked with parenthesized arguments should return 3
        var valParenPlus = Evaluate("`(+)(1, 2)", context);
        Assert.Equal(3, valParenPlus);

        // 4. (true) and (false) should evaluate to boolean values (no suppression)
        var valParenTrue = Evaluate("(true)", context);
        Assert.Equal(true, valParenTrue);

        var valParenFalse = Evaluate("(false)", context);
        Assert.Equal(false, valParenFalse);

        // 5. Quoted '`true' should evaluate to a Command object
        var valQuoteTrue = Evaluate("`true", context);
        Assert.IsType<Command>(valQuoteTrue);

        // 6. Invoking '`true()' should return true
        var valQuoteTrueCall = Evaluate("`true()", context);
        Assert.Equal(true, valQuoteTrueCall);
    }

    [Fact]
    public void TestBlockAsParameterlessFunction()
    {
        var context = new Context();
        // A block '{ 42 }' itself should evaluate to a FunctionValue
        var valBlock = Evaluate("{ 42 }", context);
        var funcVal = Assert.IsType<FunctionValue>(valBlock);
        Assert.Empty(funcVal.Parameters);

        // We can execute it
        Assert.Equal(42, funcVal.Execute([]));

        // Storing a block in a variable and retrieving it:
        // var b = { 10 }
        // Evaluating 'b' directly should execute it automatically because it is a parameterless function!
        Evaluate("var b = { 10 }", context);
        Assert.Equal(10, Evaluate("b", context));
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
        var context = new Context();
        Evaluate("var x = 0", context);
        // The true branch should run, but the false branch (which assigns x = 5) should NOT run!
        Evaluate("if (true) 10 else (x = 5)", context);
        Assert.Equal(0, Evaluate("x", context));

        // The false branch should run, but the true branch (which assigns x = 5) should NOT run!
        Evaluate("if (false) (x = 5) else 20", context);
        Assert.Equal(0, Evaluate("x", context));
    }
}
