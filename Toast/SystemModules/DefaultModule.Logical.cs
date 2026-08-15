namespace Toast.SystemModules;

public partial class DefaultModule
{
    public static readonly Command LogicalNot = Command.CreateOperator(
        "!",
        (Context context, BoolValue val) => new BoolValue(!val.Value),
        precedence: 9,
        isPrefix: true,
        description: "Logical NOT operator, inverts a boolean value.",
        returnType: ToastType.Boolean
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
            throw new ToastException(
                new TypeError("Right side of '&&' must evaluate to a boolean.")
            );
        },
        precedence: 2,
        description: "Logical AND operator with short-circuit evaluation.",
        returnType: ToastType.Boolean
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
            throw new ToastException(
                new TypeError("Right side of '||' must evaluate to a boolean.")
            );
        },
        precedence: 2,
        description: "Logical OR operator with short-circuit evaluation.",
        returnType: ToastType.Boolean
    );

    public static void RegisterLogical(Toaster toast)
    {
        toast.RegisterCommand(LogicalNot);
        toast.RegisterCommand(LogicalAnd);
        toast.RegisterCommand(LogicalOr);
    }
}
