namespace Toast.SystemModules;

[ToastModule("converter", "Standard type converters between types.")]
public class ConverterModule : IToastModule
{
    [ToastConverter]
    public static StringValue NumberToString(NumberValue val)
    {
        return new StringValue(val.Value.ToString());
    }

    [ToastConverter]
    public static StringValue BooleanToString(BoolValue val)
    {
        return new StringValue(val.Value ? "true" : "false");
    }

    [ToastConverter]
    public static StringValue ListToString(Context ctx, ListValue listVal)
    {
        var list = new List<string>();
        foreach (var x in listVal.Elements)
        {
            var type = x.Type;
            if (ctx.Toaster.TryConvert(x, type, ToastType.String, ctx, out var converted))
            {
                list.Add(converted.ToString());
                continue;
            }
            list.Add(x.ToString());
        }
        return new StringValue($"[{string.Join(", ", list)}]");
    }

    [ToastConverter]
    public static StringValue ObjectToString(Context ctx, ObjectValue objVal)
    {
        var bindings = objVal.Context.GetBindings();
        var items = new List<string>();
        foreach (var kvp in bindings)
        {
            var innerVal = kvp.Value.Value;
            var type = innerVal.Type;
            if (ctx.Toaster.TryConvert(innerVal, type, ToastType.String, ctx, out var converted))
            {
                items.Add($"{kvp.Key}: {converted}");
            }
            else
            {
                items.Add($"{kvp.Key}: {innerVal}");
            }
        }
        return new StringValue($"{{{string.Join(", ", items)}}}");
    }

    [ToastConverter(SourceTypeName = "function", TargetTypeName = "string")]
    public static StringValue FunctionToString(ToastValue val)
    {
        return new StringValue(val.ToString());
    }

    [ToastConverter]
    public static NumberValue StringToNumber(StringValue val)
    {
        if (double.TryParse(val.Value, out var num))
        {
            return new NumberValue(num);
        }
        throw new ToastException(new TypeError($"Cannot convert string '{val.Value}' to number."));
    }

    [ToastConverter]
    public static BoolValue StringToBoolean(StringValue val)
    {
        if (bool.TryParse(val.Value, out var b))
        {
            return new BoolValue(b);
        }
        throw new ToastException(new TypeError($"Cannot convert string '{val.Value}' to boolean."));
    }

    [ToastConverter]
    public static ListValue StringToList(StringValue val)
    {
        return new ListValue([.. val.Value.Select(c => (ToastValue)new StringValue(c.ToString()))]);
    }
}
