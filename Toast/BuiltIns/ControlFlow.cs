namespace Toast.BuiltIns;

public static class ControlFlow
{
    public static readonly Command If = Command.CreateFunction(
        "if",
        (Context context, BoolValue cond, AstNodeValue body) =>
        {
            if (cond.Value)
            {
                var val = context.Toaster.Evaluate(body.Node, context);
                if (val is FunctionValue funcVal && funcVal.Parameters.Count == 0)
                {
                    return funcVal.Execute([]);
                }
                return val;
            }
            return NullValue.Instance;
        }
    );

    public static readonly Command Else = Command.CreateFunction(
        "else",
        (Context context, AstNodeValue leftNode, AstNodeValue rightNode) =>
        {
            var rawLeftNode = leftNode.Node;
            while (rawLeftNode is GroupNode gn && gn.Items.Count == 1)
            {
                rawLeftNode = gn.Items[0];
            }

            if (
                rawLeftNode is CallNode callNode
                && callNode.Callee is IdentifierNode idNode
                && idNode.Name == "if"
                && callNode.Arguments.Count == 2
            )
            {
                var condObj = context.Toaster.Evaluate(callNode.Arguments[0], context);
                if (condObj is BoolValue cond && cond.Value)
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
                    var val = context.Toaster.Evaluate(rightNode.Node, context);
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
        (Context context, AstNodeValue cond, AstNodeValue body) =>
        {
            ToastValue lastVal = NullValue.Instance;
            while (true)
            {
                var condVal = context.Toaster.Evaluate(cond.Node, context);
                if (condVal is BoolValue b && b.Value)
                {
                    var val = context.Toaster.Evaluate(body.Node, context);
                    if (val is FunctionValue funcVal && funcVal.Parameters.Count == 0)
                    {
                        lastVal = funcVal.Execute([]);
                    }
                    else
                    {
                        lastVal = val;
                    }
                }
                else
                {
                    break;
                }
            }
            return lastVal;
        }
    );

    public static readonly Command For = Command.CreateFunction(
        "for",
        (Context context, ListValue items, AstNodeValue body) =>
        {
            ToastValue lastVal = NullValue.Instance;
            var bodyVal = context.Toaster.Evaluate(body.Node, context);
            if (bodyVal is FunctionValue funcVal)
            {
                foreach (var item in items.Elements)
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
