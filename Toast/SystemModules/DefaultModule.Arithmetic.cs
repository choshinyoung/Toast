namespace Toast.SystemModules;

public partial class DefaultModule
{
    [ToastCommand("+", "Unary plus operator.", Precedence = 9, IsPrefix = true)]
    public static NumberValue UnaryPlus(NumberValue val) => val;

    [ToastCommand("+", "Addition operator.", Precedence = 7)]
    public static NumberValue Addition(NumberValue left, NumberValue right) =>
        new(left.Value + right.Value);

    [ToastCommand("-", "Unary minus operator, negates a number.", Precedence = 9, IsPrefix = true)]
    public static NumberValue UnaryMinus(NumberValue val) => new(-val.Value);

    [ToastCommand("-", "Subtraction operator.", Precedence = 7)]
    public static NumberValue Subtraction(NumberValue left, NumberValue right) =>
        new(left.Value - right.Value);

    [ToastCommand("*", "Multiplication operator.", Precedence = 8)]
    public static NumberValue Multiplication(NumberValue left, NumberValue right) =>
        new(left.Value * right.Value);

    [ToastCommand("/", "Division operator.", Precedence = 8)]
    public static NumberValue Division(NumberValue left, NumberValue right)
    {
        if (right.Value == 0)
        {
            throw new ToastException(RuntimeError.DivisionByZero());
        }
        return new NumberValue(left.Value / right.Value);
    }

    [ToastCommand("%", "Modulus operator.", Precedence = 8)]
    public static NumberValue Modulus(NumberValue left, NumberValue right)
    {
        if (right.Value == 0)
        {
            throw new ToastException(RuntimeError.DivisionByZero());
        }
        return new NumberValue(left.Value % right.Value);
    }

    [ToastCommand("**", "Exponentiation operator.", Precedence = 9, IsRightAssociative = true)]
    public static NumberValue Exponentiation(NumberValue left, NumberValue right) =>
        new(System.Math.Pow(left.Value, right.Value));
}
