namespace Toast.BuiltIns;

public static class BuiltIn
{
    public static void Register(Toaster toast)
    {
        RegisterConverters(toast);
        RegisterAllCommands(toast);
    }

    public static void RegisterConverters(Toaster toast)
    {
        Converters.Register(toast);
    }

    public static void RegisterAllCommands(Toaster toast)
    {
        RegisterLiterals(toast);
        RegisterVariables(toast);
        RegisterMath(toast);
        RegisterRelational(toast);
        RegisterLogical(toast);
        RegisterBitwise(toast);
        RegisterControlFlow(toast);
        RegisterList(toast);
        RegisterString(toast);
        RegisterUtility(toast);
    }

    public static void RegisterLiterals(Toaster toast)
    {
        Literals.Register(toast);
    }

    public static void RegisterVariables(Toaster toast)
    {
        Variables.Register(toast);
    }

    public static void RegisterMath(Toaster toast)
    {
        Math.Register(toast);
    }

    public static void RegisterRelational(Toaster toast)
    {
        Relational.Register(toast);
    }

    public static void RegisterLogical(Toaster toast)
    {
        Logical.Register(toast);
    }

    public static void RegisterBitwise(Toaster toast)
    {
        Bitwise.Register(toast);
    }

    public static void RegisterControlFlow(Toaster toast)
    {
        ControlFlow.Register(toast);
    }

    public static void RegisterList(Toaster toast)
    {
        List.Register(toast);
    }

    public static void RegisterString(Toaster toast)
    {
        String.Register(toast);
    }

    public static void RegisterUtility(Toaster toast)
    {
        Utility.Register(toast);
    }
}
