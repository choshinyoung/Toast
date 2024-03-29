﻿using System;
using System.Collections.Generic;
using System.Linq;
using Toast.Exceptions;
using Toast.Nodes;

namespace Toast
{
    public class BasicCommands
    {
        public static ToastCommand[] All =>
            Literals.Concat(Operators).Concat(Statements).Concat(Others).Concat(Lists).Concat(Strings).ToArray();

        public static ToastCommand[] Literals => new ToastCommand[]
        {
            Null,
            True, False,
        };

        public static ToastCommand[] Operators => new ToastCommand[]
        {
            Addition, Subtraction, Multiplication, Division, Modulus, Exponentiation, FloorDivision,
            Equal, Greater, Less, GreaterOrEqual, LessOrEqual,
            And, Or, Not,
            BitwiseAnd, BitwiseOr, BitwiseXor, BitwiseNot, LeftShift, RightShift,
        };

        public static ToastCommand[] Statements => new ToastCommand[]
        {
            If, Else,
            While, For
        };

        public static ToastCommand[] Others => new ToastCommand[]
        {
            Print, Input, Assign, Convert, Execute, Random, RandomChoice
        };

        public static ToastCommand[] Lists => new ToastCommand[]
        {
            Member, Length, IndexOf, Filter, Map, Combine, Append, Remove, Sort, SortAs, Shuffle, Range
        };

        public static ToastCommand[] Strings => new ToastCommand[]
        {
            Split, Reverse, StartsWith, EndsWith, Contains, Trim, Substring, Join, Replace, ToUpper, ToLower
        };

        public static readonly ToastCommand True =
            ToastCommand.CreateFunc<ToastContext, bool>("true", (ctx) => true);

        public static readonly ToastCommand False =
            ToastCommand.CreateFunc<ToastContext, bool>("false", (ctx) => false);

        public static readonly ToastCommand Null =
            ToastCommand.CreateFunc<ToastContext, object>("null", (ctx) => null);

        public static readonly ToastCommand Addition =
            ToastCommand.CreateFunc<float, ToastContext, float, float>("add", (x, ctx, y) => x + y, 12);

        public static readonly ToastCommand Subtraction =
            ToastCommand.CreateFunc<float, ToastContext, float, float>("sub", (x, ctx, y) => x - y, 12);

        public static readonly ToastCommand Multiplication =
            ToastCommand.CreateFunc<float, ToastContext, float, float>("mul", (x, ctx, y) => x * y, 13);

        public static readonly ToastCommand Division =
            ToastCommand.CreateFunc<float, ToastContext, float, float>("div", (x, ctx, y) => x / y, 13);

        public static readonly ToastCommand Modulus =
            ToastCommand.CreateFunc<float, ToastContext, float, float>("mod", (x, ctx, y) => x % y, 13);

        public static readonly ToastCommand Exponentiation =
            ToastCommand.CreateFunc<float, ToastContext, float, float>("exp", (x, ctx, y) => MathF.Pow(x, y), 13);

        public static readonly ToastCommand FloorDivision =
            ToastCommand.CreateFunc<int, ToastContext, int, int>("floorDiv", (x, ctx, y) => x / y, 13);

        public static readonly ToastCommand Equal =
            ToastCommand.CreateFunc<object, ToastContext, object, bool>("is", (x, ctx, y) => x.Equals(y), 9);

        public static readonly ToastCommand Greater =
            ToastCommand.CreateFunc<float, ToastContext, float, bool>("greater", (x, ctx, y) => x > y, 10);

        public static readonly ToastCommand Less =
            ToastCommand.CreateFunc<float, ToastContext, float, bool>("less", (x, ctx, y) => x < y, 10);

        public static readonly ToastCommand GreaterOrEqual =
            ToastCommand.CreateFunc<float, ToastContext, float, bool>("greaterEqual", (x, ctx, y) => x >= y, 10);

        public static readonly ToastCommand LessOrEqual =
            ToastCommand.CreateFunc<float, ToastContext, float, bool>("lessEqual", (x, ctx, y) => x <= y, 10);

        public static readonly ToastCommand And =
            ToastCommand.CreateFunc<bool, ToastContext, bool, bool>("and", (x, ctx, y) => x && y, 3);

        public static readonly ToastCommand Or =
            ToastCommand.CreateFunc<bool, ToastContext, bool, bool>("or", (x, ctx, y) => x || y, 2);

        public static readonly ToastCommand Not =
            ToastCommand.CreateFunc<ToastContext, bool, bool>("not", (ctx, x) => !x, 4);

        public static readonly ToastCommand BitwiseAnd =
            ToastCommand.CreateFunc<int, ToastContext, int, int>("bitAnd", (x, ctx, y) => x & y, 7);

        public static readonly ToastCommand BitwiseOr =
            ToastCommand.CreateFunc<int, ToastContext, int, int>("bitOr", (x, ctx, y) => x | y, 5);

        public static readonly ToastCommand BitwiseXor =
            ToastCommand.CreateFunc<int, ToastContext, int, int>("bitXor", (x, ctx, y) => x ^ y, 6);

