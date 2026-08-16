namespace Toast.SystemModules;

public partial class DefaultModule
{
    [ToastCommand("true", Description = "Boolean true literal.")]
    public static BoolValue True() => new(true);

    [ToastCommand("false", Description = "Boolean false literal.")]
    public static BoolValue False() => new(false);

    [ToastCommand("null", Description = "Null literal representing the absence of a value.")]
    public static NullValue Null() => NullValue.Instance;
}
