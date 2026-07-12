namespace Toast;

public static class BuiltIn
{
    public static void Register(Toaster toast)
    {
        // Register Converters
        toast.RegisterConverter(
            new TypeConverter(ToastType.Integer, ToastType.Float, (_, val) => Convert.ToDouble(val))
        );
        toast.RegisterConverter(
            new TypeConverter(ToastType.Integer, ToastType.String, (_, val) => val?.ToString())
        );
        toast.RegisterConverter(
            new TypeConverter(ToastType.Float, ToastType.String, (_, val) => val?.ToString())
        );
        toast.RegisterConverter(
            new TypeConverter(ToastType.Boolean, ToastType.String, (_, val) => val?.ToString())
        );
        toast.RegisterConverter(
            new TypeConverter(
                ToastType.List,
                ToastType.String,
                (ctx, val) =>
                {
                    if (val is System.Collections.IEnumerable enumerable)
                    {
                        var list = new List<string>();
                        foreach (var x in enumerable)
                        {
                            var type = Executor.GetToastType(x);
                            if (
                                ctx.Toaster.TryConvert(
                                    x,
                                    type,
                                    ToastType.String,
                                    ctx,
                                    out var converted
                                )
                            )
                            {
                                list.Add(converted?.ToString() ?? "null");
                                continue;
                            }
                            list.Add(x?.ToString() ?? "null");
                        }
                        return $"[{string.Join(", ", list)}]";
                    }
                    return "[]";
                }
            )
        );

        // 0. 리터럴 상수 무인자 함수 등록
        toast.RegisterFunction("true", (Context context) => true);
        toast.RegisterFunction("false", (Context context) => false);

        // 1. var 변수 생성 (지연 평가)
        toast.RegisterFunction(
            "var",
            (Context context, IdentifierNode idNode) =>
            {
                return context.GetOrCreateAddress(idNode.Name);
            }
        );

        // 2. 대입 연산자 = (조기 평가)
        toast.RegisterOperator(
            "=",
            (Context context, MemoryAddress addr, object? rightVal) =>
            {
                context.SetValueAtAddress(addr, rightVal);
                return rightVal;
            },
            precedence: 1
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
        toast.RegisterOperator(
            "!",
            (Context context, bool val) => !val,
            precedence: 9,
            isPrefix: true
        );

        // 12. 전위 비트 반전 ~
        toast.RegisterOperator(
            "~",
            (Context context, int val) => ~val,
            precedence: 9,
            isPrefix: true
        );

        // 13. 중위 논리곱 && (지연 평가)
        toast.RegisterOperator(
            "&&",
            (Context context, bool left, Node right) =>
            {
                if (!left)
                    return false;
                return (bool)context.Toaster.Evaluate(right, context)!;
            },
            precedence: 2
        );

        // 14. 중위 논리합 || (지연 평가)
        toast.RegisterOperator(
            "||",
            (Context context, bool left, Node right) =>
            {
                if (left)
                    return true;
                return (bool)context.Toaster.Evaluate(right, context)!;
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
            (Context context, bool cond, Node body) =>
            {
                if (cond)
                {
                    var val = context.Toaster.Evaluate(body, context);
                    if (val is FunctionValue funcVal && funcVal.Parameters.Count == 0)
                    {
                        return funcVal.Execute([]);
                    }
                    return val;
                }
                return null;
            }
        );

        // 22. 조건문 else (지연 평가)
        toast.RegisterFunction(
            "else",
            (Context context, Node leftNode, Node rightNode) =>
            {
                while (leftNode is GroupNode gn && gn.Items.Count == 1)
                {
                    leftNode = gn.Items[0];
                }

                if (
                    leftNode is CallNode callNode
                    && callNode.Callee is IdentifierNode idNode
                    && idNode.Name == "if"
                    && callNode.Arguments.Count == 2
                )
                {
                    var cond = (bool)context.Toaster.Evaluate(callNode.Arguments[0], context)!;
                    if (cond)
                    {
                        var val = context.Toaster.Evaluate(callNode.Arguments[1], context);
                        if (val is FunctionValue funcVal && funcVal.Parameters.Count == 0)
                        {
                            return funcVal.Execute([]);
                        }
                        return val;
                    }
                    else
                    {
                        var val = context.Toaster.Evaluate(rightNode, context);
                        if (val is FunctionValue funcVal && funcVal.Parameters.Count == 0)
                        {
                            return funcVal.Execute([]);
                        }
                        return val;
                    }
                }

                throw new InvalidOperationException(
                    "Left side of 'else' must be an 'if' expression."
                );
            },
            precedence: 6,
            isRightAssociative: true, // Make else right-associative!
            isInfix: true
        );

        // 23. 덧셈 후 대입 += (조기 평가)
        toast.RegisterOperator(
            "+=",
            (Context context, MemoryAddress addr, object? rightVal) =>
            {
                var currentVal = context.GetValueAtAddress(addr);
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
                context.SetValueAtAddress(addr, newVal);
                return newVal;
            },
            precedence: 1
        );

        // 24. 뺄셈 후 대입 -= (조기 평가)
        toast.RegisterOperator(
            "-=",
            (Context context, MemoryAddress addr, object? rightVal) =>
            {
                var currentVal = context.GetValueAtAddress(addr);
                object newVal;
                if (currentVal is double || rightVal is double)
                {
                    newVal = Convert.ToDouble(currentVal) - Convert.ToDouble(rightVal);
                }
                else
                {
                    newVal = Convert.ToInt32(currentVal) - Convert.ToInt32(rightVal);
                }
                context.SetValueAtAddress(addr, newVal);
                return newVal;
            },
            precedence: 1
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
            (Context context, object? leftVal, Node targetNode) =>
            {
                var sourceType = Executor.GetToastType(leftVal);

                ToastType targetType;
                if (targetNode is TypeNode typeNode)
                {
                    targetType = typeNode.Type;
                }
                else if (targetNode is IdentifierNode idNode)
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
                {
                    return leftVal;
                }

                if (
                    context.Toaster.TryConvert(
                        leftVal,
                        sourceType,
                        targetType,
                        context,
                        out var converted
                    )
                )
                {
                    return converted;
                }

                throw new InvalidOperationException(
                    $"No converter registered from {sourceType} to {targetType}."
                );
            },
            precedence: 6,
            isInfix: true
        );

        // 27. 인용 ` (지연 평가 전위 연산자)
        toast.RegisterOperator(
            "`",
            (Context context, Node node) =>
            {
                if (node is IdentifierNode idNode)
                {
                    if (context.Toaster.InfixCommands.TryGetValue(idNode.Name, out var infixCmd))
                        return infixCmd;
                    if (context.Toaster.PrefixCommands.TryGetValue(idNode.Name, out var prefixCmd))
                        return prefixCmd;
                }
                else if (
                    node is GroupNode gn
                    && gn.Items.Count == 1
                    && gn.Items[0] is IdentifierNode innerId
                )
                {
                    if (context.Toaster.InfixCommands.TryGetValue(innerId.Name, out var infixCmd))
                        return infixCmd;
                    if (context.Toaster.PrefixCommands.TryGetValue(innerId.Name, out var prefixCmd))
                        return prefixCmd;
                }

                var executor = new Executor(context.Toaster);
                return executor.Evaluate(node, context, suppressZeroArgFunction: true);
            },
            precedence: 9,
            isPrefix: true
        );
    }
}
