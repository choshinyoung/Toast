namespace Toast.SystemModules;

[ToastModule("import", "import function.")]
public class ImportModule : IToastModule
{
    [ToastCommand("import", "Imports a module or globally installed package.")]
    public static ToastValue Import(Context context, StringValue moduleName)
    {
        ModuleManager.Instance.LoadModule(moduleName.Value, context.Toaster, context);
        return NullValue.Instance;
    }
}
