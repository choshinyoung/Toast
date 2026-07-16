namespace Toast;

public class Toaster
{
    public readonly Dictionary<string, Command> PrefixCommands = [];
    public readonly Dictionary<string, Command> InfixCommands = [];
    public readonly Dictionary<(ToastType Source, ToastType Target), TypeConverter> Converters = [];
    public readonly HashSet<ToastType> CustomTypes = [];
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

    public void RegisterType(ToastType type)
    {
        CustomTypes.Add(type);
    }

    public void RegisterType(
        string name,
        Func<Context, ToastValue[], ToastValue> constructorFunc,
        HashSet<string>? declaredMembers = null
    )
    {
        var toastType = new ToastType(name);
        CustomTypes.Add(toastType);
        var constructorCmd = new Command(
            name,
            constructorFunc,
            parameterTypes: Enumerable.Repeat(ToastType.Any, 1).ToList()
        );
        var typeVal = new TypeValue(toastType, constructorCmd, declaredMembers);
        GlobalContext.SetValueDirect(name, typeVal);
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
            GlobalContext.SetValueDirect(command.Name, new CommandValue(command));
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

    public ToastValue Execute(string rawInput)
    {
        return Executor.Execute(rawInput);
    }

    public ToastValue Evaluate(Node node, Context context)
    {
        return Executor.Evaluate(node, context);
    }

    private static readonly HashSet<string> BuiltInTypeNames =
    [
        "number",
        "string",
        "boolean",
        "list",
        "object",
        "any",
        "null",
        "function",
    ];

    public static bool IsCompatible(ToastType actual, ToastType expected, Context context)
    {
        if (expected == ToastType.Any || expected == actual)
        {
            return true;
        }

        if (expected == ToastType.Object)
        {
            if (actual != ToastType.Null && actual != ToastType.Any)
            {
                if (context.HasVariable(actual.Name) && context.GetValue(actual.Name) is TypeValue)
                {
                    return true;
                }
            }
        }

        if (BuiltInTypeNames.Contains(expected.Name) || BuiltInTypeNames.Contains(actual.Name))
        {
            return false;
        }

        if (
            context.HasVariable(expected.Name)
            && context.GetValue(expected.Name) is TypeValue expectedTv
            && context.HasVariable(actual.Name)
            && context.GetValue(actual.Name) is TypeValue actualTv
        )
        {
            foreach (var member in expectedTv.DeclaredMembers)
            {
                if (!actualTv.DeclaredMembers.Contains(member))
                {
                    return false;
                }
            }

            foreach (var member in expectedTv.DeclaredMembers)
            {
                var expectedMemberType = expectedTv.MemberTypes.TryGetValue(member, out var et)
                    ? et
                    : ToastType.Any;
                var actualMemberType = actualTv.MemberTypes.TryGetValue(member, out var at)
                    ? at
                    : ToastType.Any;

                if (!IsCompatible(actualMemberType, expectedMemberType, context))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public bool TryConvert(
        ToastValue obj,
        ToastType actual,
        ToastType expected,
        Context context,
        out ToastValue result
    )
    {
        if (IsCompatible(actual, expected, context))
        {
            result = obj;
            return true;
        }

        var lookupKey = (actual, expected);
        if (Converters.TryGetValue(lookupKey, out var conv))
        {
            result = conv.ConvertFunc(context, obj);
            return true;
        }

        // Fallback for custom objects: treat them as ToastType.Object
        if (
            actual != ToastType.Object
            && obj is ObjectValue
            && Converters.TryGetValue((ToastType.Object, expected), out var objConv)
        )
        {
            result = objConv.ConvertFunc(context, obj);
            return true;
        }

        result = NullValue.Instance;
        return false;
    }
}
