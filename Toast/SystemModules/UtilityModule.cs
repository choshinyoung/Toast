namespace Toast.SystemModules;

public class UtilityModule : IToastModule
{
    public string Name => "utility";
    public string Description => "IO, random, and other utilities.";

    public static readonly Command Print = Command.CreateFunction(
        "print",
        (Context context, ToastValue val) =>
        {
            Console.WriteLine(val);
            return NullValue.Instance;
        },
        description: "Prints a value to standard output.",
        returnType: ToastType.Null
    );

    public static readonly Command Input = Command.CreateFunction(
        "input",
        (Context context) => new StringValue(Console.ReadLine() ?? ""),
        description: "Reads a line of text from standard input.",
        returnType: ToastType.String
    );

    public static readonly Command Execute = Command.CreateFunction(
        "execute",
        (Context context, FunctionValue func, ListValue args) =>
        {
            return func.Execute(args.Elements);
        },
        description: "Executes a function with a list of arguments."
    );

    public static readonly Command Random = Command.CreateFunction(
        "random",
        (Context context, NumberValue min, NumberValue max) =>
        {
            return new NumberValue(new Random().Next((int)min.Value, (int)max.Value));
        },
        description: "Generates a pseudo-random integer between min (inclusive) and max (exclusive).",
        returnType: ToastType.Number
    );

    public static readonly Command RandomChoice = Command.CreateFunction(
        "randomChoice",
        (Context context, ListValue list) =>
        {
            if (list.Elements.Count == 0)
                return NullValue.Instance;
            return list.Elements[new Random().Next(0, list.Elements.Count)];
        },
        description: "Returns a random element from a list."
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(Print);
        toast.RegisterCommand(Input);
        toast.RegisterCommand(Execute);
        toast.RegisterCommand(Random);
        toast.RegisterCommand(RandomChoice);
    }

    public void Load(Toaster toaster, Context callerContext)
    {
        Register(toaster);
    }
}
