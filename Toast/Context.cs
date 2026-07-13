namespace Toast;

public readonly record struct MemoryAddress(Context Context, string Name)
{
    public override string ToString() => $"Ref({Name}:{Context.GetHashCode():X8})";

    public object? GetValue() => Context.GetValueDirect(Name);

    public void SetValue(object? value) => Context.SetValueDirect(Name, value);
}

public class Context(Toaster toaster, Context? parent = null)
{
    private readonly Context? _parent = parent;
    private readonly Dictionary<string, object?> _bindings = [];

    public Toaster Toaster { get; } = toaster;

    public Context(Context parent)
        : this(parent.Toaster, parent) { }

    public MemoryAddress GetOrCreateAddress(string name)
    {
        var addr = LookupAddress(name);
        if (addr != null)
        {
            return addr.Value;
        }

        _bindings[name] = null;
        return new MemoryAddress(this, name);
    }

    private Context? FindContext(string name)
    {
        if (_bindings.ContainsKey(name))
        {
            return this;
        }

        if (_parent != null)
        {
            return _parent.FindContext(name);
        }

        if (this != Toaster.GlobalContext)
        {
            return Toaster.GlobalContext.FindContext(name);
        }

        return null;
    }

    public MemoryAddress? LookupAddress(string name)
    {
        var ctx = FindContext(name);
        return ctx != null ? new MemoryAddress(ctx, name) : null;
    }

    public object? GetValue(string name)
    {
        var ctx =
            FindContext(name)
            ?? throw new InvalidOperationException($"Variable '{name}' is not defined.");
        return ctx.GetValueDirect(name);
    }

    public object? GetValueDirect(string name) =>
        _bindings.TryGetValue(name, out var val)
            ? val
            : throw new InvalidOperationException(
                $"Variable '{name}' is not defined in this context."
            );

    public void SetValueDirect(string name, object? value) => _bindings[name] = value;
}
