namespace Toast.SystemModules;

[ToastModule("flow", "Control flow functions.")]
public class FlowModule : IToastModule
{
    [ToastCommand("if", "Evaluates and returns the body expression if the condition is true.")]
    public static ToastValue If(Context context, BoolValue cond, AstNodeValue body)
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

    [ToastCommand(
        "else",
        "Evaluates the right branch if the 'if' condition is false.",
        Precedence = 6,
        IsRightAssociative = true,
        IsInfix = true
    )]
    public static ToastValue Else(Context context, AstNodeValue leftNode, AstNodeValue rightNode)
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

        throw new ToastException(
            new SyntaxError("Left side of 'else' must be an 'if' expression.")
        );
    }

    [ToastCommand(
        "while",
        "Repeatedly evaluates the body expression while the condition remains true."
    )]
    public static ToastValue While(Context context, AstNodeValue cond, AstNodeValue body)
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

    [ToastCommand(
        "for",
        "Iterates over elements of a list, executing the body function for each element."
    )]
    public static ToastValue For(Context context, ListValue items, AstNodeValue body)
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

    [ToastCommand(
        "throw",
        "Throws an error object or creates a RuntimeError with the given message.",
        IsPrefix = true
    )]
    public static ToastValue Throw(Context context, ErrorValue err)
    {
        throw new ToastException(err);
    }

    [ToastCommand("try", "Executes a block of code, capturing any errors thrown.", IsPrefix = true)]
    public static ToastValue Try(Context context, AstNodeValue bodyNode)
    {
        try
        {
            var val = context.Toaster.Evaluate(bodyNode.Node, context);
            if (val is FunctionValue funcVal && funcVal.Parameters.Count == 0)
            {
                val = funcVal.Execute([]);
            }
            return val;
        }
        catch (ToastException ex)
        {
            return ex.Error;
        }
    }

    [ToastCommand(
        "catch",
        "Catches an error thrown in a 'try' block and executes an error handling function.",
        Precedence = 6,
        IsRightAssociative = true,
        IsInfix = true
    )]
    public static ToastValue Catch(Context context, AstNodeValue leftNode, AstNodeValue rightNode)
    {
        var rawLeftNode = leftNode.Node;
        while (rawLeftNode is GroupNode gn && gn.Items.Count == 1)
        {
            rawLeftNode = gn.Items[0];
        }

        if (
            rawLeftNode is CallNode callNode
            && callNode.Callee is IdentifierNode idNode
            && idNode.Name == "try"
            && callNode.Arguments.Count == 1
        )
        {
            ToastValue tryVal;
            try
            {
                tryVal = context.Toaster.Evaluate(callNode.Arguments[0], context);
                if (tryVal is FunctionValue f && f.Parameters.Count == 0)
                {
                    tryVal = f.Execute([]);
                }
                return tryVal;
            }
            catch (ToastException ex)
            {
                var handlerVal = context.Toaster.Evaluate(rightNode.Node, context);
                if (handlerVal is FunctionValue funcVal)
                {
                    return funcVal.Execute([ex.Error]);
                }
                return handlerVal;
            }
        }

        throw new ToastException(
            new SyntaxError("Left side of 'catch' must be a 'try' expression.")
        );
    }
}
