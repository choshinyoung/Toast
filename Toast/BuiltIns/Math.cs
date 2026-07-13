namespace Toast.BuiltIns;

public static class Math
{
    public static readonly Command UnaryPlus = Command.CreateOperator(
        "+",
        (Context context, NumberValue val) => val,
        precedence: 9,
        isPrefix: true
    );

    public static readonly Command Addition = Command.CreateOperator(
        "+",
        ToastObject (Context context, ToastObject left, ToastObject right) =>
        {
            if (left is StringValue || right is StringValue)
            {
                return new StringValue(left.ToString() + right.ToString());
            }
            if (left is NumberValue ln && right is NumberValue rn)
            {
                return new NumberValue(ln.Value + rn.Value);
            }
            throw new InvalidOperationException("Cannot add non-number/non-string values.");
        },
        precedence: 7
    );

    public static readonly Command UnaryMinus = Command.CreateOperator(
        "-",
        (Context context, NumberValue val) => new NumberValue(-val.Value),
        precedence: 9,
        isPrefix: true
    );

    public static readonly Command Subtraction = Command.CreateOperator(
        "-",
        (Context context, NumberValue left, NumberValue right) =>
            new NumberValue(left.Value - right.Value),
        precedence: 7
    );

    public static readonly Command Multiplication = Command.CreateOperator(
        "*",
        (Context context, NumberValue left, NumberValue right) =>
            new NumberValue(left.Value * right.Value),
        precedence: 8
    );

    public static readonly Command Division = Command.CreateOperator(
        "/",
        (Context context, NumberValue left, NumberValue right) =>
            new NumberValue(left.Value / right.Value),
        precedence: 8
    );

    public static readonly Command Modulus = Command.CreateOperator(
        "%",
        (Context context, NumberValue left, NumberValue right) =>
            new NumberValue(left.Value % right.Value),
        precedence: 8
    );

    public static readonly Command Exponentiation = Command.CreateOperator(
        "**",
        (Context ctx, NumberValue x, NumberValue y) =>
            new NumberValue(System.Math.Pow(x.Value, y.Value)),
        precedence: 8
    );

    public static readonly Command FloorDivision = Command.CreateFunction(
        "floorDiv",
        (Context ctx, NumberValue x, NumberValue y) =>
            new NumberValue(System.Math.Floor(x.Value / y.Value)),
        precedence: 8,
        isInfix: true
    );

    public static readonly Command Sqrt = Command.CreateFunction(
        "sqrt",
        (Context context, NumberValue val) => new NumberValue(System.Math.Sqrt(val.Value))
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(UnaryPlus);
        toast.RegisterCommand(Addition);
        toast.RegisterCommand(UnaryMinus);
        toast.RegisterCommand(Subtraction);
        toast.RegisterCommand(Multiplication);
        toast.RegisterCommand(Division);
        toast.RegisterCommand(Modulus);
        toast.RegisterCommand(Exponentiation);
        toast.RegisterCommand(FloorDivision);
        toast.RegisterCommand(Sqrt);
    }
}
