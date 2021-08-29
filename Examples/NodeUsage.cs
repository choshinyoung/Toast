using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toast;
using Toast.Nodes;

namespace Examples
{
    class NodeUsage
    {
        public static void Run()
        {
            Toaster toaster = new();

            toaster.AddCommand(BasicCommands.Operators);
            toaster.AddCommand(BasicCommands.Others);
            toaster.AddCommand(BasicCommands.Literals);
            toaster.AddConverter(BasicConverters.All);

            toaster.AddCommand(ToastCommand.CreateAction<ToastContext, int, FunctionNode>("repeat", (ctx, x, y) =>
            {
                for (int i = 0; i < x; i++)
                {
                    ctx.Toaster.ExecuteFunction(y, new object[] { i }, ctx);
                }
            }));

            toaster.AddCommand(ToastCommand.CreateFunc<ToastContext, bool, object, object>("if", (ctx, x, y) => x ? y : null));
            toaster.AddCommand(ToastCommand.CreateFunc<CommandNode, ToastContext, object, object>("else", (x, ctx, y) =>
            {
                if (x.Command.Name != "if")
                {
                    throw new Exception();
                }

                if ((bool)ctx.Toaster.ExecuteNode(x.Parameters[0]))
                {
                    return ctx.Toaster.ExecuteNode(x.Parameters[1]);
                }
                else
                {
                    return y;
                }
            }));

            toaster.AddCommand(ToastCommand.CreateFunc<VariableNode, ToastContext, object, string>("of", (x, ctx, y) =>
            {
                if (x.Name == "type")
                {
                    return y.GetType().Name;
                }
                else
                {
                    throw new Exception();
                }
            }));

            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();

                object result = toaster.Execute(line);

                if (result is not null)
                {
                    Console.WriteLine(result);
                }
            }
        }
    }
}
