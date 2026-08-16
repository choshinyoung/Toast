namespace Toast.SystemModules;

[ToastModule("default", "Default primitive operator functions.")]
public partial class DefaultModule : IToastModule
{
    [ToastCommand("as", "Converts a value to the specified type.", Precedence = 6, IsInfix = true)]
    public static ToastValue As(Context context, ToastValue val, TypeValue targetTypeVal)
    {
        var targetType = targetTypeVal.TargetType;
        if (val.Type == targetType)
        {
            return val;
        }

        if (context.Toaster.TryConvert(val, val.Type, targetType, context, out var converted))
        {
            return converted;
        }

        throw new ToastException(
            new TypeError($"Cannot convert value of type '{val.Type.Name}' to '{targetType.Name}'.")
        );
    }

    [ToastCommand(
        "`",
        "Quotes an expression to obtain its command, function, or reference without direct invocation.",
        Precedence = 9,
        IsPrefix = true
    )]
    public static ToastValue Quote(Context context, AstNodeValue nodeVal)
    {
        var node = nodeVal.Node;
        if (node is IdentifierNode idNode)
        {
            if (context.Toaster.InfixCommands.TryGetValue(idNode.Name, out var infixCmd))
                return new CommandValue(infixCmd);
            if (context.Toaster.PrefixCommands.TryGetValue(idNode.Name, out var prefixCmd))
                return new CommandValue(prefixCmd);
        }
        else if (
            node is GroupNode gn
            && gn.Items.Count == 1
            && gn.Items[0] is IdentifierNode innerId
        )
        {
            if (context.Toaster.InfixCommands.TryGetValue(innerId.Name, out var infixCmd))
                return new CommandValue(infixCmd);
            if (context.Toaster.PrefixCommands.TryGetValue(innerId.Name, out var prefixCmd))
                return new CommandValue(prefixCmd);
        }

        var executor = context.Toaster.Executor;
        var result = executor.Evaluate(
            node,
            context,
            suppressZeroArgFunction: true,
            suppressDereference: true
        );

        ToastValue finalResult;
        if (result is ReferenceValue refVal)
        {
            var innerVal = refVal.Target.GetValue();
            if (innerVal is FunctionValue || innerVal is CommandValue)
            {
                finalResult = innerVal;
            }
            else
            {
                finalResult = refVal;
            }
        }
        else
        {
            finalResult = result;
        }

        if (finalResult is not (CommandValue or FunctionValue or ReferenceValue))
        {
            throw new InvalidOperationException(
                "Quote operand must evaluate to a command, function, or reference."
            );
        }

        return finalResult;
    }

    [ToastCommand(
        "|>",
        "Pipes the result of the left expression as the first argument to the right function call.",
        Precedence = 2
    )]
    public static ToastValue Pipeline(
        Context context,
        AstNodeValue leftNode,
        AstNodeValue rightNode
    )
    {
        if (rightNode.Node is CallNode callNode)
        {
            var newCallNode = new CallNode(callNode.Callee, [leftNode.Node, .. callNode.Arguments]);
            return context.Toaster.Evaluate(newCallNode, context);
        }
        else
        {
            var newCallNode = new CallNode(rightNode.Node, [leftNode.Node]);
            return context.Toaster.Evaluate(newCallNode, context);
        }
    }
}
