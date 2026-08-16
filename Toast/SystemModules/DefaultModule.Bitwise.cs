namespace Toast.SystemModules;

public partial class DefaultModule
{
    [ToastCommand(
        "~",
        Precedence = 9,
        IsPrefix = true,
        Description = "Bitwise NOT operator, inverts all bits of an integer."
    )]
    public static NumberValue BitwiseNot(NumberValue val) => new(~(int)val.Value);

    [ToastCommand("&", Precedence = 6, Description = "Bitwise AND operator.")]
    public static NumberValue BitwiseAnd(NumberValue x, NumberValue y) =>
        new((int)x.Value & (int)y.Value);

    [ToastCommand("|", Precedence = 6, Description = "Bitwise OR operator.")]
    public static NumberValue BitwiseOr(NumberValue x, NumberValue y) =>
        new((int)x.Value | (int)y.Value);

    [ToastCommand("^", Precedence = 6, Description = "Bitwise XOR operator.")]
    public static NumberValue BitwiseXor(NumberValue x, NumberValue y) =>
        new((int)x.Value ^ (int)y.Value);

    [ToastCommand("<<", Precedence = 8, Description = "Bitwise left shift operator.")]
    public static NumberValue LeftShift(NumberValue x, NumberValue y) =>
        new((int)x.Value << (int)y.Value);

    [ToastCommand(">>", Precedence = 8, Description = "Bitwise right shift operator.")]
    public static NumberValue RightShift(NumberValue x, NumberValue y) =>
        new((int)x.Value >> (int)y.Value);
}
