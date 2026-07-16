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
        },
        declaresMember: true
    );

    public static readonly Command Assign = Command.CreateOperator(
        "=",
        (Context context, ReferenceValue leftVal, ToastValue rightVal) =>
        {
            leftVal.Target.SetValue(rightVal);
            return rightVal;
        },
        precedence: 1,
        isRightAssociative: true
    );

    public static readonly Command AssignAdd = Command.CreateOperator(
        "+=",
        (Context context, ReferenceValue leftVal, ToastValue rightVal) =>
        {
            var currentVal = leftVal.Target.GetValue();
            ToastValue newVal;
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
        (Context context, ReferenceValue leftVal, ToastValue rightVal) =>
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
        (Context context, ToastValue left, AstNodeValue rightNode) =>
        {
            if (left is not ObjectValue objVal)
            {
                throw new InvalidOperationException("Left side of '.' must be an object.");
            }

            string fieldName;
            if (rightNode.Node is IdentifierNode idNode)
            {
                fieldName = idNode.Name;
            }
            else
            {
                var evalRight = context.Toaster.Evaluate(rightNode.Node, context);
                fieldName = evalRight switch
                {
                    IdentifierValue idVal => idVal.Name,
                    StringValue strVal => strVal.Value,
                    _ => throw new InvalidOperationException(
                        "Right side of '.' must be an identifier or string."
                    ),
                };
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
            if (
                !context.Toaster.Executor.SuppressZeroArgFunction
                && val is CommandValue cmdVal
                && cmdVal.Command.ParameterCount == 0
            )
            {
                return cmdVal.Command.TargetDelegate(context, []);
            }
            return val;
        },
        precedence: 10
    );

    private static Command CreateConstructorFactory(string name, string kind, FunctionValue funcVal)
    {
        var parameterTypes = Enumerable.Repeat(ToastType.Any, funcVal.Parameters.Count).ToList();
        var isParameterLazy = Enumerable.Repeat(false, funcVal.Parameters.Count).ToList();

        return new Command(
            name,
            (Context callerCtx, ToastValue[] args) =>
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

    public static HashSet<string> GetDeclaredMembers(Context context, FunctionValue funcVal)
    {
        var members = new HashSet<string>();
        foreach (var param in funcVal.Parameters)
        {
            members.Add(param.Name);
        }
        foreach (var stmt in funcVal.Statements)
        {
            if (stmt is CallNode callNode && callNode.Callee is IdentifierNode idNode)
            {
                Command? cmd = null;
                if (context.Toaster.PrefixCommands.TryGetValue(idNode.Name, out var prefixCmd))
                    cmd = prefixCmd;
                else if (context.Toaster.InfixCommands.TryGetValue(idNode.Name, out var infixCmd))
                    cmd = infixCmd;
                else if (
                    context.HasVariable(idNode.Name)
                    && context.GetValue(idNode.Name) is CommandValue cmdVal
                )
                    cmd = cmdVal.Command;

                if (cmd != null && cmd.DeclaresMember)
                {
                    if (
                        callNode.Arguments.Count > 0
                        && callNode.Arguments[0] is IdentifierNode argId
                    )
                    {
                        members.Add(argId.Name);
                    }
                }
            }
        }
        return members;
    }

    public static readonly Command TypeCreator = Command.CreateFunction(
        "type",
        (Context context, FunctionValue funcVal) =>
        {
            var factoryCmd = CreateConstructorFactory("type_factory", "type", funcVal);
            var declaredMembers = GetDeclaredMembers(context, funcVal);
            return new TypeValue(new ToastType("type_factory"), factoryCmd, declaredMembers);
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
            var declaredMembers = GetDeclaredMembers(context, funcVal);
            var typeVal = new TypeValue(new ToastType(id.Name), factoryCmd, declaredMembers);
            context.SetValueDirect(id.Name, typeVal);
            return typeVal;
        },
        declaresMember: true
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
        },
        declaresMember: true
    );

    public static readonly Command With = Command.CreateFunction(
        "with",
        (Context context, ObjectValue left, ObjectValue right) =>
        {
            var newCtx = new Context(left.Context.Toaster, left.Context.Parent);
            foreach (var kvp in left.Context.GetBindings())
            {
                newCtx.SetValueDirect(kvp.Key, kvp.Value);
            }
            foreach (var kvp in right.Context.GetBindings())
            {
                newCtx.SetValueDirect(kvp.Key, kvp.Value);
            }
            return new ObjectValue(newCtx, left.CustomType);
        },
        precedence: 6,
        isInfix: true
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
        toast.RegisterCommand(With);
    }
}
