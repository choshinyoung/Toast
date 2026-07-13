namespace Toast.BuiltIns;

public static class Variables
{
    public static readonly Command Var = Command.CreateFunction(
        "var",
        (Context context, IdentifierNode idNode) =>
        {
            context.GetOrCreateLocal(idNode.Name);
            return idNode;
        }
    );

    public static readonly Command Assign = Command.CreateOperator(
        "=",
        (Context context, Node lhsNode, object? rightVal) =>
        {
            string name;
            if (lhsNode is IdentifierNode rawIdNode)
            {
                name = rawIdNode.Name;
            }
            else
            {
                var eval = context.Toaster.Evaluate(lhsNode, context);
                if (eval is IdentifierNode idNode)
                {
                    name = idNode.Name;
                }
                else
                {
                    throw new InvalidOperationException("Left side of '=' must be an identifier.");
                }
            }

            context.SetValue(name, rightVal);
            return rightVal;
        },
        precedence: 1,
        isRightAssociative: true
    );

    public static readonly Command AssignAdd = Command.CreateOperator(
        "+=",
        (Context context, Node lhsNode, object? rightVal) =>
        {
            string name;
            if (lhsNode is IdentifierNode rawIdNode)
            {
                name = rawIdNode.Name;
            }
            else
            {
                var eval = context.Toaster.Evaluate(lhsNode, context);
                if (eval is IdentifierNode idNode)
                {
                    name = idNode.Name;
                }
                else
                {
                    throw new InvalidOperationException("Left side of '+=' must be an identifier.");
                }
            }

            var currentVal = context.GetValue(name);
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
            context.SetValue(name, newVal);
            return newVal;
        },
        precedence: 1,
        isRightAssociative: true
    );

    public static readonly Command AssignSub = Command.CreateOperator(
        "-=",
        (Context context, Node lhsNode, object? rightVal) =>
        {
            string name;
            if (lhsNode is IdentifierNode rawIdNode)
            {
                name = rawIdNode.Name;
            }
            else
            {
                var eval = context.Toaster.Evaluate(lhsNode, context);
                if (eval is IdentifierNode idNode)
                {
                    name = idNode.Name;
                }
                else
                {
                    throw new InvalidOperationException("Left side of '-=' must be an identifier.");
                }
            }

            var currentVal = context.GetValue(name);
            object newVal;
            if (currentVal is double || rightVal is double)
            {
                newVal = Convert.ToDouble(currentVal) - Convert.ToDouble(rightVal);
            }
            else
            {
                newVal = Convert.ToInt32(currentVal) - Convert.ToInt32(rightVal);
            }
            context.SetValue(name, newVal);
            return newVal;
        },
        precedence: 1,
        isRightAssociative: true
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
        toast.RegisterCommand(AssignAdd);
        toast.RegisterCommand(AssignSub);
        toast.RegisterCommand(MemberAccess);
    }
}
