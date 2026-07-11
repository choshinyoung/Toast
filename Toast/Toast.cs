namespace Toast;

public record IfResult(bool Executed, object? Value);

public class FunctionValue(
    IReadOnlyList<ParameterNode> parameters,
    Node body,
    Context closureContext
)
{
    public IReadOnlyList<ParameterNode> Parameters { get; } = parameters;
    public Node Body { get; } = body;
    public Context ClosureContext { get; } = closureContext;
}

public class Toast
{
    private readonly Dictionary<string, Command> _commands = [];
    private readonly Context _globalContext = new();

    public void RegisterCommand(Command command)
    {
        _commands[command.Name] = command;
    }

    public HashSet<string> GetInfixIdentifiers()
    {
        var infixIds = new HashSet<string>();
        foreach (var cmd in _commands.Values)
        {
            if (cmd is IdentifierCommand idCmd && idCmd.IsInfix)
            {
                infixIds.Add(cmd.Name);
            }
        }
        return infixIds;
    }

    public (int Precedence, bool IsRight) GetInfixInfo(Token token)
    {
        if (token.Value != null && _commands.TryGetValue(token.Value, out var cmd))
        {
            if (cmd.Precedence > 0)
            {
                return (cmd.Precedence, cmd.IsRightAssociative);
            }
            if (cmd is IdentifierCommand idCmd && idCmd.IsInfix)
            {
                return (6, false);
            }
        }
        if (token.Kind == TokenKind.Identifier && token.Value != null)
        {
            var name = token.Value;
            if (name is "else" or "to" or "in" or "is" || name.StartsWith('~'))
            {
                return (6, false);
            }
        }
        return (0, false);
    }

    public bool IsPrefix(Token token)
    {
        if (token.Value != null && _commands.TryGetValue(token.Value, out var cmd))
        {
            return cmd.IsPrefix;
        }
        return false;
    }

    public object? Execute(string rawInput)
    {
        var tokens = Lexer.Tokenize(rawInput);
        var ast = Parser.Parse(tokens, GetInfixInfo, IsPrefix);
        return EvaluateProgram(ast, _globalContext);
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
        // 1. Context에 바인딩이 있으면 값 반환
        if (context.LookupAddress(identifier.Name) != null)
        {
            return context.GetValue(identifier.Name);
        }

        // 2. 바인딩이 없는데 등록된 커맨드가 있으면 인수 없이 즉시 평가 (예: true, false 등 상수 역할 지원)
        if (_commands.TryGetValue(identifier.Name, out var cmd))
        {
            return cmd.Body(context, [], this);
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
            if (_commands.TryGetValue(idNode.Name, out var cmd))
            {
                return cmd.Body(context, [.. call.Arguments], this);
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
