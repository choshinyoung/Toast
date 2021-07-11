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

            string result = (string)toast.Execute("_ a s d f as df asdf _asdf _a_s_d_f");
            Console.WriteLine(result);
        }
    }
}
