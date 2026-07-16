namespace Toast.Tests;

public class ObjectTests : BaseTest
{
    [Fact]
    public void TestObjectAndMemberAccess()
    {
        var context = new Context(_toast);

        // 1. Test Object literal creation using type definition
        Evaluate("var ObjMaker = type { var x = 10\n var y = 20 }", context);

        var rawType = Evaluate("`ObjMaker", context);
        var refVal = Assert.IsType<ReferenceValue>(rawType);
        var typeVal = Assert.IsType<TypeValue>(refVal.Target.GetValue());
        Assert.Equal("(type: { x, y })", typeVal.ToString());

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

    [Fact]
    public void TestRuntimeTypeEnforcement()
    {
        var context = new Context(_toast);

        // 1. 변수 선언 시 타입 매칭 및 대입 위반 차단
        Evaluate("var x: number = 10", context);
        AssertResult("x", 10, context);

        Assert.Throws<InvalidOperationException>(() =>
        {
            Evaluate("x = \"hello\"", context);
        });

        // 2. 클래스 인스턴스 타입 제약 및 멤버 대입 위반 차단
        Evaluate(
            @"class Point (x: number, y: number) => {
            }",
            context
        );
        Evaluate("var p: Point = Point(3, 4)", context);

        Assert.Throws<InvalidOperationException>(() =>
        {
            // Point가 아닌 number 대입 시도
            Evaluate("p = 42", context);
        });

        Assert.Throws<InvalidOperationException>(() =>
        {
            // 객체 멤버에 number가 아닌 string 대입 시도
            Evaluate("p.x = \"hello\"", context);
        });

        // 2-2. 서로 다른 클래스 객체(ObjectValue) 간 구조적 타입 호환 검증
        Evaluate(
            @"class Vector (x: number, y: number) => {
            }",
            context
        );
        Evaluate("var v: Vector = Vector(3, 4)", context);

        // Point 타입 변수 p에 구조가 동일한 Vector 타입 객체 v 대입 시도 (구조적 타이핑 성공)
        Evaluate("p = v", context);
        AssertResult("p.x", 3, context);
        AssertResult("p.y", 4, context);

        // 구조적 서브타이핑 (Point3D -> Point 대입은 성공)
        Evaluate(
            @"class Point3D (x: number, y: number, z: number) => {
            }",
            context
        );
        Evaluate("var p3d: Point3D = Point3D(1, 2, 3)", context);

        // Point 타입 변수 p 에 Point3D 타입 객체 p3d 대입 (성공)
        Evaluate("p = p3d", context);
        AssertResult("p.x", 1, context);
        AssertResult("p.y", 2, context);

        // 반대로 Point3D 변수 p3d 에 Point 타입 객체 v 대입 시도는 실패해야 함 (z 멤버 부족)
        Assert.Throws<InvalidOperationException>(() =>
        {
            Evaluate("p3d = v", context);
        });

        // 3. 함수 매개변수 타입 강제 및 자동 형변환 검증
        Evaluate("var f = (x: string) => x", context);
        // string에 number를 넘겨주면, 자동 형변환이 동작하여 string "123"이 리턴됨
        AssertResult("f(123)", "123", context);

        // 자동 형변환이 불가능한 클래스 인스턴스를 number 매개변수에 전달 시 에러 발생
        Evaluate("var g = (x: number) => x * x", context);
        Assert.Throws<InvalidOperationException>(() =>
        {
            Evaluate("g(p)", context);
        });

        // 4. 예약어 '@type_factory' 사용 방지 검증 (문법 오류 발생해야 함)
        Assert.ThrowsAny<System.Exception>(() =>
        {
            Evaluate("var @type_factory = 10", context);
        });
        Assert.ThrowsAny<System.Exception>(() =>
        {
            Evaluate("class @type_factory() => {}", context);
        });

        // 'type_factory' (골뱅이 없는 것)는 이제 일반 식별자이므로 정의가 가능해야 함
        Evaluate("var type_factory = 100", context);
        AssertResult("type_factory", 100, context);
    }
}
