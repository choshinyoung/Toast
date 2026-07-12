namespace Toast.BuiltIns;

public static class String
{
    public static readonly Command Split = Command.CreateFunction(
        "split",
        (Context context, string str, string separator) =>
        {
            return str.Split(separator).ToList();
        }
    );

    public static readonly Command Reverse = Command.CreateFunction(
        "reverse",
        (Context context, string str) =>
        {
            var chars = str.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }
    );

    public static readonly Command StartsWith = Command.CreateFunction(
        "startsWith",
        (Context context, string str, string prefix) =>
        {
            return str.StartsWith(prefix);
        }
    );

    public static readonly Command EndsWith = Command.CreateFunction(
        "endsWith",
        (Context context, string str, string suffix) =>
        {
            return str.EndsWith(suffix);
        }
    );

    public static readonly Command Contains = Command.CreateFunction(
        "contains",
        (Context context, string str, string substring) =>
        {
            return str.Contains(substring);
        }
    );

    public static readonly Command Trim = Command.CreateFunction(
        "trim",
        (Context context, string str) =>
        {
            return str.Trim();
        }
    );

    public static readonly Command Substring = Command.CreateFunction(
        "substring",
        (Context context, string str, int startIndex, int length) =>
        {
            return str.Substring(startIndex, length);
        }
    );

    public static readonly Command Join = Command.CreateFunction(
        "join",
        (Context context, string separator, System.Collections.IEnumerable list) =>
        {
            var items = list.Cast<object?>().Select(x => x?.ToString() ?? "");
            return string.Join(separator, items);
        }
    );

    public static readonly Command Replace = Command.CreateFunction(
        "replace",
        (Context context, string str, string oldValue, string newValue) =>
        {
            return str.Replace(oldValue, newValue);
        }
    );

    public static readonly Command ToUpper = Command.CreateFunction(
        "toUpper",
        (Context context, string str) =>
        {
            return str.ToUpper();
        }
    );

    public static readonly Command ToLower = Command.CreateFunction(
        "toLower",
        (Context context, string str) =>
        {
            return str.ToLower();
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
