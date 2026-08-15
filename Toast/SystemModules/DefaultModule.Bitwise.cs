namespace Toast.SystemModules;

public partial class DefaultModule
{
    public static readonly Command BitwiseNot = Command.CreateOperator(
        "~",
        (Context context, NumberValue val) => new NumberValue(~(int)val.Value),
        precedence: 9,
        isPrefix: true,
        description: "Bitwise NOT operator, inverts all bits of an integer.",
        returnType: ToastType.Number
    );
    public static readonly Command BitwiseAnd = Command.CreateOperator(
        "&",
        (Context ctx, NumberValue x, NumberValue y) => new NumberValue((int)x.Value & (int)y.Value),
        precedence: 6,
        description: "Bitwise AND operator.",
        returnType: ToastType.Number
    );
    public static readonly Command BitwiseOr = Command.CreateOperator(
        "|",
        (Context ctx, NumberValue x, NumberValue y) => new NumberValue((int)x.Value | (int)y.Value),
        precedence: 6,
        description: "Bitwise OR operator.",
        returnType: ToastType.Number
    );
    public static readonly Command BitwiseXor = Command.CreateOperator(
        "^",
        (Context ctx, NumberValue x, NumberValue y) => new NumberValue((int)x.Value ^ (int)y.Value),
        precedence: 6,
        description: "Bitwise XOR operator.",
        returnType: ToastType.Number
    );
    public static readonly Command LeftShift = Command.CreateOperator(
        "<<",
        (Context ctx, NumberValue x, NumberValue y) =>
            new NumberValue((int)x.Value << (int)y.Value),
        precedence: 8,
        description: "Bitwise left shift operator.",
        returnType: ToastType.Number
    );
    public static readonly Command RightShift = Command.CreateOperator(
        ">>",
        (Context ctx, NumberValue x, NumberValue y) =>
            new NumberValue((int)x.Value >> (int)y.Value),
        precedence: 8,
        description: "Bitwise right shift operator.",
        returnType: ToastType.Number
    );

    public static void RegisterBitwise(Toaster toast)
    {
        toast.RegisterCommand(BitwiseNot);
        toast.RegisterCommand(BitwiseAnd);
        toast.RegisterCommand(BitwiseOr);
        toast.RegisterCommand(BitwiseXor);
        toast.RegisterCommand(LeftShift);
        toast.RegisterCommand(RightShift);
    }
}
