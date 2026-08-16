namespace Toast.SystemModules;

[ToastModule("string", "String functions and extension members.")]
public class StringModule : IToastModule
{
    [ToastType("string", "String type")]
    public static class StringType
    {
        [ToastCommand("split", "Splits a string into a list of substrings based on a separator.")]
        public static ListValue Split(StringValue str, StringValue separator)
        {
            var parts = str.Value.Split([separator.Value], StringSplitOptions.None);
            return new ListValue([.. parts.Select(x => (ToastValue)new StringValue(x))]);
        }

        [ToastCommand("reverse", "Reverses a string.")]
        public static StringValue Reverse(StringValue str)
        {
            var chars = str.Value.ToCharArray();
            Array.Reverse(chars);
            return new StringValue(new string(chars));
        }

        [ToastCommand(
            "startsWith",
            "Determines whether a string starts with the specified prefix."
        )]
        public static BoolValue StartsWith(StringValue str, StringValue prefix)
        {
            return new BoolValue(str.Value.StartsWith(prefix.Value));
        }

        [ToastCommand("endsWith", "Determines whether a string ends with the specified suffix.")]
        public static BoolValue EndsWith(StringValue str, StringValue suffix)
        {
            return new BoolValue(str.Value.EndsWith(suffix.Value));
        }

        [ToastCommand("contains", "Determines whether a string contains the specified substring.")]
        public static BoolValue Contains(StringValue str, StringValue substring)
        {
            return new BoolValue(str.Value.Contains(substring.Value));
        }

        [ToastCommand("trim", "Removes leading and trailing whitespace characters from a string.")]
        public static StringValue Trim(StringValue str)
        {
            return new StringValue(str.Value.Trim());
        }

        [ToastCommand(
            "substring",
            "Retrieves a substring starting at a specified character position."
        )]
        public static StringValue Substring(
            StringValue str,
            NumberValue startIndex,
            NumberValue length
        )
        {
            return new StringValue(str.Value.Substring((int)startIndex.Value, (int)length.Value));
        }

        [ToastCommand(
            "join",
            "Concatenates members of a list using the specified separator between each element."
        )]
        public static StringValue Join(StringValue separator, ListValue list)
        {
            var items = list.Elements.Select(x => x.ToString());
            return new StringValue(string.Join(separator.Value, items));
        }

        [ToastCommand(
            "replace",
            "Replaces all occurrences of a specified string with another string."
        )]
        public static StringValue Replace(
            StringValue str,
            StringValue oldValue,
            StringValue newValue
        )
        {
            return new StringValue(str.Value.Replace(oldValue.Value, newValue.Value));
        }

        [ToastCommand("toUpper", "Converts all characters in a string to uppercase.")]
        public static StringValue ToUpper(StringValue str)
        {
            return new StringValue(str.Value.ToUpper());
        }

        [ToastCommand("toLower", "Converts all characters in a string to lowercase.")]
        public static StringValue ToLower(StringValue str)
        {
            return new StringValue(str.Value.ToLower());
        }

        [ToastCommand("length", "Gets the length of a string.")]
        public static NumberValue Length(StringValue str)
        {
            return new NumberValue(str.Value.Length);
        }

        [ToastCommand("#", "Gets a character from a string at the specified zero-based index.")]
        public static StringValue Index(StringValue str, NumberValue index)
        {
            int idx = (int)index.Value;
            if (idx < 0 || idx >= str.Value.Length)
            {
                throw new ToastException(IndexError.OutOfRange(idx, str.Value.Length, "string"));
            }
            return new StringValue(str.Value[idx].ToString());
        }
    }
}
