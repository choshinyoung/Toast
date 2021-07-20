using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toast.Elements;
using Toast.Exceptions;

namespace Toast
{
    public class BasicCommands
    {
        public static ToastCommand[] All =>
            Literals.Concat(Operators).Concat(Statements).Concat(Others).Concat(Lists).Concat(Strings).Concat(Functions).ToArray();

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
            If, IfElse,
            Repeat, While, For, Foreach
        };

        public static ToastCommand[] Others => new ToastCommand[]
        {
            Print, Input, Assign
        };

        public static ToastCommand[] Lists => new ToastCommand[]
        {
            Member, Count
        };

        public static ToastCommand[] Functions => new ToastCommand[]
        {
            Execute
        };

        public static ToastCommand[] Strings => new ToastCommand[]
        {
            Split, Reverse, StartsWith, EndsWith, Contains
        };

        public static readonly ToastCommand True =
            ToastCommand.CreateFunc<ToastContext, bool>("true", (ctx) => true);

        public static readonly ToastCommand False =
            ToastCommand.CreateFunc<ToastContext, bool>("false", (ctx) => false);

        public static readonly ToastCommand Null =
            ToastCommand.CreateFunc<ToastContext, object>("null", (ctx) => null);

        public static readonly ToastCommand Addition =
                ToastCommand.CreateFunc<float, ToastContext, float, float>("add", (x, ctx, y) => x + y, 1);

        public static readonly ToastCommand Subtraction =
                ToastCommand.CreateFunc<float, ToastContext, float, float>("sub", (x, ctx, y) => x - y, 1);

        public static readonly ToastCommand Multiplication =
                ToastCommand.CreateFunc<float, ToastContext, float, float>("mul", (x, ctx, y) => x * y, 2);

        public static readonly ToastCommand Division =
                ToastCommand.CreateFunc<float, ToastContext, float, float>("div", (x, ctx, y) => x / y, 2);

        public static readonly ToastCommand Modulus =
                ToastCommand.CreateFunc<float, ToastContext, float, float>("mod", (x, ctx, y) => x % y, 2);

        public static readonly ToastCommand Exponentiation =
                ToastCommand.CreateFunc<ToastContext, float, float, float>("exp", (ctx, x, y) => MathF.Pow(x, y));

        public static readonly ToastCommand FloorDivision =
                ToastCommand.CreateFunc<ToastContext, int, int, int>("floorDiv", (ctx, x, y) => x / y);

        public static readonly ToastCommand Equal =
                ToastCommand.CreateFunc<ToastContext, object, object, bool>("equal", (ctx, x, y) => x.Equals(y));

        public static readonly ToastCommand Greater =
                ToastCommand.CreateFunc<ToastContext, float, float, bool>("greater", (ctx, x, y) => x > y);

        public static readonly ToastCommand Less =
                ToastCommand.CreateFunc<ToastContext, float, float, bool>("less", (ctx, x, y) => x < y);

        public static readonly ToastCommand GreaterOrEqual =
                ToastCommand.CreateFunc<ToastContext, float, float, bool>("greaterEqual", (ctx, x, y) => x >= y);

        public static readonly ToastCommand LessOrEqual =
                ToastCommand.CreateFunc<ToastContext, float, float, bool>("lessEqual", (ctx, x, y) => x <= y);

        public static readonly ToastCommand And =
                ToastCommand.CreateFunc<ToastContext, bool, bool, bool>("and", (ctx, x, y) => x && y);

        public static readonly ToastCommand Or =
                ToastCommand.CreateFunc<ToastContext, bool, bool, bool>("or", (ctx, x, y) => x || y);

        public static readonly ToastCommand Not =
                ToastCommand.CreateFunc<ToastContext, bool, bool>("not", (ctx, x) => !x);

        public static readonly ToastCommand BitwiseAnd =
                ToastCommand.CreateFunc<ToastContext, int, int, int>("bitAnd", (ctx, x, y) => x & y);

        public static readonly ToastCommand BitwiseOr =
                ToastCommand.CreateFunc<ToastContext, int, int, int>("bitOr", (ctx, x, y) => x | y);

        public static readonly ToastCommand BitwiseXor =
                ToastCommand.CreateFunc<ToastContext, int, int, int>("bitXor", (ctx, x, y) => x ^ y);

        public static readonly ToastCommand BitwiseNot =
                ToastCommand.CreateFunc<ToastContext, int, int>("bitNot", (ctx, x) => ~x);

        public static readonly ToastCommand LeftShift =
                ToastCommand.CreateFunc<ToastContext, int, int, int>("lShift", (ctx, x, y) => x << y);

