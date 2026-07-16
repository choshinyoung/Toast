namespace Toast.BuiltIns;

public static class String
{
    public static readonly Command Split = Command.CreateFunction(
        "split",
        (Context context, StringValue str, StringValue separator) =>
        {
            var parts = str.Value.Split([separator.Value], StringSplitOptions.None);
            return new ListValue([.. parts.Select(x => (ToastValue)new StringValue(x))]);
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

    public static readonly Command Length = Command.CreateFunction(
        "length",
        (Context context, StringValue str) =>
        {
            return new NumberValue(str.Value.Length);
        }
    );

    private static readonly Command StringIndex = Command.CreateFunction(
        "#",
        (Context context, StringValue str, NumberValue index) =>
        {
            if (context.Toaster.Executor.SuppressDereference)
            {
                throw new InvalidOperationException(
                    "Strings are immutable and cannot be modified via index assignment."
                );
            }

            int idx = (int)index.Value;
            if (idx < 0 || idx >= str.Value.Length)
            {
                throw new IndexOutOfRangeException(
                    $"Index {idx} is out of range for string of length {str.Value.Length}."
                );
            }

            return new StringValue(str.Value[idx].ToString());
        }
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterTypeMember(ToastType.String, "#", new CommandValue(StringIndex));
        toast.RegisterTypeMember(ToastType.String, "substring", new CommandValue(Substring));
        toast.RegisterTypeMember(ToastType.String, "contains", new CommandValue(Contains));
        toast.RegisterTypeMember(ToastType.String, "length", new CommandValue(Length));
        toast.RegisterTypeMember(ToastType.String, "split", new CommandValue(Split));
        toast.RegisterTypeMember(ToastType.String, "reverse", new CommandValue(Reverse));
        toast.RegisterTypeMember(ToastType.String, "startsWith", new CommandValue(StartsWith));
        toast.RegisterTypeMember(ToastType.String, "endsWith", new CommandValue(EndsWith));
        toast.RegisterTypeMember(ToastType.String, "trim", new CommandValue(Trim));
        toast.RegisterTypeMember(ToastType.String, "join", new CommandValue(Join));
        toast.RegisterTypeMember(ToastType.String, "replace", new CommandValue(Replace));
        toast.RegisterTypeMember(ToastType.String, "toUpper", new CommandValue(ToUpper));
        toast.RegisterTypeMember(ToastType.String, "toLower", new CommandValue(ToLower));
    }
}
