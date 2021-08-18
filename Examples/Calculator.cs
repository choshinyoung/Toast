using System;
using Toast;

namespace Examples
{
    class Calculator
    {
        public static void Run()
        {
            Toaster toaster = new();

            toaster.AddCommand(ToastCommand.CreateFunc<float, ToastContext, float, float>("a", (x, ctx, y) => x + y));
            toaster.AddCommand(ToastCommand.CreateFunc<float, ToastContext, float, float>("s", (x, ctx, y) => x - y));

            // Set priority as 1 so it is parsed first.
            toaster.AddCommand(ToastCommand.CreateFunc<float, ToastContext, float, float>("m", (x, ctx, y) => x * y, 1));
            toaster.AddCommand(ToastCommand.CreateFunc<float, ToastContext, float, float>("d", (x, ctx, y) => x / y, 1));

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
