namespace Toast.SystemModules;

public partial class DefaultModule
{
    public static readonly Command True = Command.CreateFunction(
        "true",
        (Context context) => new BoolValue(true),
        description: "Boolean true literal.",
        returnType: ToastType.Boolean
    );
    public static readonly Command False = Command.CreateFunction(
        "false",
        (Context context) => new BoolValue(false),
        description: "Boolean false literal.",
        returnType: ToastType.Boolean
    );
    public static readonly Command Null = Command.CreateFunction(
        "null",
        (Context context) => NullValue.Instance,
        description: "Null literal representing the absence of a value.",
        returnType: ToastType.Null
    );

    public static void RegisterLiterals(Toaster toast)
    {
        toast.RegisterCommand(True);
        toast.RegisterCommand(False);
        toast.RegisterCommand(Null);
    }
}
