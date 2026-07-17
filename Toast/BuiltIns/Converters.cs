namespace Toast.BuiltIns;

public static class Converters
{
    public static readonly TypeConverter NumberToString = new(
        ToastType.Number,
        ToastType.String,
        (_, val) => new StringValue(((NumberValue)val).Value.ToString())
    );

    public static readonly TypeConverter BooleanToString = new(
        ToastType.Boolean,
        ToastType.String,
        (_, val) => new StringValue(((BoolValue)val).Value ? "True" : "False")
    );

    public static readonly TypeConverter ListToString = new(
        ToastType.List,
        ToastType.String,
        (ctx, val) =>
        {
            if (val is ListValue listVal)
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
            return new StringValue("[]");
        }
    );

    public static readonly TypeConverter ObjectToString = new(
        ToastType.Object,
        ToastType.String,
        (ctx, val) =>
        {
            if (val is ObjectValue objVal)
            {
                var bindings = objVal.Context.GetBindings();
                var items = new List<string>();
                foreach (var kvp in bindings)
                {
                    var innerVal = kvp.Value.Value;
                    var type = innerVal.Type;
                    if (
                        ctx.Toaster.TryConvert(
                            innerVal,
                            type,
                            ToastType.String,
                            ctx,
                            out var converted
                        )
                    )
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
            return new StringValue("{}");
        }
    );

    public static readonly TypeConverter FunctionToString = new(
        ToastType.Function,
        ToastType.String,
        (_, val) => new StringValue(val.ToString())
    );

    public static readonly TypeConverter StringToNumber = new(
        ToastType.String,
        ToastType.Number,
        (_, val) => new NumberValue(double.Parse(((StringValue)val).Value))
    );

    public static readonly TypeConverter StringToBoolean = new(
        ToastType.String,
        ToastType.Boolean,
        (_, val) => new BoolValue(bool.Parse(((StringValue)val).Value))
    );

    public static readonly TypeConverter StringToList = new(
        ToastType.String,
        ToastType.List,
        (_, val) =>
            new ListValue(
                ((StringValue)val)
                    .Value.Select(c => (ToastValue)new StringValue(c.ToString()))
                    .ToList()
            )
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterConverter(NumberToString);
        toast.RegisterConverter(BooleanToString);
        toast.RegisterConverter(ListToString);
        toast.RegisterConverter(ObjectToString);
        toast.RegisterConverter(FunctionToString);
        toast.RegisterConverter(StringToNumber);
        toast.RegisterConverter(StringToBoolean);
        toast.RegisterConverter(StringToList);
    }
}
