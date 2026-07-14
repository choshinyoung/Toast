namespace Toast.Tests;

public class VariableTests : BaseTest
{
    [Fact]
    public void TestVariableAssignment()
    {
        var context = new Context(_toast);

        // var x
        var tokens = Lexer.Tokenize("var x");
        var ast = Parser.Parse(tokens, _toast.GetInfixInfo, _toast.IsPrefix);
        var xAddr = _toast.Evaluate(ast, context);
        Assert.IsType<ReferenceValue>(xAddr);

        // Assignment & Retrieval
        var ctxAss = new Context(_toast);
        AssertResult("var x = 42", 42, ctxAss);
        AssertResult("x", 42, ctxAss);
    }

    [Fact]
    public void TestDuplicateVariableDeclarationThrows()
    {
        var context = new Context(_toast);
        Evaluate("var x = 10", context);
        Assert.Throws<InvalidOperationException>(() => Evaluate("var x = 20", context));
    }

    [Fact]
    public void TestQuotedVariableReturnsReference()
    {
        var context = new Context(_toast);
        Evaluate("var x = 10", context);
        var quotedX = Evaluate("`x", context);
        var refVal = Assert.IsType<ReferenceValue>(quotedX);
        Assert.Equal(new NumberValue(10), refVal.Target.GetValue());
        Assert.Equal("(ref: x)", refVal.ToString());

        // Assign to variable via reference
        refVal.Target.SetValue(new NumberValue(20));
        AssertResult("x", 20, context);
    }

    [Fact]
    public void TestQuoteInvalidOperandThrows()
    {
        var context = new Context(_toast);
        Assert.Throws<InvalidOperationException>(() => Evaluate("`10", context));
        Assert.Throws<InvalidOperationException>(() => Evaluate("`(1 + 2)", context));
    }
}
