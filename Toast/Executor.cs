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
        if (val is double or float)
            return ToastType.Float;
        if (val is bool)
            return ToastType.Boolean;
        if (val is MemoryAddress)
            return ToastType.Identifier;
        if (val is FunctionValue or Command)
            return ToastType.Function;
        if (val is System.Collections.IEnumerable)
            return ToastType.List;
        return ToastType.Any;
    }

    public object? Execute(string rawInput)
    {
        var tokens = Lexer.Tokenize(rawInput);
        var ast = Parser.Parse(tokens, _toast.GetInfixInfo, _toast.IsPrefix);
        return EvaluateProgram(ast, _toast.GlobalContext);
    }

    public object? Evaluate(Node node, Context context, bool suppressZeroArgFunction = false)
    {
        return node switch
        {
            LiteralNode literal => literal.Value,
            IdentifierNode identifier => EvaluateIdentifier(
                identifier,
                context,
                suppressZeroArgFunction
            ),
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

    private object? EvaluateIdentifier(
        IdentifierNode identifier,
        Context context,
        bool suppressZeroArgFunction
    )
    {
        if (context.LookupAddress(identifier.Name) != null)
        {
            var val = context.GetValue(identifier.Name);
            if (
                !suppressZeroArgFunction
                && val is FunctionValue funcVal
                && funcVal.Parameters.Count == 0
            )
            {
                return ExecuteFunction(funcVal, []);
            }
            return val;
        }

        if (_toast.IdentifierCommands.TryGetValue(identifier.Name, out var cmd))
        {
            if (
                !suppressZeroArgFunction
                && cmd.ParameterTypes != null
                && cmd.ParameterTypes.Count == 0
            )
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
            return cmd;
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
        if (group.Items.Count == 1)
        {
            return Evaluate(group.Items[0], context, suppressZeroArgFunction: false);
        }
        var evaluatedItems = group.Items.Select(item => Evaluate(item, context)).ToList();
        return evaluatedItems;
    }

    private object? EvaluateList(ListNode list, Context context)
    {
        return list.Items.Select(item => Evaluate(item, context)).ToList();
    }

    private object? EvaluateCall(CallNode call, Context context)
    {
        var callArgs = call.Arguments.ToList();
        if (callArgs.Count == 1 && callArgs[0] is GroupNode gn)
        {
            callArgs = [.. gn.Items];
        }

        if (call.Callee is IdentifierNode idNode)
        {
            bool isInfixContext = callArgs.Count == 2;

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
                return ExecuteCommand(cmd, callArgs, context);
            }
        }

        var calleeVal = Evaluate(call.Callee, context, suppressZeroArgFunction: true);
        if (calleeVal is FunctionValue funcVal)
        {
            if (funcVal.Parameters.Count != callArgs.Count)
            {
                throw new InvalidOperationException(
                    $"Arity mismatch: function expects {funcVal.Parameters.Count} arguments, but got {callArgs.Count}."
                );
            }

            var evalArgs = callArgs.Select(arg => Evaluate(arg, context)).ToList();
            return ExecuteFunction(funcVal, evalArgs);
        }

        if (calleeVal is Command cmdVal)
        {
            return ExecuteCommand(cmdVal, callArgs, context);
        }

        throw new InvalidOperationException($"Callee is not a callable function or command.");
    }

    private object? ExecuteCommand(Command cmd, List<Node> callArgs, Context context)
    {
        // 1. 지연 평가 커맨드 (ParameterTypes == null) 실행
        if (cmd.ParameterTypes == null)
        {
            try
            {
                return cmd.TargetDelegate.DynamicInvoke(context, callArgs, _toast);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        // 2. 조기 평가 커맨드 (ParameterTypes != null) 매개변수 타입 검증 및 변환
        if (cmd.ParameterTypes.Count != callArgs.Count)
        {
            throw new InvalidOperationException(
                $"Arity mismatch: command '{cmd.Name}' expects {cmd.ParameterTypes.Count} arguments, but got {callArgs.Count}."
            );
        }

        var evalArgs = callArgs.Select(arg => Evaluate(arg, context)).ToList();
        var actualTypes = evalArgs.Select(GetToastType).ToList();

        var finalArgs = new List<object?>();
        for (int i = 0; i < evalArgs.Count; i++)
        {
            var expected = cmd.ParameterTypes[i];
            var actual = actualTypes[i];

            if (_toast.TryConvert(evalArgs[i], actual, expected, out var converted))
            {
                {
                    finalArgs.Add(converted);
                }
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

    private object? ExecuteFunction(FunctionValue funcVal, List<object?> evalArgs)
    {
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
}
