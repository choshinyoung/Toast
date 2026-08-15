namespace Toast.SystemModules;

public class ImportModule : IToastModule
{
    public string Name => "import";
    public string Description => "Module loading command (import).";

    public static readonly Command ImportCommand = Command.CreateFunction(
        "import",
        (Context context, AstNodeValue targetNode) =>
        {
            string moduleName;
            var node = targetNode.Node;
            while (node is GroupNode gn && gn.Items.Count == 1)
            {
                node = gn.Items[0];
            }

            if (node is IdentifierNode id)
            {
                moduleName = id.Name;
            }
            else if (node is LiteralNode lit && lit.Value is StringValue strLit)
            {
                moduleName = strLit.Value;
            }
            else
            {
                var evaluated = context.Toaster.Evaluate(node, context);
                if (evaluated is StringValue str)
                {
                    moduleName = str.Value;
                }
                else if (evaluated is TypeValue tv)
                {
                    moduleName = tv.TargetType.Name;
                }
                else
                {
                    throw new ToastException(
                        new TypeError(
                            "Argument for 'import' must be a module identifier or string name."
                        )
                    );
                }
            }

            ModuleManager.Instance.LoadModule(moduleName, context.Toaster, context);
            return NullValue.Instance;
        },
        description: "Imports a module or globally installed package."
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(ImportCommand);
    }

    public void Load(Toaster toaster, Context callerContext)
    {
        Register(toaster);
    }
}
