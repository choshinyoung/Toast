namespace Toast;

public class TypeConverter(
    ToastType source,
    ToastType target,
    Func<Context, ToastValue, ToastValue> convertFunc
)
{
    public ToastType Source { get; } = source;
    public ToastType Target { get; } = target;
    public Func<Context, ToastValue, ToastValue> ConvertFunc { get; } = convertFunc;
}