        public static readonly ToastCommand RightShift =
                ToastCommand.CreateFunc<ToastContext, int, int, int>("rShift", (ctx, x, y) => x >> y);

        public static readonly ToastCommand If =
                ToastCommand.CreateFunc<ToastContext, bool, object, object>("if", (ctx, x, y) => x ? y: null);

        public static readonly ToastCommand IfElse =
                ToastCommand.CreateFunc<ToastContext, bool, object, object, object>("ifElse", (ctx, x, y, z) => x ? y : z);

        public static readonly ToastCommand Repeat =
                ToastCommand.CreateFunc<ToastContext, int, object, object[]>("repeat", (ctx, x, y) => Enumerable.Repeat(y, x).ToArray());

        public static readonly ToastCommand While =
                ToastCommand.CreateAction<ToastContext, Function, Function>("while", (ctx, x, y) =>
                {
                    while ((bool)ctx.Toaster.ExecuteFunction(x, Array.Empty<object>()))
                    {
                        ctx.Toaster.ExecuteFunction(y, Array.Empty<object>());
                    }
                });

        public static readonly ToastCommand For =
                ToastCommand.CreateAction<ToastContext, Function, Function, Function, Function>("for", (ctx, x, y, z, w) =>
                {
                    for (ctx.Toaster.ExecuteFunction(x, Array.Empty<object>()); 
                         (bool)ctx.Toaster.ExecuteFunction(y, Array.Empty<object>()); 
                         ctx.Toaster.ExecuteFunction(z, Array.Empty<object>()))
                    {
                        ctx.Toaster.ExecuteFunction(w, Array.Empty<object>());
                    }
                });

        public static readonly ToastCommand Foreach =
                ToastCommand.CreateAction<ToastContext, object[], Function>("foreach", (ctx, x, y) =>
                {
                    foreach (object o in x)
                    {
                        ctx.Toaster.ExecuteFunction(y, new[] { o });
                    }
                });
        
        public static readonly ToastCommand Print =
                ToastCommand.CreateAction<ToastContext, object>("print", (ctx, x) => Console.WriteLine(x));

        public static readonly ToastCommand Input =
                ToastCommand.CreateFunc<ToastContext, string>("input", (ctx) => Console.ReadLine());

        /* public static readonly ToastCommand Converter =
                ToastCommand.Create<object, string, object>("convert", (x, y) =>
                {
                    Type t = Type.GetType(y);
                    if (t is null)
                        throw new Exception($"Cannot found a type named '{y}'.");

                    return Convert.ChangeType(x, t);
                }); */

        public static readonly ToastCommand Member =
                ToastCommand.CreateFunc<ToastContext, object[], int, object>("member", (ctx, x, y) => x[y]);

        public static readonly ToastCommand Count =
                ToastCommand.CreateFunc<ToastContext, object[], int>("count", (ctx, x) => x.Length);

        public static readonly ToastCommand Split =
                ToastCommand.CreateFunc<ToastContext, string, char, string[]>("split", (ctx, x, y) => x.Split(y));

        public static readonly ToastCommand Reverse =
                ToastCommand.CreateFunc<ToastContext, string, string>("reverse", (ctx, s) => new string(s.Reverse().ToArray()));

        public static readonly ToastCommand StartsWith =
                ToastCommand.CreateFunc<ToastContext, string, char, bool>("startsWith", (ctx, x, y) => x.StartsWith(y));

        public static readonly ToastCommand EndsWith =
                ToastCommand.CreateFunc<ToastContext, string, char, bool>("endsWith", (ctx, x, y) => x.EndsWith(y));

        public static readonly ToastCommand Contains =
                ToastCommand.CreateFunc<ToastContext, string, string, bool>("contains", (ctx, x, y) => x.Contains(y));

        public static readonly ToastCommand Execute =
                ToastCommand.CreateFunc<ToastContext, Function, object[], object>("execute", (ctx, x, y) => ctx.Toaster.ExecuteFunction(x, y));

        public static readonly ToastCommand Assign =
                ToastCommand.CreateAction<ToastContext, Variable, object>("var", (ctx, x, y) =>
                {
                    ToastCommand cmd = ctx.Toaster.GetCommands().ToList().Find(c => c.Name == x.GetValue());
                    if (cmd is not null)
                    {
                        ctx.Toaster.RemoveCommand(cmd);
                    }

                    ctx.Toaster.AddCommand(ToastCommand.CreateFunc<ToastContext, object>(x.GetValue(), (ctx) => y));
                });

        private BasicCommands() { }
    }
}
