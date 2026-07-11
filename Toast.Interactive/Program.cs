namespace Toast.Interactive;

public static class Program
{
    public static void Main(string[] args)
    {
        // 1. Toast 인스턴스 생성
        var engine = new Toast();

        // 2. 수동으로 모든 연산자, 함수, 제어 구조 등록
        RegisterBuiltinCommands(engine);

        // 3. 테스트 구동 (v2.0 명세 검증)
        RunTests(engine);

        // 4. REPL 세션 시작
        RunREPL(engine);
    }

    private static void RunTests(Toast engine)
    {
        Console.WriteLine("=== Running Toast DSL v2.0 Verification Tests (Fully Externalized) ===");

        void RunTest(
            string testName,
            string source,
            object? expectedResult,
            Context? context = null
        )
        {
            context ??= new Context();
            try
            {
                var tokens = Lexer.Tokenize(source);
                var ast = Parser.Parse(tokens, engine.GetInfixInfo, engine.IsPrefix);

                var result = engine.Evaluate(ast, context);

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
        RunTest("Precedence 1 (1 + 2 * 3)", "1 + 2 * 3", 7.0);
        RunTest("Precedence 2 (1 * 2 + 3)", "1 * 2 + 3", 5.0);
        RunTest("Precedence 3 (10 - 2 - 3)", "10 - 2 - 3", 5.0);

        // 2. 변수 할당
        var ctx1 = new Context();
        var tokens = Lexer.Tokenize("var x");
        var ast = Parser.Parse(tokens, t => engine.GetInfixInfo(t), t => engine.IsPrefix(t));
        var xAddr = engine.Evaluate(ast, ctx1);
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

        Console.WriteLine("================================================\n");
    }

    private static void RunREPL(Toast engine)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("  Toast DSL Interactive REPL v2.0 (Functions Only)");
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
                var result = engine.Execute(input);
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

    private static void RegisterBuiltinCommands(Toast engine)
    {
        // 0. 리터럴 상수 (true, false)를 무인자 함수로 등록
        engine.RegisterCommand(new IdentifierCommand("true", (context, args, toast) => true));
        engine.RegisterCommand(new IdentifierCommand("false", (context, args, toast) => false));

        // 1. var 변수 생성
        engine.RegisterCommand(
            new IdentifierCommand(
                "var",
                (context, args, toast) =>
                {
                    if (args.Count != 1)
                        throw new InvalidOperationException(
                            "var command requires exactly 1 argument."
                        );
                    if (args[0] is IdentifierNode idNode)
                    {
                        return context.GetOrCreateAddress(idNode.Name);
                    }
                    throw new InvalidOperationException("Argument of 'var' must be an identifier.");
                }
            )
        );

        // 2. 대입 연산자 =
        engine.RegisterCommand(
            new OperatorCommand(
                "=",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException(
                            "= command requires exactly 2 arguments."
                        );
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
            )
        );

        // 3. 역참조 및 곱셈 연산자 *
        engine.RegisterCommand(
            new OperatorCommand(
                "*",
                (context, args, toast) =>
                {
                    if (args.Count == 1) // 단항 전위 (역참조)
                    {
                        var val = toast.Evaluate(args[0], context);
                        if (val is MemoryAddress addr)
                        {
                            return context.GetValueAtAddress(addr);
                        }
                        throw new InvalidOperationException(
                            "Operand of '*' (dereference) must be a MemoryAddress."
                        );
                    }
                    else if (args.Count == 2) // 중위 곱셈
                    {
                        var left = toast.Evaluate(args[0], context);
                        var right = toast.Evaluate(args[1], context);
                        return Convert.ToDouble(left) * Convert.ToDouble(right);
                    }
                    throw new InvalidOperationException("Invalid arity for operator '*'.");
                },
                precedence: 8,
                isPrefix: true
            )
        );

        // 4. 덧셈 +
        engine.RegisterCommand(
            new OperatorCommand(
                "+",
                (context, args, toast) =>
                {
                    if (args.Count == 1) // 단항 +
                    {
                        return Convert.ToDouble(toast.Evaluate(args[0], context));
                    }
                    if (args.Count == 2) // 이항 +
                    {
                        var left = toast.Evaluate(args[0], context);
                        var right = toast.Evaluate(args[1], context);
                        if (left is string || right is string)
                        {
                            return left?.ToString() + right?.ToString();
                        }
                        return Convert.ToDouble(left) + Convert.ToDouble(right);
                    }
                    throw new InvalidOperationException("Invalid arity for operator '+'.");
                },
                precedence: 7,
                isPrefix: true
            )
        );

        // 5. 뺄셈 -
        engine.RegisterCommand(
            new OperatorCommand(
                "-",
                (context, args, toast) =>
                {
                    if (args.Count == 1) // 단항 -
                    {
                        return -Convert.ToDouble(toast.Evaluate(args[0], context));
                    }
                    if (args.Count == 2) // 이항 -
                    {
                        var left = toast.Evaluate(args[0], context);
                        var right = toast.Evaluate(args[1], context);
                        return Convert.ToDouble(left) - Convert.ToDouble(right);
                    }
                    throw new InvalidOperationException("Invalid arity for operator '-'.");
                },
                precedence: 7,
                isPrefix: true
            )
        );

        // 6. 나눗셈 /
        engine.RegisterCommand(
            new OperatorCommand(
                "/",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("Operator '/' requires 2 arguments.");
                    var left = toast.Evaluate(args[0], context);
                    var right = toast.Evaluate(args[1], context);
                    return Convert.ToDouble(left) / Convert.ToDouble(right);
                },
                precedence: 8
            )
        );

        // 7. 나머지 %
        engine.RegisterCommand(
            new OperatorCommand(
                "%",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("Operator '%' requires 2 arguments.");
                    var left = toast.Evaluate(args[0], context);
                    var right = toast.Evaluate(args[1], context);
                    return Convert.ToDouble(left) % Convert.ToDouble(right);
                },
                precedence: 8
            )
        );

        // 8. 논리 부정 !
        engine.RegisterCommand(
            new OperatorCommand(
                "!",
                (context, args, toast) =>
                {
                    if (args.Count != 1)
                        throw new InvalidOperationException("Operator '!' requires 1 argument.");
                    var val = toast.Evaluate(args[0], context);
                    return val is bool b ? !b : (val == null);
                },
                precedence: 9,
                isPrefix: true
            )
        );

        // 9. 비트 반전 ~
        engine.RegisterCommand(
            new OperatorCommand(
                "~",
                (context, args, toast) =>
                {
                    if (args.Count != 1)
                        throw new InvalidOperationException("Operator '~' requires 1 argument.");
                    var val = toast.Evaluate(args[0], context);
                    return ~Convert.ToInt64(val);
                },
                precedence: 9,
                isPrefix: true
            )
        );

        // 10. 논리곱 && (단락 평가)
        engine.RegisterCommand(
            new OperatorCommand(
                "&&",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("Operator '&&' requires 2 arguments.");
                    var left = toast.Evaluate(args[0], context);
                    if (left is bool b1)
                    {
                        if (!b1)
                            return false;
                        var right = toast.Evaluate(args[1], context);
                        return right is bool b2 && b2;
                    }
                    return false;
                },
                precedence: 2
            )
        );

