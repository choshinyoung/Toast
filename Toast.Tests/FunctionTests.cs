namespace Toast.Tests;

public class FunctionTests : BaseTest
{
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
    public void TestPipelineOperator()
    {
        // 1. Simple function call
        AssertResult("5 |> ((x) => x * 2)", 10);

        // 2. Command call - String 함수는 전역 커맨드 미등록이므로 람다로 감싸서 사용 (임시)
        AssertResult("\"  abc  \" |> (x) => x.trim()", "abc");

        // 3. Command with arguments - 동일하게 멤버 호출 방식으로 (임시)
        AssertResult("\"a,b,c\" |> (x) => x.split(\",\")", new List<string> { "a", "b", "c" });

        // 4. Chained pipelines - 체이닝도 멤버 호출 방식으로 (임시)
        AssertResult(
            "\"  a,b,c  \" |> (x) => x.trim() |> (x) => x.split(\",\")",
            new List<string> { "a", "b", "c" }
        );
    }
}