        public static readonly ToastCommand BitwiseNot =
            ToastCommand.CreateFunc<ToastContext, int, int>("bitNot", (ctx, x) => ~x, 8);

        public static readonly ToastCommand LeftShift =
            ToastCommand.CreateFunc<int, ToastContext, int, int>("lShift", (x, ctx, y) => x << y, 11);

        public static readonly ToastCommand RightShift =
            ToastCommand.CreateFunc<int, ToastContext, int, int>("rShift", (x, ctx, y) => x >> y, 11);

        public static readonly ToastCommand Assign =
            ToastCommand.CreateAction<ToastContext, CommandNode>("var", (ctx, x) =>
            {
                if (!(x.Parameters[0] is VariableNode))
                {
                    throw new InvalidCommandNodeException("var", x.Command.Name);
                }

                string name = ((VariableNode)x.Parameters[0]).Name;

                if (x.Command.Name == "is")
                {
                    object value = ctx.Toaster.ExecuteNode(x.Parameters[1], ctx);
                    setVariable(value);
                }
                else if (x.Command == Addition || x.Command == Subtraction || x.Command == Multiplication || x.Command == Division || x.Command == Modulus || x.Command == Exponentiation || x.Command == FloorDivision ||
                        x.Command == BitwiseAnd || x.Command == BitwiseOr || x.Command == BitwiseXor || x.Command == LeftShift || x.Command == RightShift)
                {
                    setVariable(ctx.Toaster.ExecuteNode(x, ctx));
                }
                else
                {
                    throw new InvalidCommandNodeException(x.Command.Name, x.Command.Name);
                }

                void setVariable(object value)
                {
                    ToastCommand cmd = ctx.Toaster.GetCommands().ToList().Find(c => c.Name == name);
                    if (cmd != null)
                    {
                        ctx.Toaster.RemoveCommand(cmd);
                    }

                    ctx.Toaster.AddCommand(ToastCommand.CreateFunc<ToastContext, object>(name, (ctx) => value));
                }
            }, -1);

        public static readonly ToastCommand Convert =
            ToastCommand.CreateFunc<object, ToastContext, VariableNode, object>("as", (x, ctx, y) =>
            {
                if (!ctx.Toaster.TypeAliases.ContainsKey(y.Name))
                {
                    throw new Exception($"Cannot find a type '{y.Name}'.");
                }

                return ctx.Toaster.ExecuteConverter(x, ctx.Toaster.TypeAliases[y.Name], ctx);
            });

        public static readonly ToastCommand If =
            ToastCommand.CreateFunc<ToastContext, bool, FunctionNode, object>("if", (ctx, x, y) => x ? ctx.Toaster.ExecuteFunction(y, Array.Empty<object>(), ctx) : null);

        public static readonly ToastCommand Else =
            ToastCommand.CreateFunc<CommandNode, ToastContext, FunctionNode, object>("else", (x, ctx, y) =>
            {
                if (x.Command.Name != "if")
                {
                    throw new Exception("else 문 앞에는 if문이 필요해요");
                }

                return (bool)ctx.Toaster.ExecuteNode(x.Parameters[0], ctx) ?
                        ctx.Toaster.ExecuteFunction((FunctionNode)ctx.Toaster.ExecuteNode(x.Parameters[1], ctx), Array.Empty<object>(), ctx) :
                        ctx.Toaster.ExecuteFunction(y, Array.Empty<object>(), ctx);
            });

        public static readonly ToastCommand While =
            ToastCommand.CreateAction<ToastContext, INode, FunctionNode>("while", (ctx, x, y) =>
            {
                while ((bool)ctx.Toaster.ExecuteNode(x, ctx))
                {
                    ctx.Toaster.ExecuteFunction(y, Array.Empty<object>(), ctx);
                }
            });

        public static readonly ToastCommand For =
            ToastCommand.CreateAction<ToastContext, object[], FunctionNode>("for", (ctx, x, y) => x.ToList().ForEach(o => ctx.Toaster.ExecuteFunction(y, new[] { o }, ctx)));

        public static readonly ToastCommand Print =
            ToastCommand.CreateAction<ToastContext, object>("print", (ctx, x) => Console.WriteLine(x), -2);

        public static readonly ToastCommand Input =
            ToastCommand.CreateFunc<ToastContext, string>("input", (ctx) => Console.ReadLine());

        public static readonly ToastCommand Execute =
            ToastCommand.CreateFunc<ToastContext, FunctionNode, object[], object>("execute", (ctx, x, y) => ctx.Toaster.ExecuteFunction(x, y, ctx));

        public static readonly ToastCommand Random =
            ToastCommand.CreateFunc<ToastContext, int, int, long>("random", (ctx, x, y) => new Random().Next(x, y));

        public static readonly ToastCommand RandomChoice =
            ToastCommand.CreateFunc<ToastContext, object[], object>("randomChoice", (ctx, x) => x[new Random().Next(0, x.Length)]);

        public static readonly ToastCommand Member =
            ToastCommand.CreateFunc<ToastContext, int, object[], object>("member", (ctx, x, y) => y[x]);

