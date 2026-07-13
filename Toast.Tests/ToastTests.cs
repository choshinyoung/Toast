namespace Toast.Tests;

public class ToastTests
{
    private readonly Toaster _toast = new(useBuiltIn: true);

    private ToastObject Evaluate(string source, Context context)
    {
        var tokens = Lexer.Tokenize(source);
        var ast = Parser.Parse(tokens, _toast.GetInfixInfo, _toast.IsPrefix);
        return _toast.Evaluate(ast, context);
    }

    private void AssertResult(string source, object? expected, Context? context = null)
    {
        context ??= new Context(_toast);
        var result = Evaluate(source, context);

        if (expected is string s && s == "IdentifierNode")
        {
            Assert.True(result is IdentifierValue);
        }
        else if (expected is System.Collections.IEnumerable enumerable && expected is not string)
        {
            Assert.NotNull(result);
            var listVal = Assert.IsType<ListValue>(result);
            var expectedList = enumerable.Cast<object>().Select(UnifyExpected).ToList();
            var resultList = listVal.Elements;
            Assert.Equal(expectedList.Count, resultList.Count);
            for (int i = 0; i < expectedList.Count; i++)
            {
                Assert.Equal(expectedList[i], resultList[i]);
            }
        }
        else
        {
            Assert.Equal(UnifyExpected(expected), result);
        }
    }

    private ToastObject UnifyExpected(object? expected)
    {
        if (expected == null)
            return NullValue.Instance;
        if (expected is string s)
            return new StringValue(s);
        if (expected is int i)
            return new NumberValue(i);
        if (expected is double d)
            return new NumberValue(d);
        if (expected is float f)
            return new NumberValue(f);
        if (expected is bool b)
            return new BoolValue(b);
        if (expected is ToastObject to)
            return to;
        throw new NotSupportedException($"Unification not supported for type {expected.GetType()}");
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
        Assert.IsType<IdentifierValue>(xAddr);

        // Assignment & Retrieval
        var ctxAss = new Context(_toast);
        AssertResult("var x = 42", 42, ctxAss);
        AssertResult("x", 42, ctxAss);
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
        Assert.Equal(new NumberValue(1), valA);

        // 2. Parenthesized '(a)' should also evaluate to 1 (rollback of suppression)
        var valParenA = Evaluate("(a)", context);
        Assert.Equal(new NumberValue(1), valParenA);

        // 3. Quoted '`a' should evaluate to the FunctionValue itself
        var valQuoteA = Evaluate("`a", context);
        Assert.IsType<FunctionValue>(valQuoteA);

        // 4. Quoted with explicit call '`a()' should evaluate and return 1
        var valQuoteACall = Evaluate("`a()", context);
        Assert.Equal(new NumberValue(1), valQuoteACall);

        // 5. Direct call 'a()' should evaluate and return 1
        var valACall = Evaluate("a()", context);
        Assert.Equal(new NumberValue(1), valACall);
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
        Assert.Equal(new NumberValue(3), valSpace);

        // 2. Calling with parenthesized comma-separated args 'b(1, 2)' should also evaluate to 3
        var valParen = Evaluate("b(1, 2)", context);
        Assert.Equal(new NumberValue(3), valParen);
    }

