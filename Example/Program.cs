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

            toast.AddCommand(ToastCommand.Create("a", () => Console.WriteLine("hello")));
            toast.AddCommand(ToastCommand.Create<object>("b", Console.WriteLine));
            toast.AddCommand(ToastCommand.Create<float, float>("c", i => i * 2));
            toast.AddCommand(ToastCommand.Create<string, string>("d", s => new string(s.Reverse().ToArray())));
            toast.AddCommand(ToastCommand.Create<float, float, float>("e", (x, y) => x + y));

            toast.Execute("b e ((1) 2)");
        }
    }
}
