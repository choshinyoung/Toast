namespace Toast.Tests;

public class ErrorTests : BaseTest
{
    [Fact]
    public void TestErrorValuePropertiesAndHierarchy()
    {
        var context = new Context(_toast);

        // 1. Error creation & Toast language is check
        Evaluate("var synErr = Error(\"SyntaxError\", \"Unmatched bracket\", 2, 5)", context);
        AssertResult("synErr.message", "Unmatched bracket", context);
        AssertResult("synErr.line", 2.0, context);
        AssertResult("synErr.column", 5.0, context);
        AssertResult("synErr.errorType", "SyntaxError", context);

        AssertResult("synErr is Error", true, context);
        AssertResult("synErr is object", true, context);

        // 2. TypeError creation & is check
        Evaluate("var typeErr = Error(\"TypeError\", \"Type mismatch\", 1, 10)", context);
        AssertResult("typeErr.errorType", "TypeError", context);
        AssertResult("typeErr is Error", true, context);
    }

    [Fact]
    public void TestLexerAndParserLineColumnTrackingInException()
    {
        var context = new Context(_toast);

        // Syntax Error on line 2, column 1
        var ex = Assert.Throws<ToastException>(() =>
        {
            Evaluate("var x = 10\nvar y =", context);
        });

        Assert.Equal("SyntaxError", ex.Error.ErrorType);
        Assert.Equal(2, ex.Error.Location.Line);
        Assert.True(ex.Error.Location.Column >= 1);
    }

    [Fact]
    public void TestRuntimeTypeErrorLineColumnTracking()
    {
        var context = new Context(_toast);

        // Type error on line 2
        var ex = Assert.Throws<ToastException>(() =>
        {
            Evaluate("var a: number = 10\na = \"hello\"", context);
        });

        Assert.Equal("TypeError", ex.Error.ErrorType);
        Assert.Equal(2, ex.Error.Location.Line);
    }

    [Fact]
    public void TestUndefinedVariableRuntimeErrorLineColumnTracking()
    {
        var context = new Context(_toast);

        var ex = Assert.Throws<ToastException>(() =>
        {
            Evaluate("var x = 10\nvar y = undefinedVar + 5", context);
        });

        Assert.Equal("RuntimeError", ex.Error.ErrorType);
        Assert.Equal(2, ex.Error.Location.Line);
    }

    [Fact]
    public void TestThrowTryCatch()
    {
        var context = new Context(_toast);

        // 1. throw Error object and catch with lambda
        Evaluate(
            "var res1 = try { throw (Error(\"something went wrong\")) } catch (err) => { err.message }",
            context
        );
        AssertResult("res1", "something went wrong", context);

        // 2. try block without exception returns result
        Evaluate("var res2 = try { 10 + 20 } catch (err) => { 0 }", context);
        AssertResult("res2", 30.0, context);

        // 3. throw custom Error object and catch with explicit parameter
        Evaluate(
            "var res3 = try { throw (Error(\"CustomError\", \"division by zero\", 5, 10)) } catch (err) => { err.errorType }",
            context
        );
        AssertResult("res3", "CustomError", context);

        // 4. Catching language runtime error (e.g. undefined variable)
        Evaluate(
            "var res4 = try { var val = nonExistentVariable + 1 } catch (err) => { err.errorType }",
            context
        );
        AssertResult("res4", "RuntimeError", context);

        // 5. Throwing non-Error value raises TypeError
        var ex = Assert.Throws<ToastException>(() =>
        {
            Evaluate("throw \"not an error object\"", context);
        });
        Assert.Equal("TypeError", ex.Error.ErrorType);
    }

    [Fact]
    public void TestDivisionByZeroRuntimeError()
    {
        var context = new Context(_toast);

        var ex = Assert.Throws<ToastException>(() =>
        {
            Evaluate("1 / 0", context);
        });

        Assert.Equal("RuntimeError", ex.Error.ErrorType);
        Assert.Equal("Division by zero.", ex.Error.Message);
    }
}
