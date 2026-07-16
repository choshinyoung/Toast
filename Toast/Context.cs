namespace Toast;

public class Context(Toaster toaster, Context? parent = null)
{
    private readonly Context? _parent = parent;
    private readonly Dictionary<string, ToastValue> _bindings = [];

    public Toaster Toaster { get; } = toaster;

    public Context(Context parent)
        : this(parent.Toaster, parent) { }

    public IReadOnlyDictionary<string, ToastValue> GetBindings() => _bindings;

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

    public bool HasVariable(string name)
    {
        return FindContext(name) != null;
    }

    public void GetOrCreateLocal(string name)
    {
        if (!_bindings.ContainsKey(name))
        {
            _bindings[name] = NullValue.Instance;
        }
    }

    public ToastValue GetValue(string name)
    {
        var ctx =
            FindContext(name)
            ?? throw new InvalidOperationException($"Variable '{name}' is not defined.");
        return ctx.GetValueDirect(name);
    }

    public ToastValue GetValueDirect(string name) =>
        _bindings.TryGetValue(name, out var val)
            ? val
            : throw new InvalidOperationException(
                $"Variable '{name}' is not defined in this context."
            );

    public void SetValue(string name, ToastValue value)
    {
        var ctx =
            FindContext(name)
            ?? throw new InvalidOperationException($"Variable '{name}' is not defined.");
        ctx.SetValueDirect(name, value);
    }

    public void SetValueDirect(string name, ToastValue value) => _bindings[name] = value;
}
