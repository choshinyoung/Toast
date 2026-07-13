namespace Toast;

public class FunctionValue(
    IReadOnlyList<ParameterNode> parameters,
    IReadOnlyList<Node> statements,
    Context closureContext,
    Toaster toast
)
{
    public IReadOnlyList<ParameterNode> Parameters { get; } = parameters;
    public IReadOnlyList<Node> Statements { get; } = statements;
    public Context ClosureContext { get; } = closureContext;
    public Toaster Toast { get; } = toast;

    public object? Execute(List<object?> evalArgs)
    {
        var runContext = new Context(ClosureContext);
        for (int i = 0; i < Parameters.Count; i++)
        {
            var param = Parameters[i];
            var addr = runContext.GetOrCreateAddress(param.Name);
            addr.SetValue(evalArgs[i]);
        }

        object? lastVal = null;
        foreach (var stmt in Statements)
        {
            lastVal = Toast.Executor.Evaluate(stmt, runContext);
        }
        return lastVal;
    }
}

public class Toaster
{
    public readonly Dictionary<string, Command> PrefixCommands = [];
    public readonly Dictionary<string, Command> InfixCommands = [];
    public readonly Dictionary<(ToastType Source, ToastType Target), TypeConverter> Converters = [];
    public readonly Context GlobalContext;
    public readonly Executor Executor;

    public Toaster(bool useBuiltIn = false)
    {
        Executor = new Executor(this);
        GlobalContext = new Context(this);
        if (useBuiltIn)
        {
            BuiltIns.BuiltIn.Register(this);
        }
    }

    public void RegisterCommand(Command command)
    {
        if (command.IsPrefix)
        {
            PrefixCommands[command.Name] = command;
        }
        else if (command.Precedence > 0 || command.IsInfix)
        {
            InfixCommands[command.Name] = command;
        }
        else
        {
            GlobalContext.SetValueDirect(command.Name, command);
        }
    }

    public void RegisterOperator(
        string name,
        Delegate targetDelegate,
        int precedence,
        bool isRightAssociative = false,
        bool isPrefix = false
    )
    {
        RegisterCommand(
            Command.CreateOperator(name, targetDelegate, precedence, isRightAssociative, isPrefix)
        );
    }

    public void RegisterFunction(
        string name,
        Delegate targetDelegate,
        int precedence = 0,
        bool isRightAssociative = false,
        bool isPrefix = false,
        bool isInfix = false
    )
    {
        RegisterCommand(
            Command.CreateFunction(
                name,
                targetDelegate,
                precedence,
                isRightAssociative,
                isPrefix,
                isInfix
            )
        );
    }

    public void RegisterConverter(TypeConverter converter)
    {
        Converters[(converter.Source, converter.Target)] = converter;
    }

    public HashSet<string> GetInfixIdentifiers()
    {
        var infixIds = new HashSet<string>();
        foreach (var cmd in InfixCommands.Values)
        {
            if (cmd.IsInfix)
            {
                infixIds.Add(cmd.Name);
            }
        }
        return infixIds;
    }

    public (int Precedence, bool IsRight) GetInfixInfo(Token token)
    {
        if (token.Value != null && InfixCommands.TryGetValue(token.Value, out var cmd))
        {
            if (cmd.Precedence > 0)
            {
                return (cmd.Precedence, cmd.IsRightAssociative);
            }
            if (cmd.IsInfix)
            {
                return (6, false);
            }
        }
        if (token.Kind == TokenKind.Identifier && token.Value != null)
        {
            var name = token.Value;
            if (name.StartsWith('~'))
            {
                return (6, false);
            }
        }
        return (0, false);
    }

    public bool IsPrefix(Token token)
    {
        return token.Value != null && PrefixCommands.ContainsKey(token.Value);
    }

    public object? Execute(string rawInput)
    {
        return Executor.Execute(rawInput);
    }

    public object? Evaluate(Node node, Context context)
    {
        return Executor.Evaluate(node, context);
    }

    public bool TryConvert(
        object? obj,
        ToastType actual,
        ToastType expected,
        Context context,
        out object? result
    )
    {
        if (expected == ToastType.Any || expected == actual)
        {
            result = obj;

            return true;
        }
        else if (Converters.TryGetValue((actual, expected), out var conv))
        {
            result = conv.ConvertFunc(context, obj);
            return true;
        }

        result = null;
        return false;
    }
}
