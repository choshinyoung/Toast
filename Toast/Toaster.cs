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

public class Toaster
{
    public readonly Dictionary<string, Command> PrefixCommands = [];
    public readonly Dictionary<string, Command> InfixCommands = [];
    public readonly Dictionary<string, Command> IdentifierCommands = [];
    public readonly Dictionary<(ToastType Source, ToastType Target), TypeConverter> Converters = [];
    public readonly Context GlobalContext = new();

    public Toaster(bool useBuiltIn = false)
    {
        if (useBuiltIn)
        {
            BuiltIn.Register(this);
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
            IdentifierCommands[command.Name] = command;
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
        var executor = new Executor(this);
        return executor.Execute(rawInput);
    }

    public object? Evaluate(Node node, Context context)
    {
        var executor = new Executor(this);
        return executor.Evaluate(node, context);
    }
}

public class TypeConverter(ToastType source, ToastType target, Func<object?, object?> convertFunc)
{
    public ToastType Source { get; } = source;
    public ToastType Target { get; } = target;
    public Func<object?, object?> ConvertFunc { get; } = convertFunc;
}
