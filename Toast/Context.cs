namespace Toast;

public class Context(Toaster toaster, Context? parent = null)
{
    private readonly Context? _parent = parent;
    private readonly Dictionary<string, (ToastValue Value, TypeValue Constraint)> _bindings = [];
    private Toaster _toaster = toaster ?? new Toaster();

    public Toaster Toaster
    {
        get => _toaster;
        set => _toaster = value ?? throw new ArgumentNullException(nameof(value));
    }
    public Context? Parent => _parent;

    public Context(Context parent)
        : this(parent.Toaster, parent) { }

    public IReadOnlyDictionary<string, (ToastValue Value, TypeValue Constraint)> GetBindings() =>
        _bindings;

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

        if (Toaster != null && this != Toaster.GlobalContext)
        {
            return Toaster.GlobalContext?.FindContext(name);
        }

        return null;
    }

    public bool HasVariable(string name)
    {
        return FindContext(name) != null;
    }

    public void GetOrCreateLocal(string name, TypeValue? constraint = null)
    {
        if (!_bindings.ContainsKey(name))
        {
            _bindings[name] = (NullValue.Instance, constraint ?? TypeValue.Any);
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
        _bindings.TryGetValue(name, out var binding)
            ? binding.Value
            : throw new InvalidOperationException(
                $"Variable '{name}' is not defined in this context."
            );

    public TypeValue GetConstraint(string name)
    {
        var ctx =
            FindContext(name)
            ?? throw new InvalidOperationException($"Variable '{name}' is not defined.");
        return ctx._bindings[name].Constraint;
    }

    public void SetValue(string name, ToastValue value)
    {
        var ctx =
            FindContext(name)
            ?? throw new InvalidOperationException($"Variable '{name}' is not defined.");
        ctx.SetValueDirect(name, value);
    }

    public void SetValueDirect(string name, ToastValue value)
    {
        if (value is ObjectValue objVal && objVal.Context.Toaster != Toaster)
        {
            objVal.Context.Toaster = Toaster;
        }

        if (_bindings.TryGetValue(name, out var binding))
        {
            if (
                Toaster != null
                && !Toaster.IsCompatible(value.Type, binding.Constraint.TargetType, this)
            )
            {
                throw new InvalidOperationException(
                    $"Type mismatch: Cannot assign value of type {value.Type} to variable '{name}' which is constrained to {binding.Constraint.TargetType}."
                );
            }
            _bindings[name] = (value, binding.Constraint);
        }
        else
        {
            _bindings[name] = (value, TypeValue.Any);
        }
    }

    public void SetConstraintDirect(string name, TypeValue constraint)
    {
        if (_bindings.TryGetValue(name, out var binding))
        {
            _bindings[name] = (binding.Value, constraint);
        }
    }
}
