using System;
using System.Linq;
using Toast;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Toaster toaster = new();

            toaster.AddConverter(BasicConverters.All);
            toaster.AddCommand(BasicCommands.All);

            toaster.AddCommand(ToastCommand.CreateAction<ToastContext>("hello", (ctx) => Console.WriteLine("hello")));

            toaster.AddCommand(ToastCommand.CreateAction<ToastContext>("cmds", (ctx) =>
            {
                Console.WriteLine($"Command list: {string.Join(", ", ctx.Toaster.GetCommands().Select(c => c.Name))}");
            }));

            toaster.AddCommand(ToastCommand.CreateFunc<CustomContext, string>("getValue", (ctx) => ctx.Value));

            while (true)
            {
                Console.Write("> ");
                Execute(Console.ReadLine());
            }

            void Execute(string line)
            {
                object result = toaster.Execute(line, new CustomContext("sans"));

                if (result is not null)
                {
                    Console.WriteLine(result);
                }
            }
        }
    }
}
