namespace Toast.BuiltIns;

public static class Logical
{
    public static readonly Command LogicalNot = Command.CreateOperator(
        "!",
        (Context context, bool val) => !val,
        precedence: 9,
        isPrefix: true
    );

    public static readonly Command LogicalAnd = Command.CreateOperator(
        "&&",
        (Context context, bool left, Node right) =>
        {
            if (!left)
                return false;
            return (bool)context.Toaster.Evaluate(right, context)!;
        },
        precedence: 2
    );

    public static readonly Command LogicalOr = Command.CreateOperator(
        "||",
        (Context context, bool left, Node right) =>
        {
            if (left)
                return true;
            return (bool)context.Toaster.Evaluate(right, context)!;
        },
        precedence: 2
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(LogicalNot);
        toast.RegisterCommand(LogicalAnd);
        toast.RegisterCommand(LogicalOr);
    }
}
