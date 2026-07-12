namespace Toast.BuiltIns;

public static class ControlFlow
{
    public static readonly Command If = Command.CreateFunction(
        "if",
        (Context context, bool cond, Node body) =>
        {
            if (cond)
            {
                var val = context.Toaster.Evaluate(body, context);
                if (val is FunctionValue funcVal && funcVal.Parameters.Count == 0)
                {
                    return funcVal.Execute([]);
                }
                return val;
            }
            return null;
        }
    );

    public static readonly Command Else = Command.CreateFunction(
        "else",
        (Context context, Node leftNode, Node rightNode) =>
        {
            while (leftNode is GroupNode gn && gn.Items.Count == 1)
            {
                leftNode = gn.Items[0];
            }

            if (
                leftNode is CallNode callNode
                && callNode.Callee is IdentifierNode idNode
                && idNode.Name == "if"
                && callNode.Arguments.Count == 2
            )
            {
                var cond = (bool)context.Toaster.Evaluate(callNode.Arguments[0], context)!;
                if (cond)
                {
                    var val = context.Toaster.Evaluate(callNode.Arguments[1], context);
                    if (val is FunctionValue funcVal && funcVal.Parameters.Count == 0)
                    {
                        return funcVal.Execute([]);
                    }
                    return val;
                }
                else
                {
                    var val = context.Toaster.Evaluate(rightNode, context);
                    if (val is FunctionValue funcVal && funcVal.Parameters.Count == 0)
                    {
                        return funcVal.Execute([]);
                    }
                    return val;
                }
            }

            throw new InvalidOperationException("Left side of 'else' must be an 'if' expression.");
        },
        precedence: 6,
        isRightAssociative: true,
        isInfix: true
    );

    public static readonly Command While = Command.CreateFunction(
        "while",
        (Context context, Node cond, Node body) =>
        {
            object? lastVal = null;
            while (true)
            {
                var condVal = context.Toaster.Evaluate(cond, context);
                if (!(condVal is bool b && b))
                {
                    break;
                }
                var val = context.Toaster.Evaluate(body, context);
                if (val is FunctionValue funcVal && funcVal.Parameters.Count == 0)
                {
                    lastVal = funcVal.Execute([]);
                }
                else
                {
                    lastVal = val;
                }
            }
            return lastVal;
        }
    );

    public static readonly Command For = Command.CreateFunction(
        "for",
        (Context context, System.Collections.IEnumerable items, Node body) =>
        {
            object? lastVal = null;
            var bodyVal = context.Toaster.Evaluate(body, context);
            if (bodyVal is FunctionValue funcVal)
            {
                foreach (var item in items)
                {
                    if (funcVal.Parameters.Count > 0)
                    {
                        lastVal = funcVal.Execute([item]);
                    }
                    else
                    {
                        lastVal = funcVal.Execute([]);
                    }
                }
            }
            return lastVal;
        }
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(If);
        toast.RegisterCommand(Else);
        toast.RegisterCommand(While);
        toast.RegisterCommand(For);
    }
}
