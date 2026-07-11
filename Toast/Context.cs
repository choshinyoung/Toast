namespace Toast;

public class MemoryAddress(string name, Guid id)
{
    public string Name { get; } = name;
    public Guid Id { get; } = id;

    public override string ToString() => $"Ref({Name}:{Id.ToString()[..8]})";
}

public class Context(Toaster toaster, Context? parent = null)
{
    private readonly Context? _parent = parent;
    private readonly Dictionary<string, MemoryAddress> _bindings = [];
    private readonly Dictionary<MemoryAddress, object?> _memory = [];

    public Toaster Toaster { get; } = toaster;
    public int Depth { get; } = parent == null ? 0 : parent.Depth + 1;

    public Context(Context parent)
        : this(parent.Toaster, parent) { }

    public MemoryAddress GetOrCreateAddress(string name)
    {
        var addr = LookupAddress(name);
        if (addr != null)
        {
            return addr;
        }

        addr = new MemoryAddress(name, Guid.NewGuid());
        _bindings[name] = addr;
        _memory[addr] = null;

        return addr;
    }

    public MemoryAddress? LookupAddress(string name)
    {
        if (_bindings.TryGetValue(name, out var addr))
        {
            return addr;
        }

        return _parent?.LookupAddress(name);
    }

    public object? GetValue(string name)
    {
        var addr =
            LookupAddress(name)
            ?? throw new InvalidOperationException($"Variable '{name}' is not defined.");

        return GetValueAtAddress(addr);
    }

    public object? GetValueAtAddress(MemoryAddress address)
    {
        if (_memory.TryGetValue(address, out var val))
        {
            return val;
        }

        if (_parent != null)
        {
            return _parent.GetValueAtAddress(address);
        }

        throw new InvalidOperationException(
            $"Memory address '{address}' is invalid or inaccessible in this context."
        );
    }

    public void SetValueAtAddress(MemoryAddress address, object? value)
    {
        if (_memory.ContainsKey(address))
        {
            _memory[address] = value;
            return;
        }

        if (_parent != null)
        {
            _parent.SetValueAtAddress(address, value);
            return;
        }

        _memory[address] = value;
    }
}
