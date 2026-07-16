namespace Toast.BuiltIns;

public static class Variables
{
    private static TypeValue ResolveType(Context context, ToastValue typeVal)
    {
        if (typeVal is TypeValue tv)
        {
            return tv;
        }

        var typeName = typeVal.ToString();
        if (context.HasVariable(typeName))
        {
            var val = context.GetValue(typeName);
            if (val is TypeValue resolvedTv)
            {
                return resolvedTv;
            }
        }

        var targetType = typeName switch
        {
            "string" => ToastType.String,
            "number" => ToastType.Number,
            "boolean" => ToastType.Boolean,
            "list" => ToastType.List,
            "object" => ToastType.Object,
            _ => new ToastType(typeName),
        };

        return new TypeValue(targetType, null);
    }

    public static readonly Command TypeAnnotation = Command.CreateOperator(
        ":",
        (Context context, IdentifierValue id, ToastValue typeVal) =>
        {
            return new TypedIdentifierValue(id.Name, ResolveType(context, typeVal));
        },
        precedence: 10,
        isRightAssociative: false
    );

    public static readonly Command Var = Command.CreateFunction(
        "var",
        (Context context, IdentifierValue target) =>
        {
            string varName;
            TypeValue typeConstraint = TypeValue.Any;

            if (target is TypedIdentifierValue typedId)
            {
                varName = typedId.Name;
                typeConstraint = typedId.TargetTypeVal;
            }
            else
            {
                varName = target.Name;
            }

            if (context.GetBindings().ContainsKey(varName))
            {
                throw new InvalidOperationException(
                    $"Variable '{varName}' is already defined in the current scope."
                );
            }
            context.GetOrCreateLocal(varName, typeConstraint);
            return new ReferenceValue(new VariableAssignTarget(context, varName));
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
                    var param = funcVal.Parameters[i];
                    var paramName = param.Name;
                    var argVal = args[i];
                    TypeValue? paramConstraint = null;

                    if (param.Type != null)
                    {
                        var expectedType = param.Type.Type;
                        if (argVal.Type != expectedType && expectedType != ToastType.Any)
                        {
                            if (
                                !callerCtx.Toaster.TryConvert(
                                    argVal,
                                    argVal.Type,
                                    expectedType,
                                    objCtx,
                                    out var converted
                                )
                            )
                            {
                                throw new InvalidOperationException(
                                    $"Type mismatch: Constructor parameter '{param.Name}' expects {expectedType}, but got {argVal.Type}."
                                );
                            }
                            argVal = converted;
                        }
                        paramConstraint = ResolveType(
                            objCtx,
                            new IdentifierValue(expectedType.Name)
                        );
                    }

                    objCtx.GetOrCreateLocal(paramName, paramConstraint);
                    objCtx.SetValueDirect(paramName, argVal);
                }
                foreach (var stmt in funcVal.Statements)
                {
                    callerCtx.Toaster.Evaluate(stmt, objCtx);
                }
                var customType = name == "@type_factory" ? null : new ToastType(name);
                return new ObjectValue(objCtx, customType);
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
            FindMembers(stmt);
        }
        return members;

        void FindMembers(Node node)
        {
            if (node is CallNode callNode)
            {
                if (callNode.Callee is IdentifierNode idNode)
                {
                    Command? cmd = null;
                    if (context.Toaster.PrefixCommands.TryGetValue(idNode.Name, out var prefixCmd))
                        cmd = prefixCmd;
                    else if (
                        context.Toaster.InfixCommands.TryGetValue(idNode.Name, out var infixCmd)
                    )
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

                foreach (var arg in callNode.Arguments)
                {
                    FindMembers(arg);
                }
            }
            else if (node is GroupNode groupNode)
            {
                foreach (var item in groupNode.Items)
                {
                    FindMembers(item);
                }
            }
        }
    }

    public static readonly Command TypeCreator = Command.CreateFunction(
        "type",
        (Context context, FunctionValue funcVal) =>
        {
            var factoryCmd = CreateConstructorFactory("@type_factory", "type", funcVal);
            var declaredMembers = GetDeclaredMembers(context, funcVal);
            var memberTypes = new Dictionary<string, ToastType>();
            foreach (var param in funcVal.Parameters)
            {
                memberTypes[param.Name] = param.Type?.Type ?? ToastType.Any;
            }
            return new TypeValue(
                new ToastType("@type_factory"),
                factoryCmd,
                declaredMembers,
                memberTypes
            );
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
            var memberTypes = new Dictionary<string, ToastType>();
            foreach (var param in funcVal.Parameters)
            {
                memberTypes[param.Name] = param.Type?.Type ?? ToastType.Any;
            }
            var typeVal = new TypeValue(
                new ToastType(id.Name),
                factoryCmd,
                declaredMembers,
                memberTypes
            );
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
                newCtx.GetOrCreateLocal(kvp.Key, kvp.Value.Constraint);
                newCtx.SetValueDirect(kvp.Key, kvp.Value.Value);
            }
            foreach (var kvp in right.Context.GetBindings())
            {
                newCtx.GetOrCreateLocal(kvp.Key, kvp.Value.Constraint);
                newCtx.SetValueDirect(kvp.Key, kvp.Value.Value);
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
        toast.RegisterCommand(TypeAnnotation);
    }
}
