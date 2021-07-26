using System;
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
            Repeat, While, For, Foreach
        };

        public static ToastCommand[] Others => new ToastCommand[]
        {
            Print, Input, Assign, Execute
        };

        public static ToastCommand[] Lists => new ToastCommand[]
        {
            Member, Length, IndexOf
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
                    if (x.Command != Equal || x.Parameters[0] is not VariableNode)
                    {
                        throw new InvalidCommandNodeException("var", x.Command.Name);
                    }

                    string name = ((VariableNode)x.Parameters[0]).Name;

                    ToastCommand cmd = ctx.Toaster.GetCommands().ToList().Find(c => c.Name == name);
                    if (cmd is not null)
                    {
                        ctx.Toaster.RemoveCommand(cmd);
                    }

                    ctx.Toaster.AddCommand(ToastCommand.CreateFunc<ToastContext, object>(name, (ctx) => ToastExecutor.Execute(ctx.Toaster, x.Parameters[1], ctx)));
                }, 1);
        
        public static readonly ToastCommand If =
                ToastCommand.CreateFunc<ToastContext, bool, object, object>("if", (ctx, x, y) => x ? y: null);

        public static readonly ToastCommand Else =
                ToastCommand.CreateFunc<CommandNode, ToastContext, object, object>("else", (x, ctx, y) =>
                {
                    if (x.Command != If)
                    {
                        throw new InvalidCommandNodeException("else", x.Command.Name);
                    }

                    if ((bool)ToastExecutor.Execute(ctx.Toaster, x.Parameters[0], ctx, typeof(bool))) 
                    {
                        return ToastExecutor.Execute(ctx.Toaster, x.Parameters[1], ctx);
                    }
                    else
                    {
                        return y;
                    }
                });

        public static readonly ToastCommand Repeat =
                ToastCommand.CreateFunc<object, ToastContext, int, object[]>("repeat", (x, ctx, y) => Enumerable.Repeat(x, y).ToArray());

        public static readonly ToastCommand While =
                ToastCommand.CreateAction<ToastContext, FunctionNode, FunctionNode>("while", (ctx, x, y) =>
                {
                    while ((bool)ctx.Toaster.ExecuteFunction(x, Array.Empty<object>(), ctx))
                    {
                        ctx.Toaster.ExecuteFunction(y, Array.Empty<object>(), ctx);
                    }
                });

        public static readonly ToastCommand For =
                ToastCommand.CreateAction<ToastContext, FunctionNode, FunctionNode, FunctionNode, FunctionNode>("for", (ctx, x, y, z, w) =>
                {
                    for (ctx.Toaster.ExecuteFunction(x, Array.Empty<object>(), ctx); 
                         (bool)ctx.Toaster.ExecuteFunction(y, Array.Empty<object>(), ctx); 
                         ctx.Toaster.ExecuteFunction(z, Array.Empty<object>(), ctx))
                    {
                        ctx.Toaster.ExecuteFunction(w, Array.Empty<object>(), ctx);
                    }
                });

        public static readonly ToastCommand Foreach =
                ToastCommand.CreateAction<ToastContext, object[], FunctionNode>("foreach", (ctx, x, y) =>
                {
                    foreach (object o in x)
                    {
                        ctx.Toaster.ExecuteFunction(y, new[] { o }, ctx);
                    }
                });
        
        public static readonly ToastCommand Print =
                ToastCommand.CreateAction<ToastContext, object>("print", (ctx, x) => Console.WriteLine(x));

        public static readonly ToastCommand Input =
                ToastCommand.CreateFunc<ToastContext, string>("input", (ctx) => Console.ReadLine());

        public static readonly ToastCommand Execute =
                ToastCommand.CreateFunc<ToastContext, FunctionNode, object[], object>("execute", (ctx, x, y) => ctx.Toaster.ExecuteFunction(x, y, ctx));

        public static readonly ToastCommand Member =
                ToastCommand.CreateFunc<int, ToastContext, object[], object>("thOf", (x, ctx, y) => y[x]);

        public static readonly ToastCommand Length =
                ToastCommand.CreateFunc<ToastContext, object[], int>("len", (ctx, x) => x.Length);

        public static readonly ToastCommand IndexOf =
                ToastCommand.CreateFunc<object[], ToastContext, object, int>("indexOf", (x, ctx, y) => 
                {
                    if (y is string s && s.Length == 1)
                    {
                        y = s[0];
                    }

                    return Array.IndexOf(x, y);
                });

        public static readonly ToastCommand Split =
                ToastCommand.CreateFunc<ToastContext, string, string, string[]>("split", (ctx, x, y) => x.Split(y));

        public static readonly ToastCommand Reverse =
                ToastCommand.CreateFunc<ToastContext, string, string>("reverse", (ctx, s) => new string(s.Reverse().ToArray()));

        public static readonly ToastCommand StartsWith =
                ToastCommand.CreateFunc<string, ToastContext, string, bool>("startsWith", (x, ctx, y) => x.StartsWith(y));

        public static readonly ToastCommand EndsWith =
                ToastCommand.CreateFunc<string, ToastContext, string, bool>("endsWith", (x, ctx, y) => x.EndsWith(y));

        public static readonly ToastCommand Contains =
                ToastCommand.CreateFunc<string, ToastContext, string, bool>("contains", (x, ctx, y) => x.Contains(y));

        public static readonly ToastCommand Trim =
                ToastCommand.CreateFunc<ToastContext, string, string>("trim", (ctx, x) => x.Trim());

        public static readonly ToastCommand Substring =
                ToastCommand.CreateFunc<ToastContext, string, int, int, string>("substring", (ctx, x, y, z) => x.Substring(y, z));

        public static readonly ToastCommand Join =
                ToastCommand.CreateFunc<string, ToastContext, string, string>("join", (x, ctx, y) => x + y);

        public static readonly ToastCommand Replace =
                ToastCommand.CreateFunc<string, ToastContext, string, string, string>("replace", (x, ctx, y, z) => x.Replace(y, z));

        public static readonly ToastCommand ToUpper =
                ToastCommand.CreateFunc<string, ToastContext, string>("toUpper", (x, ctx) => x.ToUpper());

        public static readonly ToastCommand ToLower =
                ToastCommand.CreateFunc<string, ToastContext, string>("toLower", (x, ctx) => x.ToLower());

        private BasicCommands() { }
    }
}
