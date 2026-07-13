namespace Toast;

public class TypeConverter(
    ToastType source,
    ToastType target,
    Func<Context, ToastObject, ToastObject> convertFunc
)
{
    public ToastType Source { get; } = source;
    public ToastType Target { get; } = target;
    public Func<Context, ToastObject, ToastObject> ConvertFunc { get; } = convertFunc;
}
