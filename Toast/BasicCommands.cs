using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static ToastCommand[] Strings => new ToastCommand[]
        {
            Split, Reverse, StartsWith, EndsWith, Contains
        };

        public static readonly ToastCommand True =
            ToastCommand.Create("true", () => true);

        public static readonly ToastCommand False =
            ToastCommand.Create("false", () => false);

        public static readonly ToastCommand Null =
            ToastCommand.Create<object>("null", () => null);

        public static readonly ToastCommand Addition =
                ToastCommand.Create<float, float, float>("add", (x, y) => x + y);

        public static readonly ToastCommand Subtraction =
                ToastCommand.Create<float, float, float>("sub", (x, y) => x - y);

        public static readonly ToastCommand Multiplication =
                ToastCommand.Create<float, float, float>("mul", (x, y) => x * y);

        public static readonly ToastCommand Division =
                ToastCommand.Create<float, float, float>("div", (x, y) => x / y);

        public static readonly ToastCommand Modulus =
                ToastCommand.Create<float, float, float>("mod", (x, y) => x % y);

        public static readonly ToastCommand Exponentiation =
                ToastCommand.Create<float, float, float>("exp", (x, y) => MathF.Pow(x, y));

        public static readonly ToastCommand FloorDivision =
                ToastCommand.Create<int, int, int>("floorDiv", (x, y) => x / y);

        public static readonly ToastCommand Equal =
                ToastCommand.Create<object, object, bool>("equal", (x, y) => x == y);

        public static readonly ToastCommand Greater =
                ToastCommand.Create<float, float, bool>("greater", (x, y) => x > y);

        public static readonly ToastCommand Less =
                ToastCommand.Create<float, float, bool>("less", (x, y) => x < y);

        public static readonly ToastCommand GreaterOrEqual =
                ToastCommand.Create<float, float, bool>("greaterEqual", (x, y) => x >= y);

        public static readonly ToastCommand LessOrEqual =
                ToastCommand.Create<float, float, bool>("lessEqual", (x, y) => x <= y);

        public static readonly ToastCommand And =
                ToastCommand.Create<bool, bool, bool>("and", (x, y) => x && y);

        public static readonly ToastCommand Or =
                ToastCommand.Create<bool, bool, bool>("or", (x, y) => x || y);

        public static readonly ToastCommand Not =
                ToastCommand.Create<bool, bool>("not", x => !x);

        public static readonly ToastCommand BitwiseAnd =
                ToastCommand.Create<int, int, int>("bitAnd", (x, y) => x & y);

        public static readonly ToastCommand BitwiseOr =
                ToastCommand.Create<int, int, int>("bitOr", (x, y) => x | y);

        public static readonly ToastCommand BitwiseXor =
                ToastCommand.Create<int, int, int>("bitXor", (x, y) => x ^ y);

        public static readonly ToastCommand BitwiseNot =
                ToastCommand.Create<int, int>("bitNot", x => ~x);

        public static readonly ToastCommand LeftShift =
                ToastCommand.Create<int, int, int>("lShift", (x, y) => x << y);

        public static readonly ToastCommand RightShift =
                ToastCommand.Create<int, int, int>("rShift", (x, y) => x >> y);

        public static readonly ToastCommand If =
                ToastCommand.Create<bool, object, object>("if", (x, y) => x ? y: null);

        public static readonly ToastCommand IfElse =
                ToastCommand.Create<bool, object, object, object>("ifElse", (x, y, z) => x ? y : z);

        public static readonly ToastCommand Repeat =
                ToastCommand.Create<int, object, object[]>("repeat", (x, y) => Enumerable.Repeat(y, x).ToArray());

        public static readonly ToastCommand Print =
                ToastCommand.Create<object>("print", Console.WriteLine);

        public static readonly ToastCommand Input =
                ToastCommand.Create("input", Console.ReadLine);

        /* public static readonly ToastCommand Converter =
                ToastCommand.Create<object, string, object>("convert", (x, y) =>
                {
                    Type t = Type.GetType(y);
                    if (t is null)
                        throw new Exception($"Cannot found a type named '{y}'.");

                    return Convert.ChangeType(x, t);
                }); */

        public static readonly ToastCommand Member =
                ToastCommand.Create<object[], int, object>("member", (x, y) => x[y]);

        public static readonly ToastCommand Count =
                ToastCommand.Create<object[], int>("count", x => x.Length);

        public static readonly ToastCommand Split =
                ToastCommand.Create<string, char, string[]>("split", (x, y) => x.Split(y));

        public static readonly ToastCommand Reverse =
                ToastCommand.Create<string, string>("reverse", s => new string(s.Reverse().ToArray()));

        public static readonly ToastCommand StartsWith =
                ToastCommand.Create<string, char, bool>("startsWith", (x, y) => x.StartsWith(y));

        public static readonly ToastCommand EndsWith =
                ToastCommand.Create<string, char, bool>("endsWith", (x, y) => x.EndsWith(y));

        public static readonly ToastCommand Contains =
                ToastCommand.Create<string, string, bool>("contains", (x, y) => x.Contains(y));

        private BasicCommands() { }
    }
}
