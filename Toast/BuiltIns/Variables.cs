namespace Toast.BuiltIns;

public static class Variables
{
    public static readonly Command Var = Command.CreateFunction(
        "var",
        (Context context, IdentifierValue id) =>
        {
            if (context.GetBindings().ContainsKey(id.Name))
            {
                throw new InvalidOperationException($"Variable '{id.Name}' is already defined in the current scope.");
            }
            context.GetOrCreateLocal(id.Name);
            return new ReferenceValue(new VariableAssignTarget(context, id.Name));
        }
    );

    public static readonly Command Assign = Command.CreateOperator(
        "=",
        (Context context, ReferenceValue leftVal, ToastObject rightVal) =>
        {
            leftVal.Target.SetValue(rightVal);
            return rightVal;
        },
        precedence: 1,
        isRightAssociative: true
    );

    public static readonly Command AssignAdd = Command.CreateOperator(
        "+=",
        (Context context, ReferenceValue leftVal, ToastObject rightVal) =>
        {
            var currentVal = leftVal.Target.GetValue();
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
            leftVal.Target.SetValue(newVal);
            return newVal;
        },
        precedence: 1,
        isRightAssociative: true
    );

    public static readonly Command AssignSub = Command.CreateOperator(
        "-=",
        (Context context, ReferenceValue leftVal, ToastObject rightVal) =>
        {
            var currentVal = leftVal.Target.GetValue();
            if (currentVal is NumberValue ln && rightVal is NumberValue rn)
            {
                var newVal = new NumberValue(ln.Value - rn.Value);
                leftVal.Target.SetValue(newVal);
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

                if (context.Toaster.Executor.SuppressDereference)
                {
                    return new ReferenceValue(new VariableAssignTarget(objVal.Context, fieldName));
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
