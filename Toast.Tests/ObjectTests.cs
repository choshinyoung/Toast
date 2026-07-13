namespace Toast.Tests;

public class ObjectTests : BaseTest
{
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
