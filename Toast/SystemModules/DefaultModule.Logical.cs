namespace Toast.SystemModules;

public partial class DefaultModule
{
    [ToastCommand(
        "!",
        "Logical NOT operator, inverts a boolean value.",
        Precedence = 9,
        IsPrefix = true
    )]
    public static BoolValue LogicalNot(BoolValue val) => new(!val.Value);

    [ToastCommand("&&", "Logical AND operator with short-circuit evaluation.", Precedence = 2)]
    public static ToastValue LogicalAnd(Context context, BoolValue left, AstNodeValue right)
    {
        if (!left.Value)
            return new BoolValue(false);
        var res = context.Toaster.Evaluate(right.Node, context);
        if (res is BoolValue rb)
            return rb;
        throw new ToastException(new TypeError("Right side of '&&' must evaluate to a boolean."));
    }

    [ToastCommand("||", "Logical OR operator with short-circuit evaluation.", Precedence = 2)]
    public static ToastValue LogicalOr(Context context, BoolValue left, AstNodeValue right)
    {
        if (left.Value)
            return new BoolValue(true);
        var res = context.Toaster.Evaluate(right.Node, context);
        if (res is BoolValue rb)
            return rb;
        throw new ToastException(new TypeError("Right side of '||' must evaluate to a boolean."));
    }
}
