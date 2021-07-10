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

            toast.Commands.Add(ToastCommand.Create("a", () => Console.WriteLine("hello")));
            toast.Commands.Add(ToastCommand.Create<int>("b", (i) => Console.WriteLine(i)));
            toast.Commands.Add(ToastCommand.Create<int, int>("c", (i) => i * 2));
        }
    }
}
