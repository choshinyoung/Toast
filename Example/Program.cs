﻿using System;
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
            toast.AddCommand(ToastCommand.Create<int>("b", i => Console.WriteLine(i)));
            toast.AddCommand(ToastCommand.Create<float, float>("c", i => i * 2));
            toast.AddCommand(ToastCommand.Create<string, string>("d", s => new string(s.Reverse().ToArray())));

            int result = (int)(float)toast.Execute("c 5");
            Console.WriteLine(result);
        }
    }
}
