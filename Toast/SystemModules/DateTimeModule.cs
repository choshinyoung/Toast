namespace Toast.SystemModules;

public class DateTimeModule : IToastModule
{
    public string Name => "datetime";
    public string Description => "Date and time manipulation module";

    public static readonly ToastType DateTimeType = new("datetime");

    public static ObjectValue CreateDateTimeObject(Toaster toaster, DateTime dt)
    {
        var objCtx = new Context(toaster.GlobalContext);
        objCtx.SetValueDirect("year", new NumberValue(dt.Year));
        objCtx.SetValueDirect("month", new NumberValue(dt.Month));
        objCtx.SetValueDirect("day", new NumberValue(dt.Day));
        objCtx.SetValueDirect("hour", new NumberValue(dt.Hour));
        objCtx.SetValueDirect("minute", new NumberValue(dt.Minute));
        objCtx.SetValueDirect("second", new NumberValue(dt.Second));

        var addDays = new Command(
            "addDays",
            (Context context, NumberValue days) =>
                CreateDateTimeObject(context.Toaster, dt.AddDays(days.Value)),
            parameterTypes: [ToastType.Number],
            description: "Adds the specified number of days to the datetime value.",
            returnType: DateTimeType
        );
        objCtx.SetValueDirect("addDays", new CommandValue(addDays));

        var format = new Command(
            "format",
            (Context context, StringValue fmt) => new StringValue(dt.ToString(fmt.Value)),
            parameterTypes: [ToastType.String],
            description: "Formats the datetime value using a standard format string.",
            returnType: ToastType.String
        );
        objCtx.SetValueDirect("format", new CommandValue(format));

        var unixTime = new DateTimeOffset(dt).ToUnixTimeSeconds();
        var totalSeconds = new Command(
            "totalSeconds",
            (Context context) => new NumberValue(unixTime),
            parameterTypes: [],
            description: "Gets the Unix epoch timestamp in seconds.",
            returnType: ToastType.Number
        );
        objCtx.SetValueDirect("totalSeconds", new CommandValue(totalSeconds));

        return new ObjectValue(objCtx, DateTimeType);
    }

    public static void Register(Toaster toast)
    {
        toast.RegisterType(DateTimeType);

        toast.RegisterConverter(
            new TypeConverter(
                ToastType.Null,
                DateTimeType,
                (context, val) => CreateDateTimeObject(context.Toaster, DateTime.Now)
            )
        );

        toast.RegisterConverter(
            new TypeConverter(
                ToastType.String,
                DateTimeType,
                (context, val) =>
                {
                    var str = (StringValue)val;
                    var dt = DateTime.Parse(str.Value);
                    return CreateDateTimeObject(context.Toaster, dt);
                }
            )
        );

        toast.RegisterConverter(
            new TypeConverter(
                ToastType.Number,
                DateTimeType,
                (context, val) =>
                {
                    var num = (NumberValue)val;
                    var seconds = (long)num.Value;
                    var dt = DateTimeOffset.FromUnixTimeSeconds(seconds).LocalDateTime;
                    return CreateDateTimeObject(context.Toaster, dt);
                }
            )
        );

        toast.RegisterConverter(
            new TypeConverter(
                DateTimeType,
                ToastType.String,
                (context, val) =>
                {
                    var obj = (ObjectValue)val;
                    var year = (int)((NumberValue)obj.Context.GetValue("year")).Value;
                    var month = (int)((NumberValue)obj.Context.GetValue("month")).Value;
                    var day = (int)((NumberValue)obj.Context.GetValue("day")).Value;
                    var hour = (int)((NumberValue)obj.Context.GetValue("hour")).Value;
                    var minute = (int)((NumberValue)obj.Context.GetValue("minute")).Value;
                    var second = (int)((NumberValue)obj.Context.GetValue("second")).Value;
                    var dt = new DateTime(year, month, day, hour, minute, second);
                    return new StringValue(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            )
        );
    }

    public void Load(Toaster toaster, Context callerContext)
    {
        Register(toaster);

        var nowCmd = Command.CreateFunction(
            "now",
            (Context c) => CreateDateTimeObject(toaster, DateTime.Now),
            description: "Gets the current local date and time.",
            returnType: DateTimeType
        );
        var utcNowCmd = Command.CreateFunction(
            "utcNow",
            (Context c) => CreateDateTimeObject(toaster, DateTime.UtcNow),
            description: "Gets the current UTC date and time.",
            returnType: DateTimeType
        );

        var dtCtx = new Context(toaster.GlobalContext);
        dtCtx.SetValueDirect("now", new CommandValue(nowCmd));
        dtCtx.SetValueDirect("utcNow", new CommandValue(utcNowCmd));

        var dtObj = new ObjectValue(dtCtx, new ToastType("datetimeModule"));
        callerContext.SetValueDirect("datetime", dtObj);
    }
}
