namespace Toast.BuiltIns;

public static class Utility
{
    public static readonly Command Print = Command.CreateFunction(
        "print",
        (Context context, ToastObject val) =>
        {
            Console.WriteLine(val);
            return NullValue.Instance;
        }
    );

    public static readonly Command Input = Command.CreateFunction(
        "input",
        (Context context) => new StringValue(Console.ReadLine() ?? "")
    );

    public static readonly Command Execute = Command.CreateFunction(
        "execute",
        (Context context, FunctionValue func, ListValue args) =>
        {
            return func.Execute(args.Elements);
        }
    );

    public static readonly Command Random = Command.CreateFunction(
        "random",
        (Context context, NumberValue min, NumberValue max) =>
        {
            return new NumberValue(new Random().Next((int)min.Value, (int)max.Value));
        }
    );

    public static readonly Command RandomChoice = Command.CreateFunction(
        "randomChoice",
        (Context context, ListValue list) =>
        {
            if (list.Elements.Count == 0)
                return NullValue.Instance;
            return list.Elements[new Random().Next(0, list.Elements.Count)];
        }
    );

    public static readonly Command TypeCreator = Command.CreateFunction(
        "type",
        (Context context, FunctionValue funcVal) =>
        {
            var parameterTypes = Enumerable
                .Repeat(ToastType.Any, funcVal.Parameters.Count)
                .ToList();
            var isParameterLazy = Enumerable.Repeat(false, funcVal.Parameters.Count).ToList();

            var factoryCmd = new Command(
                "type_factory",
                (Context callerCtx, ToastObject[] args) =>
                {
                    if (args.Length != funcVal.Parameters.Count)
                    {
                        throw new InvalidOperationException(
                            $"Arity mismatch: type constructor expects {funcVal.Parameters.Count} arguments, but got {args.Length}."
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

            var parameterTypes = Enumerable
                .Repeat(ToastType.Any, funcVal.Parameters.Count)
                .ToList();
            var isParameterLazy = Enumerable.Repeat(false, funcVal.Parameters.Count).ToList();

            var factoryCmd = new Command(
                id.Name,
                (Context callerCtx, ToastObject[] args) =>
                {
                    if (args.Length != funcVal.Parameters.Count)
                    {
                        throw new InvalidOperationException(
                            $"Arity mismatch: class constructor expects {funcVal.Parameters.Count} arguments, but got {args.Length}."
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

    public static readonly Command Quote = Command.CreateOperator(
        "`",
        (Context context, AstNodeValue nodeVal) =>
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

            ToastObject finalResult;
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
        },
        precedence: 9,
        isPrefix: true
    );

    public static readonly Command Pipeline = Command.CreateOperator(
        "|>",
        (Context context, AstNodeValue leftNode, AstNodeValue rightNode) =>
        {
            if (rightNode.Node is CallNode callNode)
            {
                var newCallNode = new CallNode(
                    callNode.Callee,
                    [leftNode.Node, .. callNode.Arguments]
                );
                return context.Toaster.Evaluate(newCallNode, context);
            }
            else
            {
                var newCallNode = new CallNode(rightNode.Node, [leftNode.Node]);
                return context.Toaster.Evaluate(newCallNode, context);
            }
        },
        precedence: 2
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(Print);
        toast.RegisterCommand(Input);
        toast.RegisterCommand(Execute);
        toast.RegisterCommand(Random);
        toast.RegisterCommand(RandomChoice);
        toast.RegisterCommand(TypeCreator);
        toast.RegisterCommand(ClassCreator);
        toast.RegisterCommand(FunctionCreator);
        toast.RegisterCommand(Quote);
        toast.RegisterCommand(Pipeline);
    }
}
