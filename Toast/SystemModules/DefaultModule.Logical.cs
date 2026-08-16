namespace Toast.SystemModules;

public partial class DefaultModule
{
    [ToastCommand(
        "!",
        Precedence = 9,
        IsPrefix = true,
        Description = "Logical NOT operator, inverts a boolean value."
    )]
    public static BoolValue LogicalNot(BoolValue val) => new(!val.Value);

    [ToastCommand(
        "&&",
        Precedence = 2,
        Description = "Logical AND operator with short-circuit evaluation."
    )]
    public static ToastValue LogicalAnd(Context context, BoolValue left, AstNodeValue right)
    {
        if (!left.Value)
            return new BoolValue(false);
        var res = context.Toaster.Evaluate(right.Node, context);
        if (res is BoolValue rb)
            return rb;
        throw new ToastException(new TypeError("Right side of '&&' must evaluate to a boolean."));
    }

    [ToastCommand(
        "||",
        Precedence = 2,
        Description = "Logical OR operator with short-circuit evaluation."
    )]
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
