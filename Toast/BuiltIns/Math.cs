namespace Toast.BuiltIns;

public static class Math
{
    public static readonly Command UnaryPlus = Command.CreateOperator(
        "+",
        (Context context, double val) => val,
        precedence: 9,
        isPrefix: true
    );

    public static readonly Command Addition = Command.CreateOperator(
        "+",
        object? (Context context, object? left, object? right) =>
        {
            if (left is string || right is string)
            {
                return left?.ToString() + right?.ToString();
            }
            if (left is double || right is double || left is float || right is float)
            {
                return Convert.ToDouble(left) + Convert.ToDouble(right);
            }
            return Convert.ToInt32(left) + Convert.ToInt32(right);
        },
        precedence: 7
    );

    public static readonly Command UnaryMinus = Command.CreateOperator(
        "-",
        (Context context, double val) => -val,
        precedence: 9,
        isPrefix: true
    );

    public static readonly Command Subtraction = Command.CreateOperator(
        "-",
        (Context context, double left, double right) => left - right,
        precedence: 7
    );

    public static readonly Command Multiplication = Command.CreateOperator(
        "*",
        object? (Context context, object? left, object? right) =>
        {
            if (left is double || right is double || left is float || right is float)
            {
                return Convert.ToDouble(left) * Convert.ToDouble(right);
            }
            return Convert.ToInt32(left) * Convert.ToInt32(right);
        },
        precedence: 8
    );

    public static readonly Command Division = Command.CreateOperator(
        "/",
        (Context context, double left, double right) => left / right,
        precedence: 8
    );

    public static readonly Command Modulus = Command.CreateOperator(
        "%",
        (Context context, double left, double right) => left % right,
        precedence: 8
    );

    public static readonly Command Exponentiation = Command.CreateOperator(
        "**",
        (Context ctx, double x, double y) => System.Math.Pow(x, y),
        precedence: 8
    );

    public static readonly Command FloorDivision = Command.CreateFunction(
        "floorDiv",
        (Context ctx, int x, int y) => x / y,
        precedence: 8,
        isInfix: true
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
    }
}
