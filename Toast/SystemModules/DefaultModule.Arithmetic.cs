namespace Toast.SystemModules;

public partial class DefaultModule
{
    [ToastCommand("+", Precedence = 9, IsPrefix = true, Description = "Unary plus operator.")]
    public static NumberValue UnaryPlus(NumberValue val) => val;

    [ToastCommand(
        "+",
        Precedence = 7,
        Description = "Addition operator for numbers or concatenation for strings."
    )]
    public static ToastValue Addition(ToastValue left, ToastValue right)
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
    }

    [ToastCommand(
        "-",
        Precedence = 9,
        IsPrefix = true,
        Description = "Unary minus operator, negates a number."
    )]
    public static NumberValue UnaryMinus(NumberValue val) => new(-val.Value);

    [ToastCommand("-", Precedence = 7, Description = "Subtraction operator.")]
    public static NumberValue Subtraction(NumberValue left, NumberValue right) =>
        new(left.Value - right.Value);

    [ToastCommand("*", Precedence = 8, Description = "Multiplication operator.")]
    public static NumberValue Multiplication(NumberValue left, NumberValue right) =>
        new(left.Value * right.Value);

    [ToastCommand("/", Precedence = 8, Description = "Division operator.")]
    public static NumberValue Division(NumberValue left, NumberValue right)
    {
        if (right.Value == 0)
        {
            throw new ToastException(RuntimeError.DivisionByZero());
        }
        return new NumberValue(left.Value / right.Value);
    }

    [ToastCommand("%", Precedence = 8, Description = "Modulus operator.")]
    public static NumberValue Modulus(NumberValue left, NumberValue right)
    {
        if (right.Value == 0)
        {
            throw new ToastException(RuntimeError.DivisionByZero());
        }
        return new NumberValue(left.Value % right.Value);
    }

    [ToastCommand(
        "**",
        Precedence = 9,
        IsRightAssociative = true,
        Description = "Exponentiation operator."
    )]
    public static NumberValue Exponentiation(NumberValue left, NumberValue right) =>
        new(System.Math.Pow(left.Value, right.Value));
}
