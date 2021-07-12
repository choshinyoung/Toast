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
            toast.AddCommand(ToastCommand.Create<int, int, int>("numberAdd", (x, y) => x + y));

            toast.AddCommand(ToastCommand.Create("true", () => true));
            toast.AddCommand(ToastCommand.Create("false", () => false));

            toast.AddCommand(ToastCommand.Create<object, object, bool>("equal", (x, y) => x == y));
            toast.AddCommand(ToastCommand.Create<bool, bool>("isTrue", (x) => x));

            Execute("isTrue 1");

            Console.WriteLine(BasicCommands.All.Count);

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