    [Fact]
    public void TestBuiltInCommandsAsFunctions()
    {
        var context = new Context(_toast);

        // 1. Quoted operator '`(+)' should evaluate to a CommandValue object
        var valQuotePlus = Evaluate("`(+)", context);
        Assert.IsType<CommandValue>(valQuotePlus);

        // 2. Quoted operator '`(+)' invoked with two space-separated arguments should return 3
        var valSpacePlus = Evaluate("`(+) 1 2", context);
        Assert.Equal(new NumberValue(3), valSpacePlus);

        // 3. Quoted operator '`(+)' invoked with parenthesized arguments should return 3
        var valParenPlus = Evaluate("`(+)(1, 2)", context);
        Assert.Equal(new NumberValue(3), valParenPlus);

        // 4. (true) and (false) should evaluate to boolean values (no suppression)
        var valParenTrue = Evaluate("(true)", context);
        Assert.Equal(new BoolValue(true), valParenTrue);

        var valParenFalse = Evaluate("(false)", context);
        Assert.Equal(new BoolValue(false), valParenFalse);

        // 5. Quoted '`true' should evaluate to a CommandValue object
        var valQuoteTrue = Evaluate("`true", context);
        Assert.IsType<CommandValue>(valQuoteTrue);

        // 6. Invoking '`true()' should return true
        var valQuoteTrueCall = Evaluate("`true()", context);
        Assert.Equal(new BoolValue(true), valQuoteTrueCall);
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
        Assert.Equal(new NumberValue(42), funcVal.Execute([]));

        // Storing a block in a variable and retrieving it:
        // var b = { 10 }
        // Evaluating 'b' directly should execute it automatically because it is a parameterless function!
        Evaluate("var b = { 10 }", context);
        Assert.Equal(new NumberValue(10), Evaluate("b", context));
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
    public void TestListToStringConverter()
    {
        // Get the converter from the toaster
        var sourceTarget = (ToastType.List, ToastType.String);
        Assert.True(_toast.Converters.TryGetValue(sourceTarget, out var converter));

        // Test normal list
        var list = new ListValue(
            new List<ToastObject> { new NumberValue(1), new NumberValue(2), new NumberValue(3) }
        );
        var str = converter.ConvertFunc(_toast.GlobalContext, list);
        Assert.Equal(new StringValue("[1, 2, 3]"), str);

        // Test nested list
        var nestedList = new ListValue(
            new List<ToastObject>
            {
                new NumberValue(1),
                new ListValue(new List<ToastObject> { new NumberValue(2), new NumberValue(3) }),
                new NumberValue(4),
            }
        );
        var nestedStr = converter.ConvertFunc(_toast.GlobalContext, nestedList);
        Assert.Equal(new StringValue("[1, [2, 3], 4]"), nestedStr);
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
        AssertResult("sqrt 16", 4.0);

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
        AssertResult("print 123", NullValue.Instance);
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
        Evaluate("while (x < 5) { x = x + 1 }", context);
        Assert.Equal(new NumberValue(5), Evaluate("x", context));

        // Test for loop
        Evaluate("var sum = 0", context);
        Evaluate("for (1 to 4) ((i) => sum = sum + i)", context);
        Assert.Equal(new NumberValue(10), Evaluate("sum", context));
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

    [Fact]
    public void TestObjectAndMemberAccess()
    {
        var context = new Context(_toast);

        // 1. Test Object literal creation using type definition
        Evaluate("var ObjMaker = type { var x = 10\n var y = 20 }", context);
        var obj1 = Evaluate("ObjMaker", context);
        var objVal1 = Assert.IsType<ObjectValue>(obj1);
        Assert.Equal(new NumberValue(10), objVal1.Context.GetValue("x"));
        Assert.Equal(new NumberValue(20), objVal1.Context.GetValue("y"));

        // 2. Test Member dot access
        Evaluate("var p = ObjMaker", context);
        AssertResult("p.x", 10, context);
        AssertResult("p.y", 20, context);

        // 3. Test Member assignment
        Evaluate("p.x = 42", context);
        AssertResult("p.x", 42, context);

        // 4. Test Constructor-like Function with type
        Evaluate("var Point = type (x, y) => { var this = 0 }", context);
        Evaluate("var p2 = Point 100 200", context);
        AssertResult("p2.x", 100, context);
        AssertResult("p2.y", 200, context);

        // 5. Test Parameterless member function execution and suppression
        Evaluate(
            @"var Point2 = type (x, y) => {
          var magnitude = {
            sqrt(x * x + y * y)
          }
          var normalize = {
            var mag = magnitude
            Point2 (x / mag) (y / mag)
          }
        }",
            context
        );
        Evaluate("var p3 = Point2 3 4", context);
        AssertResult("p3.magnitude", 5.0, context);

        var rawFunc = Evaluate("`(p3.magnitude)", context);
        Assert.IsType<FunctionValue>(rawFunc);
    }
}
