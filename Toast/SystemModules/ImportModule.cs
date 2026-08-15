namespace Toast.SystemModules;

public class ImportModule : IToastModule
{
    public string Name => "import";
    public string Description => "import function.";

    public static readonly Command ImportCommand = Command.CreateFunction(
        "import",
        (Context context, StringValue moduleName) =>
        {
            if (context.Parent != null)
            {
                throw new ToastException(
                    new RuntimeError("'import' can only be declared at top-level (depth 0).")
                );
            }
            ModuleManager.Instance.LoadModule(moduleName.Value, context.Toaster, context);
            return NullValue.Instance;
        },
        parameterTypes: [ToastType.String],
        description: "Imports a module or globally installed package.",
        returnType: ToastType.Null
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(ImportCommand);
    }

    public void Load(Toaster toaster, Context callerContext)
    {
        Register(toaster);
    }
}
