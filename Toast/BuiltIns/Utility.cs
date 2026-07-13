namespace Toast.BuiltIns;

public static class Utility
{
    public static readonly Command Print = Command.CreateFunction(
        "print",
        (Context context, object? val) =>
        {
            Console.WriteLine(val);
        }
    );

    public static readonly Command Input = Command.CreateFunction(
        "input",
        (Context context) => Console.ReadLine()
    );

    public static readonly Command Execute = Command.CreateFunction(
        "execute",
        (Context context, FunctionValue func, System.Collections.IEnumerable args) =>
        {
            var argList = args.Cast<object?>().ToList();
            return func.Execute(argList);
        }
    );

    public static readonly Command Random = Command.CreateFunction(
        "random",
        (Context context, int min, int max) =>
        {
            return new Random().Next(min, max);
        }
    );

    public static readonly Command RandomChoice = Command.CreateFunction(
        "randomChoice",
        (Context context, System.Collections.IEnumerable list) =>
        {
            var items = list.Cast<object?>().ToList();
            if (items.Count == 0)
                return null;
            return items[new Random().Next(0, items.Count)];
        }
    );

    public static readonly Command Quote = Command.CreateOperator(
        "`",
        (Context context, Node node) =>
        {
            if (node is IdentifierNode idNode)
            {
                if (context.Toaster.InfixCommands.TryGetValue(idNode.Name, out var infixCmd))
                    return infixCmd;
                if (context.Toaster.PrefixCommands.TryGetValue(idNode.Name, out var prefixCmd))
                    return prefixCmd;
            }
            else if (
                node is GroupNode gn
                && gn.Items.Count == 1
                && gn.Items[0] is IdentifierNode innerId
            )
            {
                if (context.Toaster.InfixCommands.TryGetValue(innerId.Name, out var infixCmd))
                    return infixCmd;
                if (context.Toaster.PrefixCommands.TryGetValue(innerId.Name, out var prefixCmd))
                    return prefixCmd;
            }

            var executor = context.Toaster.Executor;
            return executor.Evaluate(node, context, suppressZeroArgFunction: true);
        },
        precedence: 9,
        isPrefix: true
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(Print);
        toast.RegisterCommand(Input);
        toast.RegisterCommand(Execute);
        toast.RegisterCommand(Random);
        toast.RegisterCommand(RandomChoice);
        toast.RegisterCommand(Quote);
    }
}
