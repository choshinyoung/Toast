namespace Toast.BuiltIns;

public static class Variables
{
    public static readonly Command Var = Command.CreateFunction(
        "var",
        (Context context, IdentifierValue id) =>
        {
            if (context.GetBindings().ContainsKey(id.Name))
            {
                throw new InvalidOperationException(
                    $"Variable '{id.Name}' is already defined in the current scope."
                );
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

    private static Command CreateConstructorFactory(string name, string kind, FunctionValue funcVal)
    {
        var parameterTypes = Enumerable.Repeat(ToastType.Any, funcVal.Parameters.Count).ToList();
        var isParameterLazy = Enumerable.Repeat(false, funcVal.Parameters.Count).ToList();

        return new Command(
            name,
            (Context callerCtx, ToastObject[] args) =>
            {
                if (args.Length != funcVal.Parameters.Count)
                {
                    throw new InvalidOperationException(
                        $"Arity mismatch: {kind} constructor expects {funcVal.Parameters.Count} arguments, but got {args.Length}."
                    );
                }

                var objCtx = new Context(funcVal.ClosureContext);
                for (int i = 0; i < funcVal.Parameters.Count; i++)
                {
                    var paramName = funcVal.Parameters[i].Name;
                    objCtx.SetValueDirect(paramName, args[i]);
                }
                foreach (var stmt in funcVal.Statements)
                {
                    callerCtx.Toaster.Evaluate(stmt, objCtx);
                }
                return new ObjectValue(objCtx);
            },
            parameterTypes: parameterTypes,
            isParameterLazy: isParameterLazy
        );
    }

    public static readonly Command TypeCreator = Command.CreateFunction(
        "type",
        (Context context, FunctionValue funcVal) =>
        {
            var factoryCmd = CreateConstructorFactory("type_factory", "type", funcVal);
            return new CommandValue(factoryCmd);
        }
    );

    public static readonly Command ClassCreator = Command.CreateFunction(
        "class",
        (Context context, IdentifierValue id, FunctionValue funcVal) =>
        {
            if (context.GetBindings().ContainsKey(id.Name))
            {
                throw new InvalidOperationException(
                    $"Class '{id.Name}' is already defined in the current scope."
                );
            }

            var factoryCmd = CreateConstructorFactory(id.Name, "class", funcVal);
            var classConstructor = new CommandValue(factoryCmd);
            context.SetValueDirect(id.Name, classConstructor);
            return classConstructor;
        }
    );

    public static readonly Command FunctionCreator = Command.CreateFunction(
        "function",
        (Context context, IdentifierValue id, FunctionValue funcVal) =>
        {
            if (context.GetBindings().ContainsKey(id.Name))
            {
                throw new InvalidOperationException(
                    $"Variable '{id.Name}' is already defined in the current scope."
                );
            }
            context.SetValueDirect(id.Name, funcVal);
            return funcVal;
        }
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(Var);
        toast.RegisterCommand(Assign);
        toast.RegisterCommand(AssignAdd);
        toast.RegisterCommand(AssignSub);
        toast.RegisterCommand(MemberAccess);
        toast.RegisterCommand(TypeCreator);
        toast.RegisterCommand(ClassCreator);
        toast.RegisterCommand(FunctionCreator);
    }
}