        // 11. 논리합 || (단락 평가)
        engine.RegisterCommand(
            new OperatorCommand(
                "||",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("Operator '||' requires 2 arguments.");
                    var left = toast.Evaluate(args[0], context);
                    if (left is bool b1)
                    {
                        if (b1)
                            return true;
                        var right = toast.Evaluate(args[1], context);
                        return right is bool b2 && b2;
                    }
                    return false;
                },
                precedence: 2
            )
        );

        // 12. 동등 ==
        engine.RegisterCommand(
            new OperatorCommand(
                "==",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("Operator '==' requires 2 arguments.");
                    var left = toast.Evaluate(args[0], context);
                    var right = toast.Evaluate(args[1], context);
                    return Equals(left, right);
                },
                precedence: 4
            )
        );

        // 13. 부등 !=
        engine.RegisterCommand(
            new OperatorCommand(
                "!=",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("Operator '!=' requires 2 arguments.");
                    var left = toast.Evaluate(args[0], context);
                    var right = toast.Evaluate(args[1], context);
                    return !Equals(left, right);
                },
                precedence: 4
            )
        );

        // 14. 비교 <
        engine.RegisterCommand(
            new OperatorCommand(
                "<",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("Operator '<' requires 2 arguments.");
                    var left = toast.Evaluate(args[0], context);
                    var right = toast.Evaluate(args[1], context);
                    return Convert.ToDouble(left) < Convert.ToDouble(right);
                },
                precedence: 5
            )
        );

        // 15. 비교 >
        engine.RegisterCommand(
            new OperatorCommand(
                ">",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("Operator '>' requires 2 arguments.");
                    var left = toast.Evaluate(args[0], context);
                    var right = toast.Evaluate(args[1], context);
                    return Convert.ToDouble(left) > Convert.ToDouble(right);
                },
                precedence: 5
            )
        );

        // 16. 비교 <=
        engine.RegisterCommand(
            new OperatorCommand(
                "<=",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("Operator '<=' requires 2 arguments.");
                    var left = toast.Evaluate(args[0], context);
                    var right = toast.Evaluate(args[1], context);
                    return Convert.ToDouble(left) <= Convert.ToDouble(right);
                },
                precedence: 5
            )
        );

        // 17. 비교 >=
        engine.RegisterCommand(
            new OperatorCommand(
                ">=",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("Operator '>=' requires 2 arguments.");
                    var left = toast.Evaluate(args[0], context);
                    var right = toast.Evaluate(args[1], context);
                    return Convert.ToDouble(left) >= Convert.ToDouble(right);
                },
                precedence: 5
            )
        );

        // 18. 조건문 if
        engine.RegisterCommand(
            new IdentifierCommand(
                "if",
                (context, args, toast) =>
                {
                    if (args.Count < 2)
                        throw new InvalidOperationException(
                            "if command requires at least 2 arguments: condition and block."
                        );
                    var condVal = toast.Evaluate(args[0], context);
                    bool cond = condVal is bool b ? b : (condVal != null);
                    if (cond)
                    {
                        var val = toast.Evaluate(args[1], context);
                        return new IfResult(true, val);
                    }
                    return new IfResult(false, null);
                }
            )
        );

        // 19. 조건문 else (중위 식별자)
        engine.RegisterCommand(
            new IdentifierCommand(
                "else",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException(
                            "else command requires exactly 2 arguments."
                        );
                    var leftVal = toast.Evaluate(args[0], context);
                    if (leftVal is IfResult ifResult)
                    {
                        if (ifResult.Executed)
                        {
                            return ifResult.Value;
                        }
                        return toast.Evaluate(args[1], context);
                    }
                    throw new InvalidOperationException(
                        "Left side of 'else' must be an 'if' expression."
                    );
                },
                isInfix: true
            )
        );

        // 20. 덧셈 후 대입 +=
        engine.RegisterCommand(
            new OperatorCommand(
                "+=",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException(
                            "+= command requires exactly 2 arguments."
                        );
                    var leftVal = toast.Evaluate(args[0], context);
                    if (leftVal is MemoryAddress addr)
                    {
                        var currentVal = context.GetValueAtAddress(addr);
                        var rightVal = toast.Evaluate(args[1], context);
                        object newVal;
                        if (currentVal is string || rightVal is string)
                        {
                            newVal = (currentVal?.ToString() ?? "") + (rightVal?.ToString() ?? "");
                        }
                        else
                        {
                            newVal = Convert.ToDouble(currentVal) + Convert.ToDouble(rightVal);
                        }
                        context.SetValueAtAddress(addr, newVal);
                        return newVal;
                    }
                    throw new InvalidOperationException("L-value of '+=' must be a MemoryAddress.");
                },
                precedence: 1,
                isRightAssociative: true
            )
        );

        // 21. 뺄셈 후 대입 -=
        engine.RegisterCommand(
            new OperatorCommand(
                "-=",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException(
                            "-= command requires exactly 2 arguments."
                        );
                    var leftVal = toast.Evaluate(args[0], context);
                    if (leftVal is MemoryAddress addr)
                    {
                        var currentVal = context.GetValueAtAddress(addr);
                        var rightVal = toast.Evaluate(args[1], context);
                        var newVal = Convert.ToDouble(currentVal) - Convert.ToDouble(rightVal);
                        context.SetValueAtAddress(addr, newVal);
                        return newVal;
                    }
                    throw new InvalidOperationException("L-value of '-=' must be a MemoryAddress.");
                },
                precedence: 1,
                isRightAssociative: true
            )
        );

        // to operator
        engine.RegisterCommand(
            new IdentifierCommand(
                "to",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("'to' requires 2 arguments.");
                    var left = Convert.ToInt32(toast.Evaluate(args[0], context));
                    var right = Convert.ToInt32(toast.Evaluate(args[1], context));
                    return Enumerable.Range(left, right - left + 1).ToList();
                },
                isInfix: true,
                precedence: 6
            )
        );

        // in operator
        engine.RegisterCommand(
            new IdentifierCommand(
                "in",
                (context, args, toast) =>
                {
                    if (args.Count != 2)
                        throw new InvalidOperationException("'in' requires 2 arguments.");
                    var left = toast.Evaluate(args[0], context);
                    var right = toast.Evaluate(args[1], context);
                    if (right is System.Collections.IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            if (Equals(item, left))
                                return true;
                        }
                        return false;
                    }
                    return false;
                },
                isInfix: true,
                precedence: 6
            )
        );
    }
}
