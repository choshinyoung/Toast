namespace Toast.SystemModules;

[ToastModule("import", Description = "import function.")]
public class ImportModule : IToastModule
{
    [ToastCommand("import", Description = "Imports a module or globally installed package.")]
    public static ToastValue Import(Context context, StringValue moduleName)
    {
        ModuleManager.Instance.LoadModule(moduleName.Value, context.Toaster, context);
        return NullValue.Instance;
    }
}
