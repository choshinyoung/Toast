namespace Toast.BuiltIns;

public static class Literals
{
    public static readonly Command True = Command.CreateFunction(
        "true",
        (Context context) => new BoolValue(true)
    );
    public static readonly Command False = Command.CreateFunction(
        "false",
        (Context context) => new BoolValue(false)
    );
    public static readonly Command Null = Command.CreateFunction(
        "null",
        (Context context) => NullValue.Instance
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(True);
        toast.RegisterCommand(False);
        toast.RegisterCommand(Null);
    }
}
