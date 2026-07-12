namespace Toast.BuiltIns;

public static class Variables
{
    public static readonly Command Var = Command.CreateFunction(
        "var",
        (Context context, IdentifierNode idNode) =>
        {
            return context.GetOrCreateAddress(idNode.Name);
        }
    );

    public static readonly Command Assign = Command.CreateOperator(
        "=",
        (Context context, MemoryAddress addr, object? rightVal) =>
        {
            context.SetValueAtAddress(addr, rightVal);
            return rightVal;
        },
        precedence: 1
    );

    public static readonly Command Dereference = Command.CreateOperator(
        "*",
        object? (Context context, MemoryAddress addr) =>
        {
            return context.GetValueAtAddress(addr);
        },
        precedence: 9,
        isPrefix: true
    );

    public static readonly Command AssignAdd = Command.CreateOperator(
        "+=",
        (Context context, MemoryAddress addr, object? rightVal) =>
        {
            var currentVal = context.GetValueAtAddress(addr);
            object newVal;
            if (currentVal is string || rightVal is string)
            {
                newVal = (currentVal?.ToString() ?? "") + (rightVal?.ToString() ?? "");
            }
            else if (currentVal is double || rightVal is double)
            {
                newVal = Convert.ToDouble(currentVal) + Convert.ToDouble(rightVal);
            }
            else
            {
                newVal = Convert.ToInt32(currentVal) + Convert.ToInt32(rightVal);
            }
            context.SetValueAtAddress(addr, newVal);
            return newVal;
        },
        precedence: 1
    );

    public static readonly Command AssignSub = Command.CreateOperator(
        "-=",
        (Context context, MemoryAddress addr, object? rightVal) =>
        {
            var currentVal = context.GetValueAtAddress(addr);
            object newVal;
            if (currentVal is double || rightVal is double)
            {
                newVal = Convert.ToDouble(currentVal) - Convert.ToDouble(rightVal);
            }
            else
            {
                newVal = Convert.ToInt32(currentVal) - Convert.ToInt32(rightVal);
            }
            context.SetValueAtAddress(addr, newVal);
            return newVal;
        },
        precedence: 1
    );

    public static readonly Command MemberAccess = Command.CreateOperator(
        ".",
        (Context context, object? left, object? right) =>
        {
            return $"{left}.{right}";
        },
        precedence: 10
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(Var);
        toast.RegisterCommand(Assign);
        toast.RegisterCommand(Dereference);
        toast.RegisterCommand(AssignAdd);
        toast.RegisterCommand(AssignSub);
        toast.RegisterCommand(MemberAccess);
    }
}
