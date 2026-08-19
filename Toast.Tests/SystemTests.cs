namespace Toast.Tests;

public class SystemTests : BaseTest
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
        AssertResult("123 as string", "123");
        AssertResult("\"456\" as number", 456.0);
        AssertResult("\"true\" as boolean", true);
        AssertResult("\"abc\" as list", new List<string> { "a", "b", "c" });
    }

    [Fact]
    public void TestListToStringConverter()
    {
        // Get the converter from the toaster
        var sourceTarget = (ToastType.List, ToastType.String);
        Assert.True(_toast.Converters.TryGetValue(sourceTarget, out var converter));

        // Test normal list
        var list = new ListValue([new NumberValue(1), new NumberValue(2), new NumberValue(3)]);
        var str = converter.ConvertFunc(_toast.GlobalContext, list);
        Assert.Equal(new StringValue("[1, 2, 3]"), str);

        // Test nested list
        var nestedList = new ListValue([
            new NumberValue(1),
            new ListValue([new NumberValue(2), new NumberValue(3)]),
            new NumberValue(4),
        ]);
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

        var mathCtx = new Context(_toast);
        Evaluate("import \"math\"", mathCtx);
        AssertResult("math.floorDiv 15 2", 7, mathCtx);
        AssertResult("math.sqrt 16", 4.0, mathCtx);
        AssertResult("math.PI > 3.14", true, mathCtx);
        AssertResult("math.E > 2.71", true, mathCtx);

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
        AssertResult("5 is number", true);
        AssertResult("\"hello\" is string", true);
        AssertResult("5 is string", false);
        AssertResult("5 is object", false);
        AssertResult("[1, 2] is list", true);
        AssertResult("null is null", true);
        AssertResult("5 is null", false);
        AssertResult("{{ x = 10 }} is object", true);

        // 3. Bitwise Operators
        AssertResult("5 & 3", 1);
        AssertResult("5 | 3", 7);
        AssertResult("5 ^ 3", 6);
        AssertResult("~5", -6);
        AssertResult("2 << 3", 16);
        AssertResult("16 >> 2", 4);

        // 4. Converters
        AssertResult("print 123", NullValue.Instance);
        Assert.True(
            _toast.TryConvert(
                new StringValue("123"),
                ToastType.String,
                ToastType.Number,
                _toast.GlobalContext,
                out var numVal1
            )
                && ((NumberValue)numVal1).Value == 123
        );
        Assert.True(
            _toast.TryConvert(
                new StringValue("123.45"),
                ToastType.String,
                ToastType.Number,
                _toast.GlobalContext,
                out var numVal2
            )
                && ((NumberValue)numVal2).Value == 123.45
        );
        Assert.True(
            _toast.TryConvert(
                new StringValue("true"),
                ToastType.String,
                ToastType.Boolean,
                _toast.GlobalContext,
                out var boolVal
            )
                && ((BoolValue)boolVal).Value == true
        );
        Assert.True(
            _toast.TryConvert(
                new StringValue("abc"),
                ToastType.String,
                ToastType.List,
                _toast.GlobalContext,
                out var listVal
            )
                && ((ListValue)listVal).Elements.Count == 3
        );

        // 5. String Helper Functions
        // 전역 커맨드로 등록된 String 함수들은 없음 → 인스턴스 멤버(.member) 또는 전역 등록된 것만 사용
        AssertResult("\"a,b,c\".split(\",\")", new List<string> { "a", "b", "c" });
        AssertResult("\"abc\".reverse()", "cba");
        AssertResult("\"abc\".startsWith(\"ab\")", true);
        AssertResult("\"abc\".endsWith(\"bc\")", true);
        AssertResult("\"abc\".contains(\"b\")", true);
        AssertResult("\"  abc  \".trim()", "abc");
        AssertResult("\"abcdef\".substring(2, 3)", "cde");
        AssertResult("\"Hello\".substring(1, 3).reverse()", "lle");
        AssertResult("\"-\".join([\"1\", \"2\", \"3\"])", "1-2-3");
        AssertResult("\"abc\".replace(\"b\", \"z\")", "azc");
        AssertResult("\"abc\".toUpper()", "ABC");
        AssertResult("\"ABC\".toLower()", "abc");

        // 6. List Helper Functions
        AssertResult("(1 to 5).length", 5);
        AssertResult("(1 to 5).indexOf(3)", 2);
        AssertResult("(10 to 15) # 2", 12);
        AssertResult("\"hello\" # 1", "e");
        AssertResult("map (1 to 3) ((x) => x * 2)", new List<object> { 2, 4, 6 });
        AssertResult("filter (1 to 5) ((x) => x > 3)", new List<object> { 4, 5 });
        AssertResult("(1 to 2).join(3 to 4)", new List<int> { 1, 2, 3, 4 });
        AssertResult("sort(1 to 3)", new List<int> { 1, 2, 3 });
        AssertResult("sort([3, 1, 2])", new List<int> { 1, 2, 3 });
    }

    [Fact]
    public void TestListMemberAssignment()
    {
        var context = new Context(_toast);
        Evaluate("var list = [1, 2, 3]", context);

        // 1. Basic assignment
        Evaluate("list # 1 = 42", context);
        AssertResult("list # 1", 42, context);

        // 2. Add assignment (+=)
        Evaluate("list # 1 += 8", context);
        AssertResult("list # 1", 50, context);

        // 3. Sub assignment (-=)
        Evaluate("list # 1 -= 10", context);
        AssertResult("list # 1", 40, context);
    }

    [Fact]
    public void TestReduce()
    {
        // Sum: reduce (1 to 5) 0 ((acc, x) => acc + x) = 15
        AssertResult("reduce (1 to 5) 0 ((acc, x) => acc + x)", 15);

        // Product: reduce [2, 3, 4] 1 ((acc, x) => acc * x) = 24
        AssertResult("reduce [2, 3, 4] 1 ((acc, x) => acc * x)", 24);

        // String concat: reduce ["a", "b", "c"] "" ((acc, x) => acc.concat(x)) = "abc"
        AssertResult("reduce [\"a\", \"b\", \"c\"] \"\" ((acc, x) => acc.concat(x))", "abc");

        // Empty list returns initial value
        AssertResult("reduce [] 42 ((acc, x) => acc + x)", 42);
    }

    [Fact]
    public void TestPipelineWithMapFilterReduce()
    {
        // map with pipeline
        AssertResult("(1 to 3) |> map ((x) => x * 2)", new List<object> { 2, 4, 6 });

        // filter with pipeline
        AssertResult("(1 to 5) |> filter ((x) => x > 3)", new List<object> { 4, 5 });

        // reduce with pipeline
        AssertResult("(1 to 5) |> reduce 0 ((acc, x) => acc + x)", 15);

        // chained: map then filter
        AssertResult(
            "(1 to 5) |> map ((x) => x * 2) |> filter ((x) => x > 6)",
            new List<object> { 8, 10 }
        );

        // chained: filter then reduce
        AssertResult(
            "(1 to 10) |> filter ((x) => x % 2 == 0) |> reduce 0 ((acc, x) => acc + x)",
            30
        );

        // chained: map then filter then reduce
        AssertResult(
            "(1 to 5) |> map ((x) => x * 10) |> filter ((x) => x > 20) |> reduce 0 ((acc, x) => acc + x)",
            120
        );

        // String 함수들은 전역 커맨드 미등록이므로 파이프라인에서 람다로 감싸서 사용 (임시)
        AssertResult("\"abc\" |> (x) => x.toUpper()", "ABC");
        AssertResult("\"  hello  \" |> (x) => x.trim()", "hello");
        AssertResult("\"a,b,c\" |> (x) => x.split(\",\")", new List<string> { "a", "b", "c" });

        // List sort 파이프라인
        AssertResult("[3, 1, 2] |> sort", new List<int> { 1, 2, 3 });
    }

    [Fact]
    public void TestMemberGetterOperatorAndTopLevelSort()
    {
        var context = new Context(_toast);

        // 1. ..property 게터 연산자 기본 테스트
        Evaluate("var getLen = ..length", context);
        AssertResult("getLen(\"hello\")", 5, context);
        AssertResult("getLen([1, 2, 3])", 3, context);

        // 2. map과 .. 연산자 결합
        AssertResult(
            "[\"a\", \"abc\", \"de\"] |> map ..length",
            new List<int> { 1, 3, 2 },
            context
        );

        // 3. 최상위 sort 테스트
        AssertResult("[3, 1, 4, 2] |> sort", new List<int> { 1, 2, 3, 4 }, context);

        // 4. 최상위 sortAs와 .. 연산자 결합
        Evaluate(
            @"class Item(name, price) => {
            }",
            context
        );
        Evaluate(
            "var items = [Item(\"Apple\", 300), Item(\"Banana\", 100), Item(\"Cherry\", 200)]",
            context
        );
        Evaluate("var sortedItems = items |> sortAs ..price", context);
        AssertResult("sortedItems # 0 .name", "Banana", context);
        AssertResult("sortedItems # 1 .name", "Cherry", context);
        AssertResult("sortedItems # 2 .name", "Apple", context);

        // 5. 단일 객체 파이프라인과 .. 게터 결합 (e.g. item |> ..price)
        Evaluate("var singleItem = Item(\"Grape\", 500)", context);
        AssertResult("singleItem |> ..price", 500, context);
        AssertResult("singleItem |> (..name)", "Grape", context);
    }

    [Fact]
    public void TestUserPipelineExpression()
    {
        AssertResult(
            @"(1 to 10
                |> filter (x => x % 2 == 0)
                |> map (x => x * 10)
                |> reduce 0 ((acc, x) => acc + x))",
            300
        );
    }

    [Fact]
    public void TestTypeValueAndStructuralIs()
    {
        // 1. Built-in types
        AssertResult("123 is number", true);
        AssertResult("\"abc\" is string", true);
        AssertResult("true is boolean", true);
        AssertResult("[1, 2] is list", true);
        AssertResult("123 is string", false);
        AssertResult("\"abc\" is number", false);

        // Null checks
        AssertResult("null is null", true);
        AssertResult("123 is null", false);
        AssertResult("var x = null\n x is null", true);

        // 2. Custom classes structural verification
        var context = new Context(_toast);
        Evaluate("class Point(x, y) => { function add(other) => 0 }", context);
        Evaluate("class Vector(x, y) => { function add(other) => 0 }", context);
        Evaluate("class Scalar(x) => {}", context);

        Evaluate("var p = Point(1, 2)", context);

        // Exact structural match
        AssertResult("p is Point", true, context);

        // Matches Vector because it also has x, y, add
        AssertResult("p is Vector", true, context);

        // Fails Scalar because Point has x, y, add but Scalar only declares x (wait, Scalar expects x, so p has x. Does p is Scalar match? Yes! Point has x and y, so it satisfies Scalar's requirement of having x. Let's verify: does Scalar check if the object has x? Yes, and p has x. So p satisfies Scalar!)
        AssertResult("p is Scalar", true, context);

        // Fails Point for s because Scalar s only has x, missing y and add!
        Evaluate("var s = Scalar(5)", context);
        AssertResult("s is Point", false, context);

        // Verification of block-based vs parameter-based type matching
        AssertResult("p is type(x, y) => {}", true, context);
        AssertResult("p is type {\n var x\n var y\n }", true, context);
    }

    [Fact]
    public void TestDateTimeBuiltIn()
    {
        var context = new Context(_toast);
        Evaluate("import \"datetime\"", context);

        // 1. Creation and fields via datetime.now()
        Evaluate("var now = datetime.now()", context);
        AssertResult("now.year", DateTime.Now.Year, context);
        AssertResult("now.month", DateTime.Now.Month, context);
        AssertResult("now.day", DateTime.Now.Day, context);
        AssertResult("now.totalSeconds > 0", true, context);

        // 2. Verified that instance does NOT have 'now' property (prevents datetime.now.now.now)
        Assert.Throws<ToastException>(() => Evaluate("now.now", context));

        // 3. Custom Methods & Converter-based string conversion
        Evaluate("var d = datetime.now()", context);
        AssertResult("d.format \"yyyy\"", DateTime.Now.Year.ToString(), context);
        Assert.True(
            _toast.TryConvert(
                Evaluate("d", context),
                ToastType.FromName("datetime"),
                ToastType.String,
                context,
                out var convStr
            )
                && convStr is StringValue
        );

        // 4. Method Chaining
        Evaluate("var d2 = d.addDays 5", context);
        AssertResult("d2 is \"datetime\"", true, context);
    }
}
