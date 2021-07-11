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

            toast.AddCommand(ToastCommand.Create("hello", () => Console.WriteLine("hello")));
            toast.AddCommand(ToastCommand.Create<object>("print", Console.WriteLine));
            toast.AddCommand(ToastCommand.Create<string, string>("reverse", s => new string(s.Reverse().ToArray())));
            toast.AddCommand(ToastCommand.Create<float, float, float>("numberAdd", (x, y) => x + y));

            toast.AddCommand(ToastCommand.Create("true", () => true));
            toast.AddCommand(ToastCommand.Create("false", () => false));

            toast.AddCommand(ToastCommand.Create<object, object, bool>("equal", (x, y) => x == y));

            Execute("numberAdd 10 15");
            Execute("print \"asdf\"");

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
