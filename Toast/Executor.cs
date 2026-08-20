namespace Toast;

public class Executor(Toaster _toast)
{
    public ToastValue Execute(string rawInput)
    {
        return Execute(rawInput, _toast.GlobalContext);
    }

    public ToastValue Execute(string rawInput, Context context)
    {
        var tokens = Lexer.Tokenize(rawInput);
        var ast = Parser.Parse(tokens, _toast.GetInfixInfo, _toast.IsPrefix, _toast, context);
        return EvaluateProgram(ast, context);
    }

    public bool SuppressZeroArgFunction { get; set; } = false;
    public bool SuppressDereference { get; set; } = false;
    public Location CurrentLocation { get; private set; } = new Location(1, 1);

    public ToastValue Evaluate(Node node, Context context)
    {
        return Evaluate(node, context, suppressZeroArgFunction: false, suppressDereference: false);
    }

    internal ToastValue Evaluate(
        Node node,
        Context context,
        bool suppressZeroArgFunction,
        bool suppressDereference
    )
    {
        var prevLoc = CurrentLocation;
        if (node.Location != null && node.Location != Location.Unknown)
        {
            CurrentLocation = node.Location;
        }

        var prevSuppress = SuppressZeroArgFunction;
        SuppressZeroArgFunction = suppressZeroArgFunction;

        var prevSuppressRef = SuppressDereference;
        SuppressDereference = suppressDereference;
        try
        {
            var result = node switch
            {
                LiteralNode literal => literal.Value,
                IdentifierNode identifier => EvaluateIdentifier(
                    identifier,
                    context,
                    suppressZeroArgFunction || SuppressZeroArgFunction
                ),
                ProgramNode program => EvaluateProgram(program, context),
                GroupNode group => EvaluateGroup(group, context),
                ListNode list => EvaluateList(list, context),
                ObjectLiteralNode objLiteral => EvaluateObjectLiteral(objLiteral, context),
                FunctionNode function => new FunctionValue(
                    function.Parameters,
                    function.Statements,
                    context,
                    _toast
                ),
                CallNode call => EvaluateCall(call, context),
                InterpolatedStringNode interpolated => EvaluateInterpolatedString(
                    interpolated,
                    context
                ),
                _ => throw new ToastException(
                    new SyntaxError(
                        $"Node type '{node.GetType().Name}' is not supported.",
                        CurrentLocation
                    )
                ),
            };

            if (result is ObjectValue objVal && objVal.Context.Toaster != _toast)
            {
                objVal.Context.Toaster = _toast;
            }

            return result;
        }
        catch (ToastException ex)
        {
            if (ex.Error.Location == Location.Unknown)
            {
                throw new ToastException(ex.Error.WithLocation(CurrentLocation));
            }
            throw;
        }
        catch (IndexOutOfRangeException)
        {
            throw new ToastException(
                IndexError.OutOfRange(-1, 0, "collection").WithLocation(CurrentLocation)
            );
        }
        catch (Exception ex)
        {
            throw new ToastException(new RuntimeError(ex.Message, CurrentLocation));
        }
        finally
        {
            SuppressZeroArgFunction = prevSuppress;
            SuppressDereference = prevSuppressRef;
            CurrentLocation = prevLoc;
        }
    }

    private ToastValue EvaluateIdentifier(
        IdentifierNode identifier,
        Context context,
        bool suppressZeroArgFunction
    )
    {
        if (context.HasVariable(identifier.Name))
        {
            if (SuppressDereference)
            {
                return new ReferenceValue(new VariableAssignTarget(context, identifier.Name));
            }

            var val = context.GetValue(identifier.Name);
            if (
                !suppressZeroArgFunction
                && val is FunctionValue funcVal
                && funcVal.Parameters.Count == 0
            )
            {
                return ExecuteFunction(funcVal, []);
            }
            if (
                !suppressZeroArgFunction
                && val is CommandValue cmdVal
                && cmdVal.Command.ParameterCount == 0
            )
            {
                return cmdVal.Command.TargetDelegate(context, []);
            }
            if (
                !suppressZeroArgFunction
                && val is TypeValue typeVal
                && typeVal.Constructor != null
                && typeVal.Constructor.ParameterCount == 0
            )
            {
                return typeVal.Constructor.TargetDelegate(context, []);
            }
            return val;
        }

        throw new InvalidOperationException(
            $"Variable or command '{identifier.Name}' is not defined."
        );
    }

    private ToastValue EvaluateProgram(ProgramNode program, Context context)
    {
        ToastValue lastVal = NullValue.Instance;
        foreach (var stmt in program.Statements)
        {
            lastVal = Evaluate(stmt, context, SuppressZeroArgFunction, SuppressDereference);
        }
        return lastVal;
    }

    private ToastValue EvaluateGroup(GroupNode group, Context context)
    {
        if (group.Items.Count == 1)
        {
            return Evaluate(group.Items[0], context, SuppressZeroArgFunction, SuppressDereference);
        }
        return new ListValue([
            .. group.Items.Select(item =>
                Evaluate(item, context, SuppressZeroArgFunction, SuppressDereference)
            ),
        ]);
    }

    private ToastValue EvaluateList(ListNode list, Context context)
    {
        return new ListValue([
            .. list.Items.Select(item =>
                Evaluate(item, context, SuppressZeroArgFunction, SuppressDereference)
            ),
        ]);
    }

