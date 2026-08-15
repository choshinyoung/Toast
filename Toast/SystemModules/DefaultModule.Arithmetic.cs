namespace Toast.SystemModules;

public partial class DefaultModule
{
    public static readonly Command UnaryPlus = Command.CreateOperator(
        "+",
        (Context context, NumberValue val) => val,
        precedence: 9,
        isPrefix: true,
        description: "Unary plus operator.",
        returnType: ToastType.Number
    );
    public static readonly Command Addition = Command.CreateOperator(
        "+",
        ToastValue (Context context, ToastValue left, ToastValue right) =>
        {
            if (left is StringValue || right is StringValue)
            {
                return new StringValue(left.ToString() + right.ToString());
            }
            if (left is NumberValue ln && right is NumberValue rn)
            {
                return new NumberValue(ln.Value + rn.Value);
            }
            throw new ToastException(new TypeError("Cannot add non-number/non-string values."));
        },
        precedence: 7,
        description: "Addition operator for numbers or concatenation for strings."
    );
    public static readonly Command UnaryMinus = Command.CreateOperator(
        "-",
        (Context context, NumberValue val) => new NumberValue(-val.Value),
        precedence: 9,
        isPrefix: true,
        description: "Unary minus operator, negates a number.",
        returnType: ToastType.Number
    );
    public static readonly Command Subtraction = Command.CreateOperator(
        "-",
        (Context context, NumberValue left, NumberValue right) =>
            new NumberValue(left.Value - right.Value),
        precedence: 7,
        description: "Subtraction operator.",
        returnType: ToastType.Number
    );
    public static readonly Command Multiplication = Command.CreateOperator(
        "*",
        (Context context, NumberValue left, NumberValue right) =>
            new NumberValue(left.Value * right.Value),
        precedence: 8,
        description: "Multiplication operator.",
        returnType: ToastType.Number
    );
    public static readonly Command Division = Command.CreateOperator(
        "/",
        (Context context, NumberValue left, NumberValue right) =>
        {
            if (right.Value == 0)
            {
                throw new ToastException(RuntimeError.DivisionByZero());
            }
            return new NumberValue(left.Value / right.Value);
        },
        precedence: 8,
        description: "Division operator.",
        returnType: ToastType.Number
    );
    public static readonly Command Modulus = Command.CreateOperator(
        "%",
        (Context context, NumberValue left, NumberValue right) =>
        {
            if (right.Value == 0)
            {
                throw new ToastException(RuntimeError.DivisionByZero());
            }
            return new NumberValue(left.Value % right.Value);
        },
        precedence: 8,
        description: "Modulus operator.",
        returnType: ToastType.Number
    );
    public static readonly Command Exponentiation = Command.CreateOperator(
        "**",
        (Context context, NumberValue left, NumberValue right) =>
            new NumberValue(System.Math.Pow(left.Value, right.Value)),
        precedence: 9,
        isRightAssociative: true,
        description: "Exponentiation operator.",
        returnType: ToastType.Number
    );

    public static void RegisterArithmetic(Toaster toast)
    {
        toast.RegisterCommand(UnaryPlus);
        toast.RegisterCommand(Addition);
        toast.RegisterCommand(UnaryMinus);
        toast.RegisterCommand(Subtraction);
        toast.RegisterCommand(Multiplication);
        toast.RegisterCommand(Division);
        toast.RegisterCommand(Modulus);
        toast.RegisterCommand(Exponentiation);
    }
}
