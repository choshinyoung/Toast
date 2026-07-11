namespace Toast;

public class TypeConverter(ToastType source, ToastType target, Func<object?, object?> convertFunc)
{
    public ToastType Source { get; } = source;
    public ToastType Target { get; } = target;
    public Func<object?, object?> ConvertFunc { get; } = convertFunc;
}
