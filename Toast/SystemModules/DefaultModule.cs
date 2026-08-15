namespace Toast.SystemModules;

public partial class DefaultModule : IToastModule
{
    public string Name => "default";
    public string Description =>
        "Default primitive operators (arithmetic, relational, logical, bitwise, literals, pipeline, quote)";

    public static void Register(Toaster toast)
    {
        RegisterLiterals(toast);
        RegisterArithmetic(toast);
        RegisterRelational(toast);
        RegisterLogical(toast);
        RegisterBitwise(toast);
        RegisterPipeline(toast);
    }

    public void Load(Toaster toaster, Context callerContext)
    {
        Register(toaster);
    }
}
