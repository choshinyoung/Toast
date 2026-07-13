namespace Toast;

public record ToastType(string Name)
{
    public static readonly ToastType String = new("string");
    public static readonly ToastType Number = new("number");
    public static readonly ToastType Boolean = new("boolean");
    public static readonly ToastType List = new("list");
    public static readonly ToastType Object = new("object");
    public static readonly ToastType Function = new("function");
    public static readonly ToastType Identifier = new("identifier");
    public static readonly ToastType Null = new("null");
    public static readonly ToastType Any = new("any");
    public static readonly ToastType Reference = new("reference");

    public override string ToString() => Name;
}
