namespace Toast.Tests;

public abstract class BaseTest
{
    protected readonly Toaster _toast = new(useBuiltIn: true);

    protected ToastObject Evaluate(string source, Context context)
    {
        var tokens = Lexer.Tokenize(source);
        var ast = Parser.Parse(tokens, _toast.GetInfixInfo, _toast.IsPrefix);
        return _toast.Evaluate(ast, context);
    }

    protected void AssertResult(string source, object? expected, Context? context = null)
    {
        context ??= new Context(_toast);
        var result = Evaluate(source, context);

        if (expected is string s && s == "IdentifierValue")
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

    protected static ToastObject UnifyExpected(object? expected)
    {
        if (expected == null)
            return NullValue.Instance;
        if (expected is string s)
            return new StringValue(s);
        if (expected is int i)
            return new NumberValue(i);
        if (expected is long l)
            return new NumberValue(l);
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
}