    private ObjectValue EvaluateObjectLiteral(ObjectLiteralNode objLiteral, Context context)
    {
        var objCtx = new Context(context);
        foreach (var stmt in objLiteral.Statements)
        {
            if (
                stmt is CallNode callNode
                && callNode.Callee is IdentifierNode idNode
                && idNode.Name == "="
            )
            {
                if (callNode.Arguments.Count > 0 && callNode.Arguments[0] is IdentifierNode leftId)
                {
                    objCtx.GetOrCreateLocal(leftId.Name);
                }
            }
        }

        foreach (var stmt in objLiteral.Statements)
        {
            Evaluate(stmt, objCtx, SuppressZeroArgFunction, SuppressDereference);
        }
        return new ObjectValue(objCtx);
    }

    private ToastValue EvaluateCall(CallNode call, Context context)
    {
        var callArgs = call.Arguments.ToList();
        if (callArgs.Count == 1 && callArgs[0] is GroupNode gn)
        {
            callArgs = [.. gn.Items];
        }

        if (call.Callee is IdentifierNode idNode)
        {
            bool isInfixContext =
                context.Toaster.InfixCommands.ContainsKey(idNode.Name) && callArgs.Count >= 2;

            Command? cmd = null;
            if (isInfixContext && _toast.InfixCommands.TryGetValue(idNode.Name, out var infixCmd))
            {
                cmd = infixCmd;
            }
            else if (_toast.PrefixCommands.TryGetValue(idNode.Name, out var prefixCmd))
            {
                cmd = prefixCmd;
            }

            if (cmd != null)
            {
                return ExecuteCommand(cmd, callArgs, context);
            }
        }

        var calleeVal = Evaluate(
            call.Callee,
            context,
            suppressZeroArgFunction: true,
            suppressDereference: false
        );

        if (calleeVal is FunctionValue funcVal2)
        {
            if (funcVal2.Parameters.Count != callArgs.Count)
            {
                throw new ToastException(
                    TypeError.ArityMismatch(
                        call.Callee.ToString()!,
                        funcVal2.Parameters.Count,
                        callArgs.Count
                    )
                );
            }

            var evalArgs = callArgs.Select(arg => Evaluate(arg, context)).ToList();
            return ExecuteFunction(funcVal2, evalArgs);
        }

        if (calleeVal is CommandValue cmdVal2)
        {
            return ExecuteCommand(cmdVal2.Command, callArgs, context);
        }

        if (calleeVal is TypeValue typeVal)
        {
            if (typeVal.Constructor == null)
            {
                throw new ToastException(
                    new TypeError($"Type '{typeVal.TargetType.Name}' does not have a constructor.")
                );
            }
            return ExecuteCommand(typeVal.Constructor, callArgs, context);
        }

        throw new ToastException(new TypeError("Callee is not a callable function or command."));
    }

    private ToastValue ExecuteCommand(Command cmd, List<Node> callArgs, Context context)
    {
        if (callArgs.Count < cmd.ParameterCount)
        {
            throw new ToastException(
                TypeError.ArityMismatch(
                    cmd.Name,
                    cmd.ParameterCount,
                    callArgs.Count,
                    isAtLeast: true
                )
            );
        }

        var finalArgs = new ToastValue[callArgs.Count];
        if (cmd.IsRightAssociative)
        {
            for (int i = callArgs.Count - 1; i >= 0; i--)
            {
                EvaluateArg(i);
            }
        }
        else
        {
            for (int i = 0; i < callArgs.Count; i++)
            {
                EvaluateArg(i);
            }
        }

        return cmd.TargetDelegate(context, finalArgs);

        void EvaluateArg(int i)
        {
            var isLazy = i < cmd.Parameters.Count && cmd.Parameters[i].IsLazy;
            if (isLazy)
            {
                finalArgs[i] = new AstNodeValue(callArgs[i]);
            }
            else
            {
                var expectedType =
                    i < cmd.Parameters.Count ? cmd.Parameters[i].Type : ToastType.Any;
                var isReference = expectedType == ToastType.Reference;

                ToastValue evalVal;
                if (expectedType == ToastType.Identifier && callArgs[i] is IdentifierNode idNode)
                {
                    evalVal = new IdentifierValue(idNode.Name);
                }
                else
                {
                    evalVal = Evaluate(
                        callArgs[i],
                        context,
                        suppressZeroArgFunction: false,
                        suppressDereference: isReference
                    );
                }

                if (!isReference && evalVal is ReferenceValue refVal)
                {
                    evalVal = refVal.Target.GetValue();
                }

                var actualType = evalVal.Type;

                if (
                    _toast.TryConvert(evalVal, actualType, expectedType, context, out var converted)
                )
                {
                    finalArgs[i] = converted;
                }
                else
                {
                    var paramName = i < cmd.Parameters.Count ? cmd.Parameters[i].Name : $"arg{i}";
                    throw new ToastException(
                        TypeError.Mismatch(expectedType, actualType, paramName)
                    );
                }
            }
        }
    }

    private static ToastValue ExecuteFunction(FunctionValue funcVal, List<ToastValue> evalArgs)
    {
        return funcVal.Execute(evalArgs);
    }

    private StringValue EvaluateInterpolatedString(
        InterpolatedStringNode interpolated,
        Context context
    )
    {
        var sb = new System.Text.StringBuilder();
        foreach (var part in interpolated.Parts)
        {
            var val = Evaluate(
                part,
                context,
                suppressZeroArgFunction: false,
                suppressDereference: false
            );
            if (_toast.TryConvert(val, val.Type, ToastType.String, context, out var converted))
            {
                sb.Append(converted.ToString());
            }
            else
            {
                sb.Append(val.ToString());
            }
        }
        return new StringValue(sb.ToString());
    }
}
