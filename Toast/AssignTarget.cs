namespace Toast;

public interface IAssignTarget
{
    string Identifier { get; }
    ToastObject GetValue();
    void SetValue(ToastObject value);
}

public class VariableAssignTarget(Context context, string fieldName) : IAssignTarget
{
    public string Identifier => fieldName;

    public ToastObject GetValue() => context.GetValue(fieldName);

    public void SetValue(ToastObject value) => context.SetValue(fieldName, value);
}

public class ListIndexAssignTarget(ListValue listVal, int index) : IAssignTarget
{
    public string Identifier => $"list[{index}]";

    public ToastObject GetValue() => listVal.Elements[index];

    public void SetValue(ToastObject value) => listVal.Elements[index] = value;
}
