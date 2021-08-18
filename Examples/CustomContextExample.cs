using System;
using Toast;

namespace Examples
{
    class CustomContextExample
    {
        public static void Run()
        {
            Toaster toaster = new();

            toaster.AddCommand(ToastCommand.CreateFunc<CustomContext, string>("getValue", (ctx) => ctx.Value));

            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();

                object result = toaster.Execute(line, new CustomContext("sans"));

                if (result is not null)
                {
                    Console.WriteLine(result);
                }
            }
        }

        class CustomContext : ToastContext
        {
            public readonly string Value;

            public CustomContext(string value)
            {
                Value = value;
            }
        }
    }
}
