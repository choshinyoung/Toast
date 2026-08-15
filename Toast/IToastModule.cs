namespace Toast;

public interface IToastModule
{
    string Name { get; }
    string Description => "";
    void Load(Toaster toaster, Context callerContext);
}
