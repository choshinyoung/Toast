namespace Toast.Tests;

public class BuiltInTests : BaseTest
{
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
}
