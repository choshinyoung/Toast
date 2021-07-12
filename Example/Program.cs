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

            toast.AddCommand(BasicCommands.All);

            toast.AddCommand(ToastCommand.Create("hello", () => Console.WriteLine("hello")),
                             ToastCommand.Create<object>("print", Console.WriteLine),
                             ToastCommand.Create<string, string>("reverse", s => new string(s.Reverse().ToArray())));

            Execute("print exp 2 10");

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
