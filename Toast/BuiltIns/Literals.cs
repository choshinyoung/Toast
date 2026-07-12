namespace Toast.BuiltIns;

public static class Literals
{
    public static readonly Command True = Command.CreateFunction("true", (Context context) => true);
    public static readonly Command False = Command.CreateFunction(
        "false",
        (Context context) => false
    );
    public static readonly Command Null = Command.CreateFunction(
        "null",
        (Context context) => (object?)null
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(True);
        toast.RegisterCommand(False);
        toast.RegisterCommand(Null);
    }
}