        public static readonly ToastCommand Length =
            ToastCommand.CreateFunc<ToastContext, object[], int>("len", (ctx, x) => x.Length);

        public static readonly ToastCommand IndexOf =
            ToastCommand.CreateFunc<ToastContext, object[], object, int>("indexOf", (ctx, x, y) =>
            {
                if (y is string s && s.Length == 1)
                {
                    y = s[0];
                }

                return Array.IndexOf(x, y);
            });

        public static readonly ToastCommand Filter =
            ToastCommand.CreateFunc<ToastContext, object[], FunctionNode, object[]>("filter", (ctx, x, y) => x.Where(o => (bool)ctx.Toaster.ExecuteFunction(y, new[] { o }, ctx)).ToArray());

        public static readonly ToastCommand Map =
            ToastCommand.CreateFunc<ToastContext, object[], FunctionNode, object[]>("map", (ctx, x, y) => x.Select(o => ctx.Toaster.ExecuteFunction(y, new[] { o }, ctx)).ToArray());

        public static readonly ToastCommand Combine =
            ToastCommand.CreateFunc<ToastContext, object[], object[], object[]>("combine", (ctx, x, y) => x.Concat(y).ToArray());

        public static readonly ToastCommand Append =
            ToastCommand.CreateFunc<ToastContext, object[], object, object[]>("append", (ctx, x, y) => x.Concat(new[] { y }).ToArray());

        public static readonly ToastCommand Remove =
            ToastCommand.CreateFunc<ToastContext, object[], object, object[]>("remove", (ctx, x, y) =>
            {
                var list = x.ToList();
                list.Remove(y);
                return list.ToArray();
            });

        public static readonly ToastCommand Sort =
            ToastCommand.CreateFunc<ToastContext, object[], object[]>("sort", (ctx, x) =>
            {
                var list = x.ToList();
                list.Sort();
                return list.ToArray();
            });

        public static readonly ToastCommand SortAs =
            ToastCommand.CreateFunc<ToastContext, object[], FunctionNode, object[]>("sortAs", (ctx, x, y) =>
            {
                var list = x.ToList();
                list.Sort((obj1, obj2) => ((IComparable)ctx.Toaster.ExecuteFunction(y, new[] { obj1 }, ctx)).CompareTo((IComparable)ctx.Toaster.ExecuteFunction(y, new[] { obj2 }, ctx)));
                return list.ToArray();
            });

        public static readonly ToastCommand Shuffle =
            ToastCommand.CreateFunc<ToastContext, object[], object[]>("shuffle", (ctx, x) =>
            {
                Random random = new System.Random();
                return x.OrderBy(a => random.Next()).ToArray();
            });

        public static readonly ToastCommand Range =
            ToastCommand.CreateFunc<ToastContext, int, int, object[]>("range", (ctx, x, y) => Enumerable.Range(x, y).Select(a => (object)a).ToArray());

        public static readonly ToastCommand Split =
            ToastCommand.CreateFunc<ToastContext, string, string, object[]>("split", (ctx, x, y) => x.Split(y).Select(s => (object)s).ToArray());

        public static readonly ToastCommand Reverse =
            ToastCommand.CreateFunc<ToastContext, string, string>("reverse", (ctx, s) => new string(s.Reverse().ToArray()));

        public static readonly ToastCommand StartsWith =
            ToastCommand.CreateFunc<ToastContext, string, string, bool>("startsWith", (ctx, x, y) => x.StartsWith(y));

        public static readonly ToastCommand EndsWith =
            ToastCommand.CreateFunc<ToastContext, string, string, bool>("endsWith", (ctx, x, y) => x.EndsWith(y));

        public static readonly ToastCommand Contains =
            ToastCommand.CreateFunc<ToastContext, string, string, bool>("contains", (ctx, x, y) => x.Contains(y));

        public static readonly ToastCommand Trim =
            ToastCommand.CreateFunc<ToastContext, string, string>("trim", (ctx, x) => x.Trim());

        public static readonly ToastCommand Substring =
            ToastCommand.CreateFunc<ToastContext, string, int, int, string>("substring", (ctx, x, y, z) => x.Substring(y, z));

        public static readonly ToastCommand Join =
            ToastCommand.CreateFunc<string, ToastContext, object, string>("join", (x, ctx, y) =>
            {
                if (y is object[] arr)
                {
                    return string.Join(x, arr);
                }
                else
                {
                    return x + y.ToString();
                }
            });

        public static readonly ToastCommand Replace =
            ToastCommand.CreateFunc<ToastContext, string, string, string, string>("replace", (ctx, x, y, z) => x.Replace(y, z));

        public static readonly ToastCommand ToUpper =
            ToastCommand.CreateFunc<ToastContext, string, string>("toUpper", (ctx, x) => x.ToUpper());

        public static readonly ToastCommand ToLower =
            ToastCommand.CreateFunc<ToastContext, string, string>("toLower", (ctx, x) => x.ToLower());

        private BasicCommands() { }
    }
}
