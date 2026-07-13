namespace Toast.BuiltIns;

public static class String
{
    public static readonly Command Split = Command.CreateFunction(
        "split",
        (Context context, StringValue str, StringValue separator) =>
        {
            var parts = str.Value.Split([separator.Value], StringSplitOptions.None);
            return new ListValue(parts.Select(x => (ToastObject)new StringValue(x)).ToList());
        }
    );

    public static readonly Command Reverse = Command.CreateFunction(
        "reverse",
        (Context context, StringValue str) =>
        {
            var chars = str.Value.ToCharArray();
            Array.Reverse(chars);
            return new StringValue(new string(chars));
        }
    );

    public static readonly Command StartsWith = Command.CreateFunction(
        "startsWith",
        (Context context, StringValue str, StringValue prefix) =>
        {
            return new BoolValue(str.Value.StartsWith(prefix.Value));
        }
    );

    public static readonly Command EndsWith = Command.CreateFunction(
        "endsWith",
        (Context context, StringValue str, StringValue suffix) =>
        {
            return new BoolValue(str.Value.EndsWith(suffix.Value));
        }
    );

    public static readonly Command Contains = Command.CreateFunction(
        "contains",
        (Context context, StringValue str, StringValue substring) =>
        {
            return new BoolValue(str.Value.Contains(substring.Value));
        }
    );

    public static readonly Command Trim = Command.CreateFunction(
        "trim",
        (Context context, StringValue str) =>
        {
            return new StringValue(str.Value.Trim());
        }
    );

    public static readonly Command Substring = Command.CreateFunction(
        "substring",
        (Context context, StringValue str, NumberValue startIndex, NumberValue length) =>
        {
            return new StringValue(str.Value.Substring((int)startIndex.Value, (int)length.Value));
        }
    );

    public static readonly Command Join = Command.CreateFunction(
        "join",
        (Context context, StringValue separator, ListValue list) =>
        {
            var items = list.Elements.Select(x => x.ToString());
            return new StringValue(string.Join(separator.Value, items));
        }
    );

    public static readonly Command Replace = Command.CreateFunction(
        "replace",
        (Context context, StringValue str, StringValue oldValue, StringValue newValue) =>
        {
            return new StringValue(str.Value.Replace(oldValue.Value, newValue.Value));
        }
    );

    public static readonly Command ToUpper = Command.CreateFunction(
        "toUpper",
        (Context context, StringValue str) =>
        {
            return new StringValue(str.Value.ToUpper());
        }
    );

    public static readonly Command ToLower = Command.CreateFunction(
        "toLower",
        (Context context, StringValue str) =>
        {
            return new StringValue(str.Value.ToLower());
        }
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(Split);
        toast.RegisterCommand(Reverse);
        toast.RegisterCommand(StartsWith);
        toast.RegisterCommand(EndsWith);
        toast.RegisterCommand(Contains);
        toast.RegisterCommand(Trim);
        toast.RegisterCommand(Substring);
        toast.RegisterCommand(Join);
        toast.RegisterCommand(Replace);
        toast.RegisterCommand(ToUpper);
        toast.RegisterCommand(ToLower);
    }
}
