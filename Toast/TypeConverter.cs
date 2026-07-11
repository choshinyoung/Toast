namespace Toast;

public class TypeConverter(
    ToastType source,
    ToastType target,
    Func<Context, object?, object?> convertFunc
)
{
    public ToastType Source { get; } = source;
    public ToastType Target { get; } = target;
    public Func<Context, object?, object?> ConvertFunc { get; } = convertFunc;
}
