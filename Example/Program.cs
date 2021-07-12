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

            toast.AddConverter(ToastConverter.Create<int, bool>(i => i != 0));

            toast.AddCommand(ToastCommand.Create("hello", () => Console.WriteLine("hello")));
            toast.AddCommand(ToastCommand.Create<object>("print", Console.WriteLine));
            toast.AddCommand(ToastCommand.Create<string, string>("reverse", s => new string(s.Reverse().ToArray())));

            toast.AddCommand(BasicCommands.All);

            Execute("print null");

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
