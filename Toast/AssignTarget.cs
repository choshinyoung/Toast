namespace Toast;

public interface IAssignTarget
{
    string Identifier { get; }
    ToastValue GetValue();
    void SetValue(ToastValue value);
}

public class VariableAssignTarget(Context context, string fieldName) : IAssignTarget
{
    public string Identifier => fieldName;

    public ToastValue GetValue() => context.GetValue(fieldName);

    public void SetValue(ToastValue value) => context.SetValue(fieldName, value);
}

public class ListIndexAssignTarget(ListValue listVal, int index) : IAssignTarget
{
    public string Identifier => $"list[{index}]";

    public ToastValue GetValue() => listVal.Elements[index];

    public void SetValue(ToastValue value) => listVal.Elements[index] = value;
}

public class ObjectPropertyAssignTarget(ObjectValue objVal, string fieldName) : IAssignTarget
{
    public string Identifier => $"{objVal}.{fieldName}";

    public ToastValue GetValue()
    {
        var bindings = objVal.Context.GetBindings();
        if (bindings.TryGetValue(fieldName, out var binding))
        {
            return binding.Value;
        }
        throw new InvalidOperationException(
            $"Property '{fieldName}' is not defined on target object."
        );
    }

    public void SetValue(ToastValue value)
    {
        objVal.Context.SetValueDirect(fieldName, value);
    }
}

