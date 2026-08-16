namespace Toast.SystemModules;

[ToastModule("utility", "IO, random, error and other utilities.")]
public class UtilityModule : IToastModule
{
    [ToastCommand("print", "Prints a value to standard output.")]
    public static ToastValue Print(ToastValue val)
    {
        Console.WriteLine(val);
        return NullValue.Instance;
    }

    [ToastCommand("input", "Reads a line of text from standard input.")]
    public static StringValue Input()
    {
        return new StringValue(Console.ReadLine() ?? "");
    }

    [ToastCommand("execute", "Executes a function with a list of arguments.")]
    public static ToastValue Execute(FunctionValue func, ListValue args)
    {
        return func.Execute(args.Elements);
    }

    [ToastCommand(
        "random",
        "Generates a pseudo-random integer between min (inclusive) and max (exclusive)."
    )]
    public static NumberValue Random(NumberValue min, NumberValue max)
    {
        return new NumberValue(System.Random.Shared.Next((int)min.Value, (int)max.Value));
    }

    [ToastCommand("randomChoice", "Returns a random element from a list.")]
    public static ToastValue RandomChoice(ListValue list)
    {
        if (list.Elements.Count == 0)
            return NullValue.Instance;
        return list.Elements[System.Random.Shared.Next(0, list.Elements.Count)];
    }

    [ToastCommand(
        "error",
        "Creates an Error object with error type, message, location, and optional cause."
    )]
    public static ErrorValue CreateError(ToastValue[] args)
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
    }
}
