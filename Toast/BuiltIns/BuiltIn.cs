namespace Toast.BuiltIns;

public static class BuiltIn
{
    public static void Register(Toaster toast)
    {
        RegisterConverters(toast);
        RegisterAllCommands(toast);
        RegisterBuiltInTypes(toast);
    }

    public static ToastObject ConvertToType(Context context, ToastObject val, ToastType targetType)
    {
        var sourceType = val.Type;
        if (sourceType == targetType)
            return val;

        if (context.Toaster.TryConvert(val, sourceType, targetType, context, out var converted))
        {
            return converted;
        }

        throw new InvalidOperationException(
            $"No converter registered from {sourceType} to {targetType}."
        );
    }

    public static void RegisterBuiltInTypes(Toaster toast)
    {
        RegisterType(toast, ToastType.Number, "number");
        RegisterType(toast, ToastType.String, "string");
        RegisterType(toast, ToastType.Boolean, "boolean");
        RegisterType(toast, ToastType.List, "list");
        RegisterType(toast, ToastType.Object, "object");
    }

    private static void RegisterType(Toaster toast, ToastType targetType, string name)
    {
        var cmd = new Command(
            name,
            (Context context, ToastObject val) => ConvertToType(context, val, targetType),
            parameterTypes: [ToastType.Any]
        );
        var typeValue = new TypeValue(targetType, cmd);
        toast.GlobalContext.SetValueDirect(name, typeValue);
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
        DateTimeBuiltIn.Register(toast);
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
