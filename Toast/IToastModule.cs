namespace Toast;

public interface IToastModule
{
    string Name => ModuleLoader.GetModuleName(GetType());
    string Description => ModuleLoader.GetModuleDescription(GetType());
    void OnLoad(Toaster toaster, Context callerContext) { }
}
