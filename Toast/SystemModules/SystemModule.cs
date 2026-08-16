namespace Toast.SystemModules;

[ToastModule("system", "System bundle providing all built-in modules.")]
public class SystemModule : IToastModule
{
    public void OnLoad(Toaster toaster, Context callerContext)
    {
        toaster.Load<DefaultModule>();
        toaster.Load<ObjectModule>();
        toaster.Load<FlowModule>();
        toaster.Load<ConverterModule>();
        toaster.Load<StringModule>();
        toaster.Load<ListModule>();
        toaster.Load<UtilityModule>();
        toaster.Load<ImportModule>();
    }
}
