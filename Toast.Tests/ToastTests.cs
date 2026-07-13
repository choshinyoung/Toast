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
        context ??= new Context(_toast);
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
        var context = new Context(_toast);

        // var x
        var tokens = Lexer.Tokenize("var x");
        var ast = Parser.Parse(tokens, _toast.GetInfixInfo, _toast.IsPrefix);
        var xAddr = _toast.Evaluate(ast, context);
        Assert.IsType<MemoryAddress>(xAddr);

        // Assignment & Retrieval
        var ctxAss = new Context(_toast);
        AssertResult("var x = 42", 42, ctxAss);
        AssertResult("x", 42, ctxAss);
    }

    [Fact]
    public void TestPointersAndDereferences()
    {
        var context = new Context(_toast);
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

        var context = new Context(_toast);
        AssertResult("var cond = true\n if (cond) { 10 } else { 20 }", 10, context);
    }

    [Fact]
    public void TestInfixIdentifiers()
    {
        AssertResult("1 to 5", new List<int> { 1, 2, 3, 4, 5 });

        var context = new Context(_toast);
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
        var context = new Context(_toast);

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
        var context = new Context(_toast);

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
        var context = new Context(_toast);

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
        var context = new Context(_toast);
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
        var context = new Context(_toast);
        Evaluate("var x = 0", context);
        // The true branch should run, but the false branch (which assigns x = 5) should NOT run!
        Evaluate("if (true) 10 else (x = 5)", context);
        Assert.Equal(0, Evaluate("x", context));

        // The false branch should run, but the true branch (which assigns x = 5) should NOT run!
        Evaluate("if (false) (x = 5) else 20", context);
        Assert.Equal(0, Evaluate("x", context));
    }

    [Fact]
    public void TestListToStringConverter()
    {
        // Get the converter from the toaster
        var sourceTarget = (ToastType.List, ToastType.String);
        Assert.True(_toast.Converters.TryGetValue(sourceTarget, out var converter));

        // Test normal list
        var list = new List<object> { 1, 2, 3 };
        var str = converter.ConvertFunc(_toast.GlobalContext, list);
        Assert.Equal("[1, 2, 3]", str);

        // Test nested list
        var nestedList = new List<object>
        {
            1,
            new List<object> { 2, 3 },
            4,
        };
        var nestedStr = converter.ConvertFunc(_toast.GlobalContext, nestedList);
        Assert.Equal("[1, [2, 3], 4]", nestedStr);
    }

    [Fact]
    public void TestBasicCommandsAndConverters()
    {
        // 1. Math Operators
        AssertResult("5 + 10", 15);
        AssertResult("20 - 5", 15.0);
        AssertResult("3 * 5", 15);
        AssertResult("30 / 2", 15.0);
        AssertResult("17 % 5", 2.0);
        AssertResult("2 ** 3", 8.0);
        AssertResult("floorDiv 15 2", 7);

        // 2. Relational / Logical Operators
        AssertResult("5 == 5", true);
        AssertResult("5 == 6", false);
        AssertResult("10 > 5", true);
        AssertResult("3 < 5", true);
        AssertResult("5 >= 5", true);
        AssertResult("4 <= 5", true);
        AssertResult("true && false", false);
        AssertResult("false || true", true);
        AssertResult("!false", true);

        // 3. Bitwise Operators
        AssertResult("5 & 3", 1);
        AssertResult("5 | 3", 7);
        AssertResult("5 ^ 3", 6);
        AssertResult("~5", -6);
        AssertResult("2 << 3", 16);
        AssertResult("16 >> 2", 4);

        // 4. Converters
        AssertResult("print 123", null);
        AssertResult("\"123\" as integer", 123);
        AssertResult("\"123.45\" as float", 123.45);
        AssertResult("\"true\" as boolean", true);
        AssertResult("\"abc\" as list", new List<string> { "a", "b", "c" });

        // 5. String Helper Functions
        AssertResult("split \"a,b,c\" \",\"", new List<string> { "a", "b", "c" });
        AssertResult("reverse \"abc\"", "cba");
        AssertResult("startsWith \"abc\" \"ab\"", true);
        AssertResult("endsWith \"abc\" \"bc\"", true);
        AssertResult("contains \"abc\" \"b\"", true);
        AssertResult("trim \"  abc  \"", "abc");
        AssertResult("substring \"abcdef\" 2 3", "cde");
        AssertResult("join \"-\" (1 to 3)", "1-2-3");
        AssertResult("replace \"abc\" \"b\" \"z\"", "azc");
        AssertResult("toUpper \"abc\"", "ABC");
        AssertResult("toLower \"ABC\"", "abc");

        // 6. List Helper Functions
        AssertResult("len (1 to 5)", 5);
        AssertResult("indexOf (1 to 5) 3", 2);
        AssertResult("range 5 3", new List<int> { 5, 6, 7 });
        AssertResult("member 2 (10 to 15)", 12);
        AssertResult("(10 to 15) # 2", 12);
        AssertResult("\"hello\" # 1", "e");
        AssertResult("map (1 to 3) ((x) => x * 2)", new List<object> { 2, 4, 6 });
        AssertResult("filter (1 to 5) ((x) => x > 3)", new List<object> { 4, 5 });
        AssertResult("combine (1 to 2) (3 to 4)", new List<int> { 1, 2, 3, 4 });
        AssertResult("append (1 to 2) 3", new List<object> { 1, 2, 3 });
        AssertResult("remove (1 to 3) 2", new List<object> { 1, 3 });
    }

    [Fact]
    public void TestLoops()
    {
        var context = new Context(_toast);
        // Test while loop
        Evaluate("var x = 0", context);
        Evaluate("while (x < 5) { var x = x + 1 }", context);
        Assert.Equal(5, Evaluate("x", context));

        // Test for loop
        Evaluate("var sum = 0", context);
        Evaluate("for (1 to 4) ((i) => var sum = sum + i)", context);
        Assert.Equal(10, Evaluate("sum", context));
    }

    [Fact]
    public void TestPointerEscape()
    {
        var context = new Context(_toast);
        Evaluate("var makePointer = () => { var local = 42\n (var local) }", context);
        Evaluate("var ptr = makePointer()", context);
        Assert.Equal(42, Evaluate("*ptr", context));
        Evaluate("ptr = 100", context);
        Assert.Equal(100, Evaluate("*ptr", context));
    }

    [Fact]
    public void TestMultilineExpression()
    {
        // 1. Enclosed in parentheses (Alternative 2)
        AssertResult("(2\n* 3)", 6);
        AssertResult("(2\n\n*\n\n3)", 6);

        // 2. Operator at the end of the line (continues naturally)
        AssertResult("2 *\n3", 6);
        AssertResult("2 *\n\n3", 6);

        // 3. Top-level without parentheses and operator at the start of the line
        // should evaluate as two separate statements (resolves the *ptr ambiguity).
        var context = new Context(_toast);
        Evaluate("var x = 10", context);
        Evaluate("var ptr = (var x)", context);
        // *ptr on a new line is a separate statement, not multiplied to 10
        Evaluate("var dummy = 10\n*ptr", context);
        Assert.Equal(10, Evaluate("*ptr", context));
    }

    [Fact]
    public void TestPipelineOperator()
    {
        // 1. Simple function call
        AssertResult("5 |> ((x) => x * 2)", 10);

        // 2. Command call
        AssertResult("\"  abc  \" |> trim", "abc");

        // 3. Command with arguments
        AssertResult("\"a,b,c\" |> split \",\"", new List<string> { "a", "b", "c" });

        // 4. Chained pipelines
        AssertResult("\"  a,b,c  \" |> trim |> split \",\"", new List<string> { "a", "b", "c" });
    }
}
