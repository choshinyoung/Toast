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

public record StringValue : ObjectValue
{
    public string Value { get; }
    public override ToastType Type => ToastType.String;

    public StringValue(string value)
        : base(new Context(Toaster.Empty))
    {
        Value = value;
        Context.SetValueDirect("length", new NumberValue(value.Length));

        Context.SetValueDirect(
            "substring",
            new CommandValue(
                Command.CreateFunction(
                    "substring",
                    (Context ctx, NumberValue start, NumberValue len) =>
                    {
                        int s = (int)start.Value;
                        int l = (int)len.Value;
                        return new StringValue(value.Substring(s, l));
                    }
                )
            )
        );

        Context.SetValueDirect(
            "contains",
            new CommandValue(
                Command.CreateFunction(
                    "contains",
                    (Context ctx, StringValue search) =>
                    {
                        return new BoolValue(value.Contains(search.Value));
                    }
                )
            )
        );
    }

    public override string ToString() => Value;

    public virtual bool Equals(StringValue? other)
    {
        if (other is null)
            return false;
        return Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
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

public record ListValue : ObjectValue
{
    public List<ToastValue> Elements { get; }
    public override ToastType Type => ToastType.List;

    public ListValue(List<ToastValue> elements)
        : base(new Context(Toaster.Empty))
    {
        Elements = elements;
        Context.SetValueDirect("length", new NumberValue(elements.Count));

        Context.SetValueDirect(
            "add",
            new CommandValue(
                Command.CreateFunction(
                    "add",
                    (Context ctx, ToastValue item) =>
                    {
                        elements.Add(item);
                        Context.SetValueDirect("length", new NumberValue(elements.Count));
                        return NullValue.Instance;
                    }
                )
            )
        );

        Context.SetValueDirect(
            "removeAt",
            new CommandValue(
                Command.CreateFunction(
                    "removeAt",
                    (Context ctx, NumberValue index) =>
                    {
                        int i = (int)index.Value;
                        var removed = elements[i];
                        elements.RemoveAt(i);
                        Context.SetValueDirect("length", new NumberValue(elements.Count));
                        return removed;
                    }
                )
            )
        );
    }

    public override string ToString() => "[" + string.Join(", ", Elements) + "]";

    public virtual bool Equals(ListValue? other)
    {
        if (other is null)
            return false;
        if (Elements.Count != other.Elements.Count)
            return false;
        for (int i = 0; i < Elements.Count; i++)
        {
            if (!Elements[i].Equals(other.Elements[i]))
                return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        foreach (var el in Elements)
        {
            hash = hash * 31 + (el?.GetHashCode() ?? 0);
        }
        return hash;
    }
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
