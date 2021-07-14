using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            toaster.AddCommand(ToastCommand.Create<ToastContext>("hello", (ctx) => Console.WriteLine("hello")));

            while (true)
            {
                Console.Write("> ");
                Execute(Console.ReadLine());
            }

            void Execute(string line)
            {
                object result = toaster.Execute(line);

                if (result is not null)
                {
                    Console.WriteLine(result);
                }
            }
        }
    }
}
