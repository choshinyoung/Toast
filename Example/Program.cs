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
            Toast.Toast toast = new();

            toast.AddConverter(BasicConverters.All);
            toast.AddCommand(BasicCommands.All);

            toast.AddCommand(ToastCommand.Create("hello", () => Console.WriteLine("hello")));

            while (true)
            {
                Console.Write("> ");
                Execute(Console.ReadLine());
            }

            void Execute(string line)
            {
                object result = toast.Execute(line);

                if (result is not null)
                {
                    Console.WriteLine(result);
                }
            }
        }
    }
}
