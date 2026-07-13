namespace Toast.BuiltIns;

public static class Logical
{
    public static readonly Command LogicalNot = Command.CreateOperator(
        "!",
        (Context context, BoolValue val) => new BoolValue(!val.Value),
        precedence: 9,
        isPrefix: true
    );

    public static readonly Command LogicalAnd = Command.CreateOperator(
        "&&",
        (Context context, BoolValue left, AstNodeValue right) =>
        {
            if (!left.Value)
                return new BoolValue(false);
            var res = context.Toaster.Evaluate(right.Node, context);
            if (res is BoolValue rb)
                return rb;
            throw new InvalidOperationException("Right side of '&&' must evaluate to a boolean.");
        },
        precedence: 2
    );

    public static readonly Command LogicalOr = Command.CreateOperator(
        "||",
        (Context context, BoolValue left, AstNodeValue right) =>
        {
            if (left.Value)
                return new BoolValue(true);
            var res = context.Toaster.Evaluate(right.Node, context);
            if (res is BoolValue rb)
                return rb;
            throw new InvalidOperationException("Right side of '||' must evaluate to a boolean.");
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
