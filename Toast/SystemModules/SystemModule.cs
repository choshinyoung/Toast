namespace Toast.SystemModules;

public class SystemModule : IToastModule
{
    public string Name => "system";
    public string Description =>
        "System bundle providing all built-in modules (default, object, flow, string, list, utility, converter, import)";

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
        RegisterType(toast, ToastType.Number, "Converts a value to a number type.");
        RegisterType(toast, ToastType.String, "Converts a value to a string type.");
        RegisterType(toast, ToastType.Boolean, "Converts a value to a boolean type.");
        RegisterType(toast, ToastType.List, "Converts a value to a list type.");
        RegisterType(toast, ToastType.Object, "Converts a value to an object type.");
        RegisterErrorType(toast, ToastType.Error);
    }

    private static void RegisterType(Toaster toast, ToastType targetType, string description = "")
    {
        var name = targetType.Name;
        var cmd = new Command(
            name,
            (Context context, ToastValue val) => ConvertToType(context, val, targetType),
            parameterTypes: [ToastType.Any],
            description: description,
            returnType: targetType
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
            isParameterLazy: [false],
            description: "Creates an Error object with error type, message, location, and optional cause.",
            returnType: targetType
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
    }

    public static void RegisterConverters(Toaster toast)
    {
        ConverterModule.Register(toast);
    }

    public static void RegisterAllCommands(Toaster toast)
    {
        ImportModule.Register(toast);
        DefaultModule.Register(toast);
        ObjectModule.Register(toast);
        FlowModule.Register(toast);
        ListModule.Register(toast);
        StringModule.Register(toast);
        UtilityModule.Register(toast);
    }

    public void Load(Toaster toaster, Context callerContext)
    {
        new DefaultModule().Load(toaster, callerContext);
        new ObjectModule().Load(toaster, callerContext);
        new FlowModule().Load(toaster, callerContext);
        new ConverterModule().Load(toaster, callerContext);
        new StringModule().Load(toaster, callerContext);
        new ListModule().Load(toaster, callerContext);
        new UtilityModule().Load(toaster, callerContext);
        new ImportModule().Load(toaster, callerContext);
    }
}
