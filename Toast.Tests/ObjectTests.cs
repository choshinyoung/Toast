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

    [Fact]
    public void TestObjectLiteralSugar()
    {
        var context = new Context(_toast);

        // 1. 단일 라인 이중 중괄호 객체 초기화
        Evaluate("var obj = {{ x = 10, y = 20 }}", context);
        AssertResult("obj.x", 10, context);
        AssertResult("obj.y", 20, context);

        // 2. 대입을 통한 속성 변경
        Evaluate("obj.x = 42", context);
        AssertResult("obj.x", 42, context);

        // 3. 다중 라인 이중 중괄호 객체 초기화 및 var 선언 지원
        Evaluate(
            @"var obj2 = {{
                var a = 1
                var b = 2
            }}",
            context
        );
        AssertResult("obj2.a", 1, context);
        AssertResult("obj2.b", 2, context);
    }

    [Fact]
    public void TestClassAndFunctionSyntaxSugar()
    {
        var context = new Context(_toast);

        Evaluate(
            @"class Point2 (x, y) => {
  function magnitude () => sqrt(x * x + y * y)
  function normalize () => {
    var mag = magnitude
    Point2 (x / mag) (y / mag)
  }
}",
            context
        );

        Evaluate("var p3 = Point2 3 4", context);
        AssertResult("p3.magnitude", 5.0, context);

        Evaluate("var p3Norm = p3.normalize", context);
        AssertResult("p3Norm.x", 0.6, context);
        AssertResult("p3Norm.y", 0.8, context);
    }

    [Fact]
    public void TestMemberFunctionCalls()
    {
        var context = new Context(_toast);

        // Test Case 1: Point.add
        Evaluate(
            @"class Point (x, y) => {
                function add(_x, _y) => {
                    Point(x + _x, y + _y)
                }
            }",
            context
        );
        Evaluate("var p1 = Point(3, 4)", context);
        Evaluate("var p2 = p1.add 3 4", context);
        AssertResult("p2.x", 6, context);
        AssertResult("p2.y", 8, context);

        // Test Case 2: ranshi.moomin
        Evaluate(
            @"class ranshi(name) => {
                function moomin() => {
                    ""응 난 랜덤"" + name + "" 이야""
                }
            }",
            context
        );
        Evaluate("var moomin = ranshi(\"무민\")", context);
        AssertResult("moomin.moomin()", "응 난 랜덤무민 이야", context);
        AssertResult("(moomin.moomin)()", "응 난 랜덤무민 이야", context);
    }

    [Fact]
    public void TestPointChaining()
    {
        var context = new Context(_toast);
        Evaluate(
            @"class Point (x, y) => {
                function add(_x, _y) => {
                    Point(x + _x, y + _y)
                }
            }",
            context
        );
        Evaluate("var p = (Point(1, 2)).add(3, 4)", context);
        AssertResult("p.x", 4, context);
        AssertResult("p.y", 6, context);
    }

    [Fact]
    public void TestWithCommand()
    {
        var context = new Context(_toast);

        // Class Point 정의
        Evaluate(
            @"class Point (x, y) => {
            }",
            context
        );

        // 1. 단일 with 연산 테스트
        Evaluate("var p1 = Point(3, 4) with {{ x = 0 }}", context);
        AssertResult("p1.x", 0, context);
        AssertResult("p1.y", 4, context);

        // 2. with 연산 체이닝 및 불변성 검증
        Evaluate("var p2 = Point(1, 2) with {{ x = 10 }} with {{ y = 20 }}", context);
        AssertResult("p2.x", 10, context);
        AssertResult("p2.y", 20, context);

        // 원본 Point(1, 2)는 변경되지 않았는지 확인
        Evaluate("var original = Point(1, 2)", context);
        AssertResult("original.x", 1, context);
        AssertResult("original.y", 2, context);
    }
}
