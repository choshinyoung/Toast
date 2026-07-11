using Toast;

var toast = new Toaster();

toast.RegisterConverter(
    new TypeConverter(ToastType.Integer, ToastType.Float, val => Convert.ToDouble(val))
);
toast.RegisterConverter(
    new TypeConverter(ToastType.Integer, ToastType.String, val => val?.ToString())
);
toast.RegisterConverter(
    new TypeConverter(ToastType.Float, ToastType.String, val => val?.ToString())
);
toast.RegisterConverter(
    new TypeConverter(ToastType.Boolean, ToastType.String, val => val?.ToString())
);

RegisterBuiltinCommands(toast);
RunTests(toast);
RunREPL(toast);

static void RunTests(Toaster toast)
{
    Console.WriteLine("=== Running Toast DSL v2.0 Verification Tests (Natural Lambda Types) ===");

    void RunTest(string testName, string source, object? expectedResult, Context? context = null)
    {
        context ??= new Context();
        try
        {
            var tokens = Lexer.Tokenize(source);
            var ast = Parser.Parse(tokens, toast.GetInfixInfo, toast.IsPrefix);

            var result = toast.Evaluate(ast, context);

            bool isSuccess = false;
            if (expectedResult is string s && s == "MemoryAddress")
            {
                isSuccess = result is MemoryAddress;
            }
            else if (
                expectedResult is System.Collections.IEnumerable enumerable
                && expectedResult is not string
            )
            {
                if (result is System.Collections.IEnumerable resEnumerable)
                {
                    var expList = enumerable.Cast<object>().ToList();
                    var resList = resEnumerable.Cast<object>().ToList();
                    isSuccess = expList.SequenceEqual(resList);
                }
            }
            else
            {
                isSuccess = Equals(result, expectedResult);
            }

            if (isSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[PASS] {testName}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FAIL] {testName}");
                Console.WriteLine($"  Source:   {source}");
                Console.WriteLine(
                    $"  Expected: {expectedResult} (Type: {expectedResult?.GetType().Name})"
                );
                Console.WriteLine($"  Got:      {result} (Type: {result?.GetType().Name})");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {testName} threw exception: {ex.Message}");
        }
        Console.ResetColor();
    }

    // 1. 연산자 우선순위 테스트
    RunTest("Precedence 1 (1 + 2 * 3)", "1 + 2 * 3", 7);
    RunTest("Precedence 2 (1 * 2 + 3)", "1 * 2 + 3", 5);
    RunTest("Precedence 3 (10 - 2 - 3)", "10 - 2 - 3", 5.0);

    // 2. 변수 할당
    var ctx1 = new Context();
    var tokens = Lexer.Tokenize("var x");
    var ast = Parser.Parse(tokens, t => toast.GetInfixInfo(t), t => toast.IsPrefix(t));
    var xAddr = toast.Evaluate(ast, ctx1);
    if (xAddr is MemoryAddress)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[PASS] var x returns MemoryAddress");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FAIL] var x did not return MemoryAddress. Got: {xAddr}");
    }
    Console.ResetColor();

    var ctxAss = new Context();
    RunTest("Assignment (var x = 42)", "var x = 42", 42, ctxAss);
    RunTest("Retrieval (x)", "x", 42, ctxAss);

    // 3. 포인터 및 역참조
    var ctx2 = new Context();
    RunTest("Pointer Setup (a = var c)", "var c = 10\n var a = (var c)", "MemoryAddress", ctx2);
    RunTest("Pointer Assignment (a = 20)", "a = 20", 20, ctx2);
    RunTest("Dereference (*a)", "*a", 20, ctx2);
    RunTest("Original Value c after pointer edit", "c", 20, ctx2);

    // 4. 조건문
    RunTest("If-else (True branch)", "if (true) { 100 } else { 200 }", 100);
    RunTest("If-else (False branch)", "if (false) { 100 } else { 200 }", 200);

    var ctx3 = new Context();
    RunTest(
        "If-else with Variable condition",
        "var cond = true\n if (cond) { 10 } else { 20 }",
        10,
        ctx3
    );

    // 5. 중위 식별자
    RunTest("to operator (1 to 5)", "1 to 5", new List<int> { 1, 2, 3, 4, 5 });
    var ctx4 = new Context();
    RunTest("in operator (3 in 1 to 5)", "var r = 1 to 5\n 3 in r", true, ctx4);
    RunTest("in operator (6 in 1 to 5)", "6 in (1 to 5)", false);

    // 6. 명시적 캐스팅 as 테스트
    RunTest("Explicit Cast (1 as float)", "1 as float", 1.0);
    RunTest("Explicit Cast (true as string)", "true as string", "True");

    Console.WriteLine("================================================\n");
}

