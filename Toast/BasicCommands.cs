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
            Repeat,
        };

        public static ToastCommand[] Others => new ToastCommand[]
        {
            Print, Input
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
            ToastCommand.Create<ToastContext, bool>("true", (ctx) => true);

        public static readonly ToastCommand False =
            ToastCommand.Create<ToastContext, bool>("false", (ctx) => false);

        public static readonly ToastCommand Null =
            ToastCommand.Create<ToastContext, object>("null", (ctx) => null);

        public static readonly ToastCommand Addition =
                ToastCommand.Create<ToastContext, float, float, float>("add", (ctx, x, y) => x + y);

        public static readonly ToastCommand Subtraction =
                ToastCommand.Create<ToastContext, float, float, float>("sub", (ctx, x, y) => x - y);

        public static readonly ToastCommand Multiplication =
                ToastCommand.Create<ToastContext, float, float, float>("mul", (ctx, x, y) => x * y);

        public static readonly ToastCommand Division =
                ToastCommand.Create<ToastContext, float, float, float>("div", (ctx, x, y) => x / y);

        public static readonly ToastCommand Modulus =
                ToastCommand.Create<ToastContext, float, float, float>("mod", (ctx, x, y) => x % y);

        public static readonly ToastCommand Exponentiation =
                ToastCommand.Create<ToastContext, float, float, float>("exp", (ctx, x, y) => MathF.Pow(x, y));

        public static readonly ToastCommand FloorDivision =
                ToastCommand.Create<ToastContext, int, int, int>("floorDiv", (ctx, x, y) => x / y);

        public static readonly ToastCommand Equal =
                ToastCommand.Create<ToastContext, object, object, bool>("equal", (ctx, x, y) => x == y);

        public static readonly ToastCommand Greater =
                ToastCommand.Create<ToastContext, float, float, bool>("greater", (ctx, x, y) => x > y);

        public static readonly ToastCommand Less =
                ToastCommand.Create<ToastContext, float, float, bool>("less", (ctx, x, y) => x < y);

        public static readonly ToastCommand GreaterOrEqual =
                ToastCommand.Create<ToastContext, float, float, bool>("greaterEqual", (ctx, x, y) => x >= y);

        public static readonly ToastCommand LessOrEqual =
                ToastCommand.Create<ToastContext, float, float, bool>("lessEqual", (ctx, x, y) => x <= y);

        public static readonly ToastCommand And =
                ToastCommand.Create<ToastContext, bool, bool, bool>("and", (ctx, x, y) => x && y);

        public static readonly ToastCommand Or =
                ToastCommand.Create<ToastContext, bool, bool, bool>("or", (ctx, x, y) => x || y);

        public static readonly ToastCommand Not =
                ToastCommand.Create<ToastContext, bool, bool>("not", (ctx, x) => !x);

        public static readonly ToastCommand BitwiseAnd =
                ToastCommand.Create<ToastContext, int, int, int>("bitAnd", (ctx, x, y) => x & y);

        public static readonly ToastCommand BitwiseOr =
                ToastCommand.Create<ToastContext, int, int, int>("bitOr", (ctx, x, y) => x | y);

        public static readonly ToastCommand BitwiseXor =
                ToastCommand.Create<ToastContext, int, int, int>("bitXor", (ctx, x, y) => x ^ y);

        public static readonly ToastCommand BitwiseNot =
                ToastCommand.Create<ToastContext, int, int>("bitNot", (ctx, x) => ~x);

        public static readonly ToastCommand LeftShift =
                ToastCommand.Create<ToastContext, int, int, int>("lShift", (ctx, x, y) => x << y);

        public static readonly ToastCommand RightShift =
                ToastCommand.Create<ToastContext, int, int, int>("rShift", (ctx, x, y) => x >> y);

        public static readonly ToastCommand If =
                ToastCommand.Create<ToastContext, bool, object, object>("if", (ctx, x, y) => x ? y: null);

        public static readonly ToastCommand IfElse =
                ToastCommand.Create<ToastContext, bool, object, object, object>("ifElse", (ctx, x, y, z) => x ? y : z);

        public static readonly ToastCommand Repeat =
                ToastCommand.Create<ToastContext, int, object, object[]>("repeat", (ctx, x, y) => Enumerable.Repeat(y, x).ToArray());

        public static readonly ToastCommand Print =
                ToastCommand.Create<ToastContext, object>("print", (ctx, x) => Console.WriteLine(x));

        public static readonly ToastCommand Input =
                ToastCommand.Create<ToastContext, string>("input", (ctx) => Console.ReadLine());

        /* public static readonly ToastCommand Converter =
                ToastCommand.Create<object, string, object>("convert", (x, y) =>
                {
                    Type t = Type.GetType(y);
                    if (t is null)
                        throw new Exception($"Cannot found a type named '{y}'.");

                    return Convert.ChangeType(x, t);
                }); */

        public static readonly ToastCommand Member =
                ToastCommand.Create<ToastContext, object[], int, object>("member", (ctx, x, y) => x[y]);

        public static readonly ToastCommand Count =
                ToastCommand.Create<ToastContext, object[], int>("count", (ctx, x) => x.Length);

        public static readonly ToastCommand Split =
                ToastCommand.Create<ToastContext, string, char, string[]>("split", (ctx, x, y) => x.Split(y));

        public static readonly ToastCommand Reverse =
                ToastCommand.Create<ToastContext, string, string>("reverse", (ctx, s) => new string(s.Reverse().ToArray()));

        public static readonly ToastCommand StartsWith =
                ToastCommand.Create<ToastContext, string, char, bool>("startsWith", (ctx, x, y) => x.StartsWith(y));

        public static readonly ToastCommand EndsWith =
                ToastCommand.Create<ToastContext, string, char, bool>("endsWith", (ctx, x, y) => x.EndsWith(y));

        public static readonly ToastCommand Contains =
                ToastCommand.Create<ToastContext, string, string, bool>("contains", (ctx, x, y) => x.Contains(y));

        public static readonly ToastCommand Execute =
                ToastCommand.Create<ToastContext, Function, object>("execute", (ctx, x) =>
                {
                    foreach(Element[] line in x.GetValue())
                    {
                        if (line[0] is not Command)
                        {
                            throw new InvalidCommandLineException($"{line[0].GetValue()}..");
                        }

                        ctx.Toaster.ExecuteParsedLine(line);
                    }

                    return null;
                });

        private BasicCommands() { }
    }
}
