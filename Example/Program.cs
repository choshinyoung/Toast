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

            ToastCommand cmd = new("a", () => Console.WriteLine("hello"));
            ToastCommand cmd2 = new("b", (i) => Console.WriteLine(i));
            ToastCommand cmd3 = new("c", (i) => (int)i * 2);
        }
    }
}
