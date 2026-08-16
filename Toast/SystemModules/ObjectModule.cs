namespace Toast.SystemModules;

[ToastModule("object", "Variable, type and object functions.")]
public class ObjectModule : IToastModule
{
    [ToastCommand(
        ":",
        "Type annotation operator, constrains a variable or parameter to a specified type.",
        Precedence = 10
    )]
    public static TypedIdentifierValue TypeAnnotation(IdentifierValue id, TypeValue typeVal)
    {
        return new TypedIdentifierValue(id.Name, typeVal);
    }

    [ToastCommand("var", "Declares a new variable in the current scope.", DeclaresMember = true)]
    public static ToastValue Var(Context context, IdentifierValue target)
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
            throw new ToastException(RuntimeError.AlreadyDefined(varName));
        }
        context.GetOrCreateLocal(varName, typeConstraint);
        return new ReferenceValue(new VariableAssignTarget(context, varName));
    }

    [ToastCommand(
        "=",
        "Assignment operator, stores a value into a variable or object property.",
        Precedence = 1,
        IsRightAssociative = true,
        IsInfix = true
    )]
    public static ToastValue Assign(ReferenceValue leftVal, ToastValue rightVal)
    {
        leftVal.Target.SetValue(rightVal);
        return rightVal;
    }

    [ToastCommand(
        "+=",
        "Addition assignment operator.",
        Precedence = 1,
        IsRightAssociative = true,
        IsInfix = true
    )]
    public static ToastValue AssignAdd(ReferenceValue leftVal, ToastValue rightVal)
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
            throw new ToastException(new TypeError("Invalid types for += operator."));
        }
        leftVal.Target.SetValue(newVal);
        return newVal;
    }

    [ToastCommand(
        "-=",
        "Subtraction assignment operator.",
        Precedence = 1,
        IsRightAssociative = true,
        IsInfix = true
    )]
    public static ToastValue AssignSub(ReferenceValue leftVal, ToastValue rightVal)
    {
        var currentVal = leftVal.Target.GetValue();
        if (currentVal is NumberValue ln && rightVal is NumberValue rn)
        {
            var newVal = new NumberValue(ln.Value - rn.Value);
            leftVal.Target.SetValue(newVal);
            return newVal;
        }
        throw new ToastException(new TypeError("Invalid types for -= operator."));
    }

    [ToastCommand(
        ".",
        "Member access operator, accesses a property or method of an object.",
        Precedence = 10
    )]
    public static ToastValue MemberAccess(Context context, ToastValue left, AstNodeValue rightNode)
    {
        if (left is TypeValue typeVal)
        {
            if (rightNode.Node is not IdentifierNode typeIdNode)
            {
                throw new ToastException(new TypeError("Right side of '.' must be an identifier."));
            }
            string staticFieldName = typeIdNode.Name;
            if (
                context.Toaster.ExtensionMembers.TryGetValue(typeVal.TargetType, out var members)
                && members.TryGetValue(staticFieldName, out var staticMemberVal)
            )
            {
                if (
                    !context.Toaster.Executor.SuppressZeroArgFunction
                    && staticMemberVal is FunctionValue sfv
                    && sfv.Parameters.Count == 0
                )
                {
                    return sfv.Execute([]);
                }
                if (
                    !context.Toaster.Executor.SuppressZeroArgFunction
                    && staticMemberVal is CommandValue scv
                    && scv.Command.Parameters.Count == 0
                )
                {
                    return scv.Command.TargetDelegate(context, []);
                }
                return staticMemberVal;
            }
            throw new ToastException(RuntimeError.PropertyNotDefined(staticFieldName));
        }

        if (left is not ObjectValue objVal)
        {
            throw new ToastException(new TypeError("Left side of '.' must be an object or type."));
        }

        if (rightNode.Node is not IdentifierNode idNode)
        {
            throw new ToastException(new TypeError("Right side of '.' must be an identifier."));
        }

        string fieldName = idNode.Name;

        if (context.Toaster.Executor.SuppressDereference)
        {
            return new ReferenceValue(new ObjectPropertyAssignTarget(objVal, fieldName));
        }

        var bindings = objVal.Context.GetBindings();
        if (!bindings.TryGetValue(fieldName, out var binding))
        {
            if (
                context.Toaster.ExtensionMembers.TryGetValue(objVal.Type, out var extMembers)
                && extMembers.TryGetValue(fieldName, out var extVal)
            )
            {
                if (
                    !context.Toaster.Executor.SuppressZeroArgFunction
                    && extVal is FunctionValue efv
                    && efv.Parameters.Count == 0
                )
                {
                    return efv.Execute([]);
                }
                if (
                    !context.Toaster.Executor.SuppressZeroArgFunction
                    && extVal is CommandValue ecv
                    && ecv.Command.Parameters.Count == 0
                )
                {
                    return ecv.Command.TargetDelegate(context, []);
                }
                return extVal;
            }
            throw new ToastException(RuntimeError.PropertyNotDefined(fieldName));
        }

        var val = binding.Value;
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
    }

    [ToastCommand("type", "Creates an anonymous custom type constructor.")]
    public static ToastValue TypeCreator(Context context, FunctionValue funcVal)
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

    [ToastCommand("class", "Declares a named class/type.", DeclaresMember = true)]
    public static ToastValue ClassCreator(
        Context context,
        IdentifierValue id,
        FunctionValue funcVal
    )
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
    }

    [ToastCommand(
        "function",
        "Declares a named function in the current scope.",
        DeclaresMember = true
    )]
    public static ToastValue FunctionCreator(
        Context context,
        IdentifierValue id,
        FunctionValue funcVal
    )
    {
        if (context.GetBindings().ContainsKey(id.Name))
        {
            throw new ToastException(RuntimeError.AlreadyDefined(id.Name));
        }
        context.SetValueDirect(id.Name, funcVal);
        return funcVal;
    }

    [ToastCommand(
        "with",
        "Merges properties of two objects to produce a combined object.",
        Precedence = 6,
        IsInfix = true
    )]
    public static ObjectValue With(ObjectValue left, ObjectValue right)
    {
        var newCtx = new Context(left.Context.Toaster, left.Context.Parent);
        var newObj = new ObjectValue(newCtx, left.CustomType);
        newCtx.Owner = newObj;

        foreach (var kvp in left.Context.GetBindings())
        {
            newCtx.GetOrCreateLocal(kvp.Key, kvp.Value.Constraint);
            var val = kvp.Value.Value;
            if (val is FunctionValue funcVal)
            {
                val = funcVal with { ClosureContext = newCtx };
            }
            newCtx.SetValueDirect(kvp.Key, val);
        }
        foreach (var kvp in right.Context.GetBindings())
        {
            newCtx.GetOrCreateLocal(kvp.Key, kvp.Value.Constraint);
            var val = kvp.Value.Value;
            if (val is FunctionValue funcVal)
            {
                val = funcVal with { ClosureContext = newCtx };
            }
            newCtx.SetValueDirect(kvp.Key, val);
        }
        return newObj;
    }

    [ToastCommand(
        "typeof",
        "Returns the Type value of the given expression.",
        Precedence = 9,
        IsPrefix = true
    )]
    public static TypeValue TypeOf(Context context, ToastValue val)
    {
        var typeName = val.Type.Name;
        if (context.HasVariable(typeName))
        {
            var registeredVal = context.GetValue(typeName);
            if (registeredVal is TypeValue tv)
            {
                return tv;
            }
        }
        return new TypeValue(val.Type, null);
    }

    private static Command CreateConstructorFactory(string name, string kind, FunctionValue funcVal)
    {
        var parameterTypes = Enumerable.Repeat(ToastType.Any, funcVal.Parameters.Count).ToList();
        var isParameterLazy = Enumerable.Repeat(false, funcVal.Parameters.Count).ToList();

        return new Command(
            name,
            (Context callerCtx, ToastValue[] args) =>
            {
                if (funcVal.Parameters.Count != args.Length)
                {
                    throw new ToastException(
                        TypeError.ArityMismatch(kind, funcVal.Parameters.Count, args.Length)
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
                                throw new ToastException(
                                    TypeError.Mismatch(expectedType, argVal.Type, param.Name)
                                );
                            }
                            argVal = converted;
                        }
                        paramConstraint = new TypeValue(param.Type.Type, null);
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
            isParameterLazy: isParameterLazy,
            description: $"Constructor factory for '{name}'."
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
}
