namespace Toast.Tests;

public class ParserTests : BaseTest
{
    [Fact]
    public void TestOperatorPrecedence()
    {
        AssertResult("1 + 2 * 3", 7);
        AssertResult("1 * 2 + 3", 5);
        AssertResult("10 - 2 - 3", 5.0);
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
}
