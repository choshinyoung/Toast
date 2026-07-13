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
}
