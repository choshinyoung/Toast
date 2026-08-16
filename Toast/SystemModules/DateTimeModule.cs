namespace Toast.SystemModules;

[ToastModule("datetime", Description = "Date and time manipulation module")]
public class DateTimeModule : IToastModule
{
    [ToastType("datetime")]
    public static class DateTimeTypeClass
    {
        [ToastCommand(
            "addDays",
            Description = "Adds the specified number of days to the datetime value."
        )]
        public static ObjectValue AddDays(Context context, ObjectValue self, NumberValue days)
        {
            var dt = ToDateTime(self);
            return CreateDateTimeObject(context.Toaster, dt.AddDays(days.Value));
        }

        [ToastCommand(
            "format",
            Description = "Formats the datetime value using a standard format string."
        )]
        public static StringValue Format(ObjectValue self, StringValue fmt)
        {
            var dt = ToDateTime(self);
            return new StringValue(dt.ToString(fmt.Value));
        }

        [ToastCommand("totalSeconds", Description = "Gets the Unix epoch timestamp in seconds.")]
        public static NumberValue TotalSeconds(ObjectValue self)
        {
            var dt = ToDateTime(self);
            return new NumberValue(new DateTimeOffset(dt).ToUnixTimeSeconds());
        }

        internal static DateTime ToDateTime(ObjectValue obj)
        {
            var year = (int)((NumberValue)obj.Context.GetValue("year")).Value;
            var month = (int)((NumberValue)obj.Context.GetValue("month")).Value;
            var day = (int)((NumberValue)obj.Context.GetValue("day")).Value;
            var hour = (int)((NumberValue)obj.Context.GetValue("hour")).Value;
            var minute = (int)((NumberValue)obj.Context.GetValue("minute")).Value;
            var second = (int)((NumberValue)obj.Context.GetValue("second")).Value;
            return new DateTime(year, month, day, hour, minute, second);
        }
    }

    [ToastObject("datetime")]
    public static class DateTimeNamespace
    {
        [ToastCommand("now", Description = "Gets the current local date and time.")]
        public static ObjectValue Now(Context context)
        {
            return CreateDateTimeObject(context.Toaster, DateTime.Now);
        }

        [ToastCommand("utcNow", Description = "Gets the current UTC date and time.")]
        public static ObjectValue UtcNow(Context context)
        {
            return CreateDateTimeObject(context.Toaster, DateTime.UtcNow);
        }
    }

    [ToastConverter(SourceTypeName = "string", TargetTypeName = "datetime")]
    public static ObjectValue FromString(Context context, StringValue val)
    {
        var dt = DateTime.Parse(val.Value);
        return CreateDateTimeObject(context.Toaster, dt);
    }

    [ToastConverter(SourceTypeName = "number", TargetTypeName = "datetime")]
    public static ObjectValue FromNumber(Context context, NumberValue val)
    {
        var seconds = (long)val.Value;
        var dt = DateTimeOffset.FromUnixTimeSeconds(seconds).LocalDateTime;
        return CreateDateTimeObject(context.Toaster, dt);
    }

    [ToastConverter(SourceTypeName = "datetime", TargetTypeName = "string")]
    public static StringValue ToString(ObjectValue obj)
    {
        var dt = DateTimeTypeClass.ToDateTime(obj);
        return new StringValue(dt.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    public static ObjectValue CreateDateTimeObject(Toaster toaster, DateTime dt)
    {
        var dtType = ToastType.FromName("datetime");
        var objCtx = new Context(toaster.GlobalContext);
        objCtx.SetValueDirect("year", new NumberValue(dt.Year));
        objCtx.SetValueDirect("month", new NumberValue(dt.Month));
        objCtx.SetValueDirect("day", new NumberValue(dt.Day));
        objCtx.SetValueDirect("hour", new NumberValue(dt.Hour));
        objCtx.SetValueDirect("minute", new NumberValue(dt.Minute));
        objCtx.SetValueDirect("second", new NumberValue(dt.Second));

        var obj = new ObjectValue(objCtx, dtType);
        objCtx.Owner = obj;
        return obj;
    }
}
