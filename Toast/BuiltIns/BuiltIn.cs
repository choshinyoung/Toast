namespace Toast.BuiltIns;

public static class BuiltIn
{
    public static void Register(Toaster toast)
    {
        RegisterConverters(toast);
        RegisterAllCommands(toast);
        RegisterBuiltInTypes(toast);
    }

    public static ToastValue ConvertToType(Context context, ToastValue val, ToastType targetType)
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
        RegisterType(toast, ToastType.Number);
        RegisterType(toast, ToastType.String);
        RegisterType(toast, ToastType.Boolean);
        RegisterType(toast, ToastType.List);
        RegisterType(toast, ToastType.Object);
        RegisterErrorType(toast, ToastType.ErrorValue);
    }

    private static void RegisterType(Toaster toast, ToastType targetType)
    {
        var name = targetType.Name;
        var cmd = new Command(
            name,
            (Context context, ToastValue val) => ConvertToType(context, val, targetType),
            parameterTypes: [ToastType.Any]
        );
        var typeValue = new TypeValue(targetType, cmd);
        toast.GlobalContext.SetValueDirect(name, typeValue);
    }

    private static void RegisterErrorType(Toaster toast, ToastType targetType)
    {
        var name = targetType.Name;
        var cmd = new Command(
            name,
            (Context context, ToastValue[] args) =>
            {
                if (args.Length == 0)
                {
                    return new ErrorValue("Error", "An error occurred", new Location(1, 1), null);
                }

                string errType = "Error";
                string msg = args[0].ToString();
                int line = 1;
                int col = 1;
                ToastValue? cause = null;

                if (args.Length >= 2 && args[1] is StringValue)
                {
                    errType = args[0].ToString();
                    msg = args[1].ToString();
                    line = args.Length > 2 && args[2] is NumberValue ln1 ? (int)ln1.Value : 1;
                    col = args.Length > 3 && args[3] is NumberValue cn1 ? (int)cn1.Value : 1;
                    cause = args.Length > 4 ? args[4] : null;
                }
                else
                {
                    line = args.Length > 1 && args[1] is NumberValue ln2 ? (int)ln2.Value : 1;
                    col = args.Length > 2 && args[2] is NumberValue cn2 ? (int)cn2.Value : 1;
                    cause = args.Length > 3 ? args[3] : null;
                }

                return ErrorValue.Create(errType, msg, new Location(line, col), cause);
            },
            parameterTypes: [ToastType.Any],
            isParameterLazy: [false]
        );
        var declaredMembers = new HashSet<string>
        {
            "errorType",
            "message",
            "line",
            "column",
            "cause",
        };
        var typeValue = new TypeValue(targetType, cmd, declaredMembers);
        toast.GlobalContext.SetValueDirect(name, typeValue);
        toast.GlobalContext.SetValueDirect("ErrorValue", typeValue);
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
