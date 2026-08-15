namespace Toast.SystemModules;

public class StringModule : IToastModule
{
    public string Name => "string";
    public string Description => "String manipulation functions and extension methods.";

    public static readonly Command Split = Command.CreateFunction(
        "split",
        (Context context, StringValue str, StringValue separator) =>
        {
            var parts = str.Value.Split([separator.Value], StringSplitOptions.None);
            return new ListValue([.. parts.Select(x => (ToastValue)new StringValue(x))]);
        },
        description: "Splits a string into a list of substrings based on a separator.",
        returnType: ToastType.List
    );
    public static readonly Command Reverse = Command.CreateFunction(
        "reverse",
        (Context context, StringValue str) =>
        {
            var chars = str.Value.ToCharArray();
            Array.Reverse(chars);
            return new StringValue(new string(chars));
        },
        description: "Reverses a string.",
        returnType: ToastType.String
    );
    public static readonly Command StartsWith = Command.CreateFunction(
        "startsWith",
        (Context context, StringValue str, StringValue prefix) =>
        {
            return new BoolValue(str.Value.StartsWith(prefix.Value));
        },
        description: "Determines whether a string starts with the specified prefix.",
        returnType: ToastType.Boolean
    );
    public static readonly Command EndsWith = Command.CreateFunction(
        "endsWith",
        (Context context, StringValue str, StringValue suffix) =>
        {
            return new BoolValue(str.Value.EndsWith(suffix.Value));
        },
        description: "Determines whether a string ends with the specified suffix.",
        returnType: ToastType.Boolean
    );
    public static readonly Command Contains = Command.CreateFunction(
        "contains",
        (Context context, StringValue str, StringValue substring) =>
        {
            return new BoolValue(str.Value.Contains(substring.Value));
        },
        description: "Determines whether a string contains the specified substring.",
        returnType: ToastType.Boolean
    );
    public static readonly Command Trim = Command.CreateFunction(
        "trim",
        (Context context, StringValue str) =>
        {
            return new StringValue(str.Value.Trim());
        },
        description: "Removes leading and trailing whitespace characters from a string.",
        returnType: ToastType.String
    );
    public static readonly Command Substring = Command.CreateFunction(
        "substring",
        (Context context, StringValue str, NumberValue startIndex, NumberValue length) =>
        {
            return new StringValue(str.Value.Substring((int)startIndex.Value, (int)length.Value));
        },
        description: "Retrieves a substring starting at a specified character position.",
        returnType: ToastType.String
    );
    public static readonly Command Join = Command.CreateFunction(
        "join",
        (Context context, StringValue separator, ListValue list) =>
        {
            var items = list.Elements.Select(x => x.ToString());
            return new StringValue(string.Join(separator.Value, items));
        },
        description: "Concatenates members of a list using the specified separator between each element.",
        returnType: ToastType.String
    );
    public static readonly Command Replace = Command.CreateFunction(
        "replace",
        (Context context, StringValue str, StringValue oldValue, StringValue newValue) =>
        {
            return new StringValue(str.Value.Replace(oldValue.Value, newValue.Value));
        },
        description: "Replaces all occurrences of a specified string with another string.",
        returnType: ToastType.String
    );
    public static readonly Command ToUpper = Command.CreateFunction(
        "toUpper",
        (Context context, StringValue str) =>
        {
            return new StringValue(str.Value.ToUpper());
        },
        description: "Returns a copy of the string converted to uppercase.",
        returnType: ToastType.String
    );
    public static readonly Command ToLower = Command.CreateFunction(
        "toLower",
        (Context context, StringValue str) =>
        {
            return new StringValue(str.Value.ToLower());
        },
        description: "Returns a copy of the string converted to lowercase.",
        returnType: ToastType.String
    );
    public static readonly Command Length = Command.CreateFunction(
        "length",
        (Context context, StringValue str) =>
        {
            return new NumberValue(str.Value.Length);
        },
        description: "Gets the number of characters in the string.",
        returnType: ToastType.Number
    );
    private static readonly Command StringIndex = Command.CreateFunction(
        "#",
        (Context context, StringValue str, NumberValue index) =>
        {
            if (context.Toaster.Executor.SuppressDereference)
            {
                throw new ToastException(
                    new TypeError(
                        "Strings are immutable and cannot be modified via index assignment."
                    )
                );
            }

            int idx = (int)index.Value;
            if (idx < 0 || idx >= str.Value.Length)
            {
                throw new ToastException(IndexError.OutOfRange(idx, str.Value.Length, "string"));
            }

            return new StringValue(str.Value[idx].ToString());
        },
        description: "Gets the character at the specified index in the string.",
        returnType: ToastType.String
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

    public void Load(Toaster toaster, Context callerContext)
    {
        Register(toaster);
    }
}
