namespace Toast.SystemModules;

public partial class DefaultModule
{
    [ToastCommand(
        "~",
        "Bitwise NOT operator, inverts all bits of an integer.",
        Precedence = 9,
        IsPrefix = true
    )]
    public static NumberValue BitwiseNot(NumberValue val) => new(~(int)val.Value);

    [ToastCommand("&", "Bitwise AND operator.", Precedence = 6)]
    public static NumberValue BitwiseAnd(NumberValue x, NumberValue y) =>
        new((int)x.Value & (int)y.Value);

    [ToastCommand("|", "Bitwise OR operator.", Precedence = 6)]
    public static NumberValue BitwiseOr(NumberValue x, NumberValue y) =>
        new((int)x.Value | (int)y.Value);

    [ToastCommand("^", "Bitwise XOR operator.", Precedence = 6)]
    public static NumberValue BitwiseXor(NumberValue x, NumberValue y) =>
        new((int)x.Value ^ (int)y.Value);

    [ToastCommand("<<", "Bitwise left shift operator.", Precedence = 8)]
    public static NumberValue LeftShift(NumberValue x, NumberValue y) =>
        new((int)x.Value << (int)y.Value);

    [ToastCommand(">>", "Bitwise right shift operator.", Precedence = 8)]
    public static NumberValue RightShift(NumberValue x, NumberValue y) =>
        new((int)x.Value >> (int)y.Value);
}