static void RunREPL(Toaster toast)
{
    Console.WriteLine("========================================");
    Console.WriteLine("  Toast DSL Interactive REPL v2.0 (OOP & Natural Lambdas)");
    Console.WriteLine("========================================");
    Console.WriteLine("Type exit or quit to end the session.\n");

    while (true)
    {
        Console.Write("toast> ");
        var input = Console.ReadLine();
        if (input == null)
            break;

        var trimmed = input.Trim();
        if (trimmed is "exit" or "quit")
            break;
        if (string.IsNullOrWhiteSpace(trimmed))
            continue;

        try
        {
            var result = toast.Execute(input);
            if (result != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(result);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("(null)");
            }
            Console.ResetColor();
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}

static void RegisterBuiltinCommands(Toaster toast)
{
    // 0. 리터럴 상수 무인자 함수 등록
    toast.RegisterFunction("true", (Context context) => true);
    toast.RegisterFunction("false", (Context context) => false);

    // 1. var 변수 생성 (지연 평가)
    toast.RegisterFunction(
        "var",
        object? (Context context, List<Node> args, Toaster toast) =>
        {
            var idNode = (IdentifierNode)args[0];
            return context.GetOrCreateAddress(idNode.Name);
        }
    );

    // 2. 대입 연산자 = (지연 평가)
    toast.RegisterOperator(
        "=",
        object? (Context context, List<Node> args, Toaster toast) =>
        {
            var leftVal = toast.Evaluate(args[0], context);
            if (leftVal is MemoryAddress addr)
            {
                var rightVal = toast.Evaluate(args[1], context);
                context.SetValueAtAddress(addr, rightVal);
                return rightVal;
            }
            throw new InvalidOperationException("L-value of '=' must be a MemoryAddress.");
        },
        precedence: 1,
        isRightAssociative: true
    );

    // 3. 전위 역참조 *
    toast.RegisterOperator(
        "*",
        object? (Context context, MemoryAddress addr) =>
        {
            return context.GetValueAtAddress(addr);
        },
        precedence: 9,
        isPrefix: true
    );

    // 4. 중위 곱셈 *
    toast.RegisterOperator(
        "*",
        object? (Context context, object? left, object? right) =>
        {
            if (left is double || right is double || left is float || right is float)
            {
                return Convert.ToDouble(left) * Convert.ToDouble(right);
            }
            return Convert.ToInt32(left) * Convert.ToInt32(right);
        },
        precedence: 8
    );

    // 5. 전위 단항 +
    toast.RegisterOperator(
        "+",
        (Context context, double val) => val,
        precedence: 9,
        isPrefix: true
    );

    // 6. 중위 덧셈 +
    toast.RegisterOperator(
        "+",
        object? (Context context, object? left, object? right) =>
        {
            if (left is string || right is string)
            {
                return left?.ToString() + right?.ToString();
            }
            if (left is double || right is double || left is float || right is float)
            {
                return Convert.ToDouble(left) + Convert.ToDouble(right);
            }
            return Convert.ToInt32(left) + Convert.ToInt32(right);
        },
        precedence: 7
    );

    // 7. 전위 단항 -
    toast.RegisterOperator(
        "-",
        (Context context, double val) => -val,
        precedence: 9,
        isPrefix: true
    );

    // 8. 중위 뺄셈 -
    toast.RegisterOperator(
        "-",
        (Context context, double left, double right) => left - right,
        precedence: 7
    );

    // 9. 중위 나눗셈 /
    toast.RegisterOperator(
        "/",
        (Context context, double left, double right) => left / right,
        precedence: 8
    );

    // 10. 중위 나머지 %
    toast.RegisterOperator(
        "%",
        (Context context, double left, double right) => left % right,
        precedence: 8
    );

    // 11. 전위 부정 !
    toast.RegisterOperator("!", (Context context, bool val) => !val, precedence: 9, isPrefix: true);

    // 12. 전위 비트 반전 ~
    toast.RegisterOperator("~", (Context context, int val) => ~val, precedence: 9, isPrefix: true);

    // 13. 중위 논리곱 && (지연 평가)
    toast.RegisterOperator(
        "&&",
        object? (Context context, List<Node> args, Toaster toast) =>
        {
            var left = (bool)toast.Evaluate(args[0], context)!;
            if (!left)
                return false;
            return (bool)toast.Evaluate(args[1], context)!;
        },
        precedence: 2
    );

    // 14. 중위 논리합 || (지연 평가)
    toast.RegisterOperator(
        "||",
        object? (Context context, List<Node> args, Toaster toast) =>
        {
            var left = (bool)toast.Evaluate(args[0], context)!;
            if (left)
                return true;
            return (bool)toast.Evaluate(args[1], context)!;
        },
        precedence: 2
    );

    // 15. 중위 동등 ==
    toast.RegisterOperator(
        "==",
        (Context context, object? left, object? right) => Equals(left, right),
        precedence: 4
    );

    // 16. 중위 부등 !=
    toast.RegisterOperator(
        "!=",
        (Context context, object? left, object? right) => !Equals(left, right),
        precedence: 4
    );

    // 17. 중위 비교 <
    toast.RegisterOperator(
        "<",
        (Context context, double left, double right) => left < right,
        precedence: 5
    );

    // 18. 중위 비교 >
    toast.RegisterOperator(
        ">",
        (Context context, double left, double right) => left > right,
        precedence: 5
    );

    // 19. 중위 비교 <=
    toast.RegisterOperator(
        "<=",
        (Context context, double left, double right) => left <= right,
        precedence: 5
    );

    // 20. 중위 비교 >=
    toast.RegisterOperator(
        ">=",
        (Context context, double left, double right) => left >= right,
        precedence: 5
    );

    // 21. 조건문 if (지연 평가)
    toast.RegisterFunction(
        "if",
        object? (Context context, List<Node> args, Toaster toast) =>
        {
            var cond = (bool)toast.Evaluate(args[0], context)!;
            if (cond)
            {
                var val = toast.Evaluate(args[1], context);
                return new IfResult(true, val);
            }
            return new IfResult(false, null);
        }
    );

    // 22. 조건문 else (지연 평가)
    toast.RegisterFunction(
        "else",
        object? (Context context, List<Node> args, Toaster toast) =>
        {
            var leftVal = toast.Evaluate(args[0], context);
            if (leftVal is IfResult ifResult)
            {
                if (ifResult.Executed)
                {
                    return ifResult.Value;
                }
                return toast.Evaluate(args[1], context);
            }
            throw new InvalidOperationException("Left side of 'else' must be an 'if' expression.");
        },
        precedence: 6,
        isInfix: true
    );

    // 23. 덧셈 후 대입 += (지연 평가)
    toast.RegisterOperator(
        "+=",
        object? (Context context, List<Node> args, Toaster toast) =>
        {
            var addr = toast.Evaluate(args[0], context) as MemoryAddress;
            var currentVal = context.GetValueAtAddress(addr!);
            var rightVal = toast.Evaluate(args[1], context);
            object newVal;
            if (currentVal is string || rightVal is string)
            {
                newVal = (currentVal?.ToString() ?? "") + (rightVal?.ToString() ?? "");
            }
            else if (currentVal is double || rightVal is double)
            {
                newVal = Convert.ToDouble(currentVal) + Convert.ToDouble(rightVal);
            }
            else
            {
                newVal = Convert.ToInt32(currentVal) + Convert.ToInt32(rightVal);
            }
            context.SetValueAtAddress(addr!, newVal);
            return newVal;
        },
        precedence: 1,
        isRightAssociative: true
    );

    // 24. 뺄셈 후 대입 -= (지연 평가)
    toast.RegisterOperator(
        "-=",
        object? (Context context, List<Node> args, Toaster toast) =>
        {
            var addr = toast.Evaluate(args[0], context) as MemoryAddress;
            var currentVal = context.GetValueAtAddress(addr!);
            var rightVal = toast.Evaluate(args[1], context);
            object newVal;
            if (currentVal is double || rightVal is double)
            {
                newVal = Convert.ToDouble(currentVal) - Convert.ToDouble(rightVal);
            }
            else
            {
                newVal = Convert.ToInt32(currentVal) - Convert.ToInt32(rightVal);
            }
            context.SetValueAtAddress(addr!, newVal);
            return newVal;
        },
        precedence: 1,
        isRightAssociative: true
    );

    // 25. 멤버 접근 .
    toast.RegisterOperator(
        ".",
        (Context context, object? left, object? right) =>
        {
            return $"{left}.{right}";
        },
        precedence: 10
    );

    // 26. 중위 식별자 to
    toast.RegisterFunction(
        "to",
        (Context context, int left, int right) =>
        {
            return Enumerable.Range(left, right - left + 1).ToList();
        },
        precedence: 6,
        isInfix: true
    );

    // 27. 중위 식별자 in
    toast.RegisterFunction(
        "in",
        (Context context, object? left, System.Collections.IEnumerable right) =>
        {
            foreach (var item in right)
            {
                if (Equals(item, left))
                    return true;
            }
            return false;
        },
        precedence: 6,
        isInfix: true
    );

    // 28. 중위 식별자 is
    toast.RegisterFunction(
        "is",
        (Context context, object? left, object? right) =>
        {
            var typeStr = right is IdentifierNode typeId ? typeId.Name : right?.ToString();
            return left?.GetType().Name.ToLower() == typeStr?.ToLower();
        },
        precedence: 6,
        isInfix: true
    );

    // 29. 명시적 형변환 as 연산자 등록
    toast.RegisterFunction(
        "as",
        object? (Context context, List<Node> args, Toaster toast) =>
        {
            var leftVal = toast.Evaluate(args[0], context);
            var sourceType = Executor.GetToastType(leftVal);

            ToastType targetType;
            if (args[1] is TypeNode typeNode)
            {
                targetType = typeNode.Type;
            }
            else if (args[1] is IdentifierNode idNode)
            {
                targetType = idNode.Name.ToLower() switch
                {
                    "string" => ToastType.String,
                    "integer" => ToastType.Integer,
                    "float" => ToastType.Float,
                    "boolean" => ToastType.Boolean,
                    _ => throw new InvalidOperationException(
                        $"Invalid cast target type: {idNode.Name}"
                    ),
                };
            }
            else
            {
                throw new InvalidOperationException("Right side of 'as' must be a type.");
            }

            if (sourceType == targetType)
                return leftVal;

            if (toast.Converters.TryGetValue((sourceType, targetType), out var converter))
            {
                return converter.ConvertFunc(leftVal);
            }

            throw new InvalidOperationException(
                $"No converter registered from {sourceType} to {targetType}."
            );
        },
        precedence: 6,
        isInfix: true
    );
}
