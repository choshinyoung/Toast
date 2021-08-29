using System;
using Toast;

namespace Examples
{
    class CustomContextExample
    {
        public static void Run()
        {
            Toaster toaster = new();

            toaster.AddCommand(ToastCommand.CreateFunc<CustomContext, int>("getValue", (ctx) => ctx.Value));

            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();

                object result = toaster.Execute(line, new CustomContext(10));

                if (result is not null)
                {
                    Console.WriteLine(result);
                }
            }
        }

        class CustomContext : ToastContext
        {
            public readonly int Value;

            public CustomContext(int value)
            {
                Value = value;
            }
        }
    }
}
