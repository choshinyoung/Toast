namespace Toast.BuiltIns;

public static class Converters
{
    public static readonly TypeConverter IntegerToFloat = new TypeConverter(
        ToastType.Integer,
        ToastType.Float,
        (_, val) => Convert.ToDouble(val)
    );

    public static readonly TypeConverter IntegerToString = new TypeConverter(
        ToastType.Integer,
        ToastType.String,
        (_, val) => val?.ToString()
    );

    public static readonly TypeConverter FloatToString = new TypeConverter(
        ToastType.Float,
        ToastType.String,
        (_, val) => val?.ToString()
    );

    public static readonly TypeConverter BooleanToString = new TypeConverter(
        ToastType.Boolean,
        ToastType.String,
        (_, val) => val?.ToString()
    );

    public static readonly TypeConverter ListToString = new TypeConverter(
        ToastType.List,
        ToastType.String,
        (ctx, val) =>
        {
            if (val is System.Collections.IEnumerable enumerable)
            {
                var list = new List<string>();
                foreach (var x in enumerable)
                {
                    var type = Executor.GetToastType(x);
                    if (ctx.Toaster.TryConvert(x, type, ToastType.String, ctx, out var converted))
                    {
                        list.Add(converted?.ToString() ?? "null");
                        continue;
                    }
                    list.Add(x?.ToString() ?? "null");
                }
                return $"[{string.Join(", ", list)}]";
            }
            return "[]";
        }
    );

    public static readonly TypeConverter StringToInteger = new TypeConverter(
        ToastType.String,
        ToastType.Integer,
        (_, val) => int.Parse((string)val!)
    );

    public static readonly TypeConverter StringToFloat = new TypeConverter(
        ToastType.String,
        ToastType.Float,
        (_, val) => double.Parse((string)val!)
    );

    public static readonly TypeConverter StringToBoolean = new TypeConverter(
        ToastType.String,
        ToastType.Boolean,
        (_, val) => bool.Parse((string)val!)
    );

    public static readonly TypeConverter StringToList = new TypeConverter(
        ToastType.String,
        ToastType.List,
        (_, val) => ((string)val!).Select(c => c.ToString()).ToList()
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterConverter(IntegerToFloat);
        toast.RegisterConverter(IntegerToString);
        toast.RegisterConverter(FloatToString);
        toast.RegisterConverter(BooleanToString);
        toast.RegisterConverter(ListToString);
        toast.RegisterConverter(StringToInteger);
        toast.RegisterConverter(StringToFloat);
        toast.RegisterConverter(StringToBoolean);
        toast.RegisterConverter(StringToList);
    }
}
