namespace Toast;

public class Executor(Toaster _toast)
{
    public static ToastType GetToastType(object? val)
    {
        if (val == null)
            return ToastType.Null;
        if (val is string)
            return ToastType.String;
        if (val is int)
            return ToastType.Integer;
        if (val is double || val is float)
            return ToastType.Float;
        if (val is bool)
            return ToastType.Boolean;
        if (val is MemoryAddress)
            return ToastType.Identifier;
        if (val is FunctionValue)
            return ToastType.Function;
        if (val is System.Collections.IEnumerable)
            return ToastType.Array;
        return ToastType.Any;
    }

    public object? Execute(string rawInput)
    {
        var tokens = Lexer.Tokenize(rawInput);
        var ast = Parser.Parse(tokens, _toast.GetInfixInfo, _toast.IsPrefix);
        return EvaluateProgram(ast, _toast.GlobalContext);
    }

    public object? Evaluate(Node node, Context context)
    {
        return node switch
        {
            LiteralNode literal => literal.Value,
            IdentifierNode identifier => EvaluateIdentifier(identifier, context),
            ProgramNode program => EvaluateProgram(program, context),
            BlockNode block => EvaluateBlock(block, context),
            GroupNode group => EvaluateGroup(group, context),
            ListNode list => EvaluateList(list, context),
            FunctionNode function => new FunctionValue(function.Parameters, function.Body, context),
            CallNode call => EvaluateCall(call, context),
            _ => throw new NotSupportedException(
                $"Node type '{node.GetType().Name}' is not supported."
            ),
        };
    }

    private object? EvaluateIdentifier(IdentifierNode identifier, Context context)
    {
        if (context.LookupAddress(identifier.Name) != null)
        {
            return context.GetValue(identifier.Name);
        }

        if (_toast.IdentifierCommands.TryGetValue(identifier.Name, out var cmd))
        {
            if (cmd.ParameterTypes != null && cmd.ParameterTypes.Count == 0)
            {
                try
                {
                    return cmd.TargetDelegate.DynamicInvoke(context);
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    throw ex.InnerException ?? ex;
                }
            }
        }

        throw new InvalidOperationException(
            $"Variable or command '{identifier.Name}' is not defined."
        );
    }

    private object? EvaluateProgram(ProgramNode program, Context context)
    {
        object? lastVal = null;
        foreach (var stmt in program.Statements)
        {
            lastVal = Evaluate(stmt, context);
        }
        if (lastVal is IfResult ifRes)
        {
            lastVal = ifRes.Value;
        }
        return lastVal;
    }

    private object? EvaluateBlock(BlockNode block, Context context)
    {
        var blockContext = new Context(context);
        object? lastVal = null;
        foreach (var stmt in block.Statements)
        {
            lastVal = Evaluate(stmt, blockContext);
        }
        if (lastVal is IfResult ifRes)
        {
            lastVal = ifRes.Value;
        }
        return lastVal;
    }

    private object? EvaluateGroup(GroupNode group, Context context)
    {
        var evaluatedItems = group.Items.Select(item => Evaluate(item, context)).ToList();
        return evaluatedItems.Count == 1 ? evaluatedItems[0] : evaluatedItems;
    }

    private object? EvaluateList(ListNode list, Context context)
    {
        return list.Items.Select(item => Evaluate(item, context)).ToList();
    }

    private object? EvaluateCall(CallNode call, Context context)
    {
        if (call.Callee is IdentifierNode idNode)
        {
            bool isInfixContext = call.Arguments.Count == 2;

            Command? cmd = null;
            if (isInfixContext && _toast.InfixCommands.TryGetValue(idNode.Name, out var infixCmd))
            {
                cmd = infixCmd;
            }
            else if (_toast.PrefixCommands.TryGetValue(idNode.Name, out var prefixCmd))
            {
                cmd = prefixCmd;
            }
            else if (_toast.IdentifierCommands.TryGetValue(idNode.Name, out var idCmd))
            {
                cmd = idCmd;
            }

            if (cmd != null)
            {
                // 1. 지연 평가 커맨드 (ParameterTypes == null) 실행
                if (cmd.ParameterTypes == null)
                {
                    try
                    {
                        return cmd.TargetDelegate.DynamicInvoke(
                            context,
                            call.Arguments.ToList(),
                            _toast
                        );
                    }
                    catch (System.Reflection.TargetInvocationException ex)
                    {
                        throw ex.InnerException ?? ex;
                    }
                }

                // 2. 조기 평가 커맨드 (ParameterTypes != null) 매개변수 타입 검증 및 변환
                if (cmd.ParameterTypes.Count != call.Arguments.Count)
                {
                    throw new InvalidOperationException(
                        $"Arity mismatch: command '{cmd.Name}' expects {cmd.ParameterTypes.Count} arguments, but got {call.Arguments.Count}."
                    );
                }

                var evalArgs = call.Arguments.Select(arg => Evaluate(arg, context)).ToList();
                var actualTypes = evalArgs.Select(GetToastType).ToList();

                var finalArgs = new List<object?>();
                for (int i = 0; i < evalArgs.Count; i++)
                {
                    var expected = cmd.ParameterTypes[i];
                    var actual = actualTypes[i];
                    if (expected == ToastType.Any || expected == actual)
                    {
                        finalArgs.Add(evalArgs[i]);
                    }
                    else if (_toast.Converters.TryGetValue((actual, expected), out var conv))
                    {
                        finalArgs.Add(conv.ConvertFunc(evalArgs[i]));
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Type mismatch: parameter {i} of '{cmd.Name}' expects {expected}, but got {actual}."
                        );
                    }
                }

                var invokeArgs = new object?[finalArgs.Count + 1];
                invokeArgs[0] = context;
                for (int i = 0; i < finalArgs.Count; i++)
                {
                    invokeArgs[i + 1] = finalArgs[i];
                }

                try
                {
                    return cmd.TargetDelegate.DynamicInvoke(invokeArgs);
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    throw ex.InnerException ?? ex;
                }
            }
        }

        var calleeVal = Evaluate(call.Callee, context);
        if (calleeVal is FunctionValue funcVal)
        {
            if (funcVal.Parameters.Count != call.Arguments.Count)
            {
                throw new InvalidOperationException(
                    $"Arity mismatch: function expects {funcVal.Parameters.Count} arguments, but got {call.Arguments.Count}."
                );
            }

            var evalArgs = call.Arguments.Select(arg => Evaluate(arg, context)).ToList();
            var runContext = new Context(funcVal.ClosureContext);
            for (int i = 0; i < funcVal.Parameters.Count; i++)
            {
                var param = funcVal.Parameters[i];
                var addr = runContext.GetOrCreateAddress(param.Name);
                runContext.SetValueAtAddress(addr, evalArgs[i]);
            }

            var res = Evaluate(funcVal.Body, runContext);
            if (res is IfResult ifRes)
            {
                res = ifRes.Value;
            }
            return res;
        }

        throw new InvalidOperationException($"Callee is not a callable function or command.");
    }
}
