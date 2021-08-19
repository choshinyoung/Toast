using System;
using Toast;

namespace Examples
{
    class Program
    {
        static void Main()
        {
            Toaster toaster = new();

            toaster.AddCommand(BasicCommands.All);
            toaster.AddConverter(BasicConverters.All);

            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();

                object result = toaster.ExecuteConverter<string>(toaster.Execute(line));

                if (result is not null)
                {
                    Console.WriteLine(result);
                }
            }
        }
    }
}
