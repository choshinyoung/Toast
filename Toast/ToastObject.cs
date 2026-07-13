namespace Toast;

public abstract record ToastObject
{
    public abstract ToastType Type { get; }
}

public sealed record NullValue : ToastObject
{
    public static readonly NullValue Instance = new();

    private NullValue() { }

    public override ToastType Type => ToastType.Null;

    public override string ToString() => "null";
}

public sealed record StringValue(string Value) : ToastObject
{
    public override ToastType Type => ToastType.String;

    public override string ToString() => Value;
}

public sealed record NumberValue(double Value) : ToastObject
{
    public override ToastType Type => ToastType.Number;

    public override string ToString() => Value.ToString();
}

public sealed record BoolValue(bool Value) : ToastObject
{
    public override ToastType Type => ToastType.Boolean;

    public override string ToString() => Value ? "true" : "false";
}

public sealed record ListValue(List<ToastObject> Elements) : ToastObject
{
    public override ToastType Type => ToastType.List;

    public override string ToString() => "[" + string.Join(", ", Elements) + "]";
}

public sealed record ObjectValue(Context Context, ToastType? CustomType = null) : ToastObject
{
    public override ToastType Type => CustomType ?? ToastType.Object;

    public override string ToString()
    {
        var bindings = Context.GetBindings();
        var items = bindings.Select(kvp => $"{kvp.Key}: {kvp.Value}");
        return $"{{{string.Join(", ", items)}}}";
    }
}

public sealed record FunctionValue(
    IReadOnlyList<ParameterNode> Parameters,
    IReadOnlyList<Node> Statements,
    Context ClosureContext,
    Toaster Toaster
) : ToastObject
{
    public override ToastType Type => ToastType.Function;

    public override string ToString() => "function";

    public ToastObject Execute(List<ToastObject> evalArgs)
    {
        var runContext = new Context(ClosureContext);
        for (int i = 0; i < Parameters.Count; i++)
        {
            var param = Parameters[i];
            var argVal = i < evalArgs.Count ? evalArgs[i] : NullValue.Instance;
            runContext.SetValueDirect(param.Name, argVal);
        }

        ToastObject lastVal = NullValue.Instance;
        foreach (var stmt in Statements)
        {
            lastVal = Toaster.Executor.Evaluate(stmt, runContext);
        }
        return lastVal;
    }
}

public sealed record CommandValue(Command Command) : ToastObject
{
    public override ToastType Type => ToastType.Function;

    public override string ToString() => $"command:{Command.Name}";
}

public sealed record IdentifierValue(string Name) : ToastObject
{
    public override ToastType Type => ToastType.Identifier;

    public override string ToString() => Name;
}

public sealed record AstNodeValue(Node Node) : ToastObject
{
    public override ToastType Type => ToastType.Any;

    public override string ToString() => Node.ToString();
}
