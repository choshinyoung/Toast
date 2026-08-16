namespace Toast.SystemModules;

public partial class DefaultModule
{
    [ToastCommand("true", "Boolean true literal.")]
    public static BoolValue True() => new(true);

    [ToastCommand("false", "Boolean false literal.")]
    public static BoolValue False() => new(false);

    [ToastCommand("null", "Null literal representing the absence of a value.")]
    public static NullValue Null() => NullValue.Instance;
}
