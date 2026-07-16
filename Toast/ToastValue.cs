namespace Toast;

public abstract record ToastValue
{
    public abstract ToastType Type { get; }
}

public record NullValue : ToastValue
{
    public static readonly NullValue Instance = new();

    private NullValue() { }

    public override ToastType Type => ToastType.Null;

    public override string ToString() => "null";
}

public record StringValue(string Value) : ToastValue
{
    public override ToastType Type => ToastType.String;

    public override string ToString() => Value;
}

public record NumberValue(double Value) : ToastValue
{
    public override ToastType Type => ToastType.Number;

    public override string ToString() => Value.ToString();
}

public record BoolValue(bool Value) : ToastValue
{
    public override ToastType Type => ToastType.Boolean;

    public override string ToString() => Value ? "true" : "false";
}

public record ListValue(List<ToastValue> Elements) : ToastValue
{
    public override ToastType Type => ToastType.List;

    public override string ToString() => "[" + string.Join(", ", Elements) + "]";
}

public record ObjectValue(Context Context, ToastType? CustomType = null) : ToastValue
{
    public override ToastType Type => CustomType ?? ToastType.Object;

    public override string ToString()
    {
        var bindings = Context.GetBindings();
        var items = bindings.Select(kvp => $"{kvp.Key}: {kvp.Value.Value}");
        return $"{{{string.Join(", ", items)}}}";
    }
}

public record FunctionValue(
    IReadOnlyList<ParameterNode> Parameters,
    IReadOnlyList<Node> Statements,
    Context ClosureContext,
    Toaster Toaster
) : ToastValue
{
    public override ToastType Type => ToastType.Function;

    public override string ToString() => "function";

    public ToastValue Execute(List<ToastValue> evalArgs)
    {
        var runContext = new Context(ClosureContext);
        for (int i = 0; i < Parameters.Count; i++)
        {
            var param = Parameters[i];
            var argVal = i < evalArgs.Count ? evalArgs[i] : NullValue.Instance;
            if (param.Type != null)
            {
                var expectedType = param.Type.Type;
                if (argVal.Type != expectedType && expectedType != ToastType.Any)
                {
                    if (
                        Toaster.TryConvert(
                            argVal,
                            argVal.Type,
                            expectedType,
                            runContext,
                            out var converted
                        )
                    )
                    {
                        argVal = converted;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Type mismatch: Parameter '{param.Name}' expects {expectedType}, but got {argVal.Type}."
                        );
                    }
                }
            }
            runContext.SetValueDirect(param.Name, argVal);
        }

        ToastValue lastVal = NullValue.Instance;
        foreach (var stmt in Statements)
        {
            lastVal = Toaster.Executor.Evaluate(stmt, runContext);
        }
        return lastVal;
    }
}

public record CommandValue(Command Command) : ToastValue
{
    public override ToastType Type => ToastType.Function;

    public override string ToString() => "function";
}

public record TypeValue : ToastValue
{
    public static readonly TypeValue Any = new(ToastType.Any, null);

    public ToastType TargetType { get; }
    public Command? Constructor { get; }
    public HashSet<string> DeclaredMembers { get; }
    public IReadOnlyDictionary<string, ToastType> MemberTypes { get; }

    public TypeValue(
        ToastType targetType,
        Command? constructor,
        HashSet<string>? declaredMembers = null,
        IReadOnlyDictionary<string, ToastType>? memberTypes = null
    )
    {
        TargetType = targetType;
        Constructor = constructor;
        DeclaredMembers = declaredMembers ?? [];
        MemberTypes = memberTypes ?? new Dictionary<string, ToastType>();
    }

    public override ToastType Type => ToastType.Type;

    public override string ToString()
    {
        if (TargetType.Name == "@type_factory")
        {
            var sortedMembers = DeclaredMembers.OrderBy(m => m);
            return $"(type: {{ {string.Join(", ", sortedMembers)} }})";
        }
        return $"(type: {TargetType.Name})";
    }
}

public record IdentifierValue(string Name) : ToastValue
{
    public override ToastType Type => ToastType.Identifier;

    public override string ToString() => Name;
}

public record AstNodeValue(Node Node) : ToastValue
{
    public override ToastType Type => ToastType.Any;

    public override string ToString() => Node.ToString();
}

public record ReferenceValue(IAssignTarget Target) : ToastValue
{
    public override ToastType Type => ToastType.Reference;

    public override string ToString() => $"(ref: {Target.Identifier})";
}

public record TypedIdentifierValue(string Name, TypeValue TargetTypeVal) : IdentifierValue(Name)
{
    public override string ToString() => $"{Name}: {TargetTypeVal}";
}
