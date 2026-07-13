namespace Toast.BuiltIns;

public static class Variables
{
    private static (Context TargetCtx, string FieldName) ResolveLhs(Context context, Node lhsNode)
    {
        if (lhsNode is IdentifierNode rawIdNode)
        {
            return (context, rawIdNode.Name);
        }
        else if (
            lhsNode is CallNode callNode
            && callNode.Callee is IdentifierNode idNode
            && idNode.Name == "."
        )
        {
            var leftVal = context.Toaster.Evaluate(callNode.Arguments[0], context);
            if (leftVal is ObjectValue objVal)
            {
                var rightNode = callNode.Arguments[1];
                string fieldName;
                if (rightNode is IdentifierNode rid)
                {
                    fieldName = rid.Name;
                }
                else
                {
                    var evaluatedRight = context.Toaster.Evaluate(rightNode, context);
                    if (evaluatedRight is IdentifierValue rvid)
                        fieldName = rvid.Name;
                    else if (evaluatedRight is StringValue rvstr)
                        fieldName = rvstr.Value;
                    else
                        throw new InvalidOperationException(
                            "Member name must be an identifier or string."
                        );
                }

                return (objVal.Context, fieldName);
            }
            throw new InvalidOperationException("Left side of '.' must be an object.");
        }
        else
        {
            var eval = context.Toaster.Evaluate(lhsNode, context);
            if (eval is IdentifierValue idVal)
            {
                return (context, idVal.Name);
            }
            throw new InvalidOperationException(
                "Left side of assignment must be an identifier or object property."
            );
        }
    }

    public static readonly Command Var = Command.CreateFunction(
        "var",
        (Context context, AstNodeValue idNode) =>
        {
            if (idNode.Node is IdentifierNode id)
            {
                context.GetOrCreateLocal(id.Name);
                return new IdentifierValue(id.Name);
            }
            throw new InvalidOperationException("var parameter must be an identifier.");
        }
    );

    public static readonly Command Assign = Command.CreateOperator(
        "=",
        (Context context, AstNodeValue lhsNode, ToastObject rightVal) =>
        {
            var (targetCtx, fieldName) = ResolveLhs(context, lhsNode.Node);
            targetCtx.SetValue(fieldName, rightVal);
            return rightVal;
        },
        precedence: 1,
        isRightAssociative: true
    );

    public static readonly Command AssignAdd = Command.CreateOperator(
        "+=",
        (Context context, AstNodeValue lhsNode, ToastObject rightVal) =>
        {
            var (targetCtx, fieldName) = ResolveLhs(context, lhsNode.Node);
            var currentVal = targetCtx.GetValue(fieldName);

            ToastObject newVal;
            if (currentVal is StringValue || rightVal is StringValue)
            {
                newVal = new StringValue(currentVal.ToString() + rightVal.ToString());
            }
            else if (currentVal is NumberValue ln && rightVal is NumberValue rn)
            {
                newVal = new NumberValue(ln.Value + rn.Value);
            }
            else
            {
                throw new InvalidOperationException("Invalid types for += operator.");
            }
            targetCtx.SetValue(fieldName, newVal);
            return newVal;
        },
        precedence: 1,
        isRightAssociative: true
    );

    public static readonly Command AssignSub = Command.CreateOperator(
        "-=",
        (Context context, AstNodeValue lhsNode, ToastObject rightVal) =>
        {
            var (targetCtx, fieldName) = ResolveLhs(context, lhsNode.Node);
            var currentVal = targetCtx.GetValue(fieldName);

            if (currentVal is NumberValue ln && rightVal is NumberValue rn)
            {
                var newVal = new NumberValue(ln.Value - rn.Value);
                targetCtx.SetValue(fieldName, newVal);
                return newVal;
            }
            throw new InvalidOperationException("Invalid types for -= operator.");
        },
        precedence: 1,
        isRightAssociative: true
    );

    public static readonly Command MemberAccess = Command.CreateOperator(
        ".",
        (Context context, ToastObject left, AstNodeValue rightNode) =>
        {
            if (left is ObjectValue objVal)
            {
                string fieldName;
                if (rightNode.Node is IdentifierNode idNode)
                {
                    fieldName = idNode.Name;
                }
                else
                {
                    var evalRight = context.Toaster.Evaluate(rightNode.Node, context);
                    if (evalRight is IdentifierValue idVal)
                        fieldName = idVal.Name;
                    else if (evalRight is StringValue strVal)
                        fieldName = strVal.Value;
                    else
                        throw new InvalidOperationException(
                            "Right side of '.' must be an identifier or string."
                        );
                }

                var val = objVal.Context.GetValue(fieldName);
                if (
                    !context.Toaster.Executor.SuppressZeroArgFunction
                    && val is FunctionValue funcVal
                    && funcVal.Parameters.Count == 0
                )
                {
                    return funcVal.Execute([]);
                }
                return val;
            }
            throw new InvalidOperationException("Left side of '.' must be an object.");
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
