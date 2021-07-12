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

            toast.AddCommand(ToastCommand.Create("hello", () => Console.WriteLine("hello")));

            Execute("print member input 3");

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
