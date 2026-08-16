namespace Toast;

public class Toaster
{
    private static readonly IReadOnlyList<IToastModule> DefaultModules =
    [
        new SystemModules.ImportModule(),
    ];

    public static readonly Toaster Empty = new([]);

    public readonly Dictionary<string, Command> PrefixCommands = [];
    public readonly Dictionary<string, Command> InfixCommands = [];
    public readonly Dictionary<(ToastType Source, ToastType Target), TypeConverter> Converters = [];
    public readonly HashSet<ToastType> CustomTypes = [];
    public readonly Dictionary<ToastType, Dictionary<string, ToastValue>> ExtensionMembers = [];
    public readonly Context GlobalContext;
    public readonly Executor Executor;

    public Toaster(IEnumerable<IToastModule>? modules = null)
    {
        Executor = new Executor(this);
        GlobalContext = new Context(this);
        RegisterBuiltInTypes();

        var targetModules = modules ?? DefaultModules;
        foreach (var module in targetModules)
        {
            Load(module);
        }
    }

    private void RegisterBuiltInTypes()
    {
        GlobalContext.SetValueDirect(ToastType.Number.Name, new TypeValue(ToastType.Number, null));
        GlobalContext.SetValueDirect(ToastType.String.Name, new TypeValue(ToastType.String, null));
        GlobalContext.SetValueDirect(
            ToastType.Boolean.Name,
            new TypeValue(ToastType.Boolean, null)
        );
        GlobalContext.SetValueDirect(ToastType.List.Name, new TypeValue(ToastType.List, null));
        GlobalContext.SetValueDirect(ToastType.Object.Name, new TypeValue(ToastType.Object, null));
        GlobalContext.SetValueDirect(
            ToastType.Function.Name,
            new TypeValue(ToastType.Function, null)
        );
        GlobalContext.SetValueDirect(ToastType.Null.Name, new TypeValue(ToastType.Null, null));
        GlobalContext.SetValueDirect(ToastType.Error.Name, new TypeValue(ToastType.Error, null));
        GlobalContext.SetValueDirect("Error", new TypeValue(ToastType.Error, null));
    }

    public void Load(string moduleName)
    {
        ModuleManager.Instance.LoadModule(moduleName, this, GlobalContext);
    }

    public void Load<T>()
        where T : IToastModule, new()
    {
        var module = new T();
        Load(module);
    }

    public void Load(Type moduleType)
    {
        if (
            !typeof(IToastModule).IsAssignableFrom(moduleType)
            || moduleType.IsAbstract
            || moduleType.IsInterface
        )
        {
            throw new ArgumentException(
                $"Type '{moduleType.FullName}' must implement IToastModule and be a non-abstract class."
            );
        }

        if (Activator.CreateInstance(moduleType) is IToastModule module)
        {
            Load(module);
        }
    }

    public void Load(IToastModule module)
    {
        ModuleLoader.Load(module, this, GlobalContext);
    }

    public void RegisterType(ToastType type)
    {
        CustomTypes.Add(type);
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

    public void RegisterConverter(TypeConverter converter)
    {
        Converters[(converter.Source, converter.Target)] = converter;
    }

    public void RegisterTypeMember(ToastType type, string memberName, ToastValue value)
    {
        if (!ExtensionMembers.TryGetValue(type, out var members))
        {
            members = [];
            ExtensionMembers[type] = members;
        }
        members[memberName] = value;
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

    public ToastValue Execute(string rawInput, Context context)
    {
        return Executor.Execute(rawInput, context);
    }

    public ToastValue Evaluate(Node node, Context context)
    {
        return Executor.Evaluate(node, context);
    }

    public static bool IsCompatible(ToastType actual, ToastType expected, Context context)
    {
        if (expected == ToastType.Any || expected == actual)
        {
            return true;
        }

        if (expected == ToastType.Error && actual == ToastType.Error)
        {
            return true;
        }

        if (expected == ToastType.Object)
        {
            if (
                actual == ToastType.Object
                || actual == ToastType.String
                || actual == ToastType.List
                || actual == ToastType.Error
            )
            {
                return true;
            }
            if (
                actual != ToastType.Null
                && actual != ToastType.Any
                && actual != ToastType.Number
                && actual != ToastType.Boolean
                && actual != ToastType.Function
            )
            {
                if (context.HasVariable(actual.Name) && context.GetValue(actual.Name) is TypeValue)
                {
                    return true;
                }
            }
        }

        if (
            ToastType.SystemTypeNames.Contains(expected.Name)
            || ToastType.SystemTypeNames.Contains(actual.Name)
        )
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
