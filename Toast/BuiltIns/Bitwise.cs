namespace Toast.BuiltIns;

public static class Bitwise
{
    public static readonly Command BitwiseNot = Command.CreateOperator(
        "~",
        (Context context, int val) => ~val,
        precedence: 9,
        isPrefix: true
    );

    public static readonly Command BitwiseAnd = Command.CreateOperator(
        "&",
        (Context ctx, int x, int y) => x & y,
        precedence: 6
    );

    public static readonly Command BitwiseOr = Command.CreateOperator(
        "|",
        (Context ctx, int x, int y) => x | y,
        precedence: 6
    );

    public static readonly Command BitwiseXor = Command.CreateOperator(
        "^",
        (Context ctx, int x, int y) => x ^ y,
        precedence: 6
    );

    public static readonly Command LeftShift = Command.CreateOperator(
        "<<",
        (Context ctx, int x, int y) => x << y,
        precedence: 8
    );

    public static readonly Command RightShift = Command.CreateOperator(
        ">>",
        (Context ctx, int x, int y) => x >> y,
        precedence: 8
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(BitwiseNot);
        toast.RegisterCommand(BitwiseAnd);
        toast.RegisterCommand(BitwiseOr);
        toast.RegisterCommand(BitwiseXor);
        toast.RegisterCommand(LeftShift);
        toast.RegisterCommand(RightShift);
    }
}
