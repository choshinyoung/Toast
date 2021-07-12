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
        public static ToastCommand[] All => new ToastCommand[]
        {
            Addition, Subtraction, Multiplication, Division, Modulus, Exponentiation, FloorDivision,
            Equal, Greater, Less, GreaterOrEqual, LessOrEqual,
            And, Or, Not,
            BitwiseAnd, BitwiseOr, BitwiseXor, BitwiseNot, LeftShift, RightShift,
            True, False, Null
        };

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
                ToastCommand.Create<int, int, int>("floordiv", (x, y) => x / y);

        public static readonly ToastCommand Equal =
                ToastCommand.Create<object, object, bool>("equal", (x, y) => x == y);

        public static readonly ToastCommand Greater =
                ToastCommand.Create<float, float, bool>("greater", (x, y) => x > y);

        public static readonly ToastCommand Less =
                ToastCommand.Create<float, float, bool>("less", (x, y) => x < y);

        public static readonly ToastCommand GreaterOrEqual =
                ToastCommand.Create<float, float, bool>("greaterorequal", (x, y) => x >= y);

        public static readonly ToastCommand LessOrEqual =
                ToastCommand.Create<float, float, bool>("lessorequal", (x, y) => x <= y);

        public static readonly ToastCommand And =
                ToastCommand.Create<bool, bool, bool>("and", (x, y) => x && y);

        public static readonly ToastCommand Or =
                ToastCommand.Create<bool, bool, bool>("or", (x, y) => x || y);

        public static readonly ToastCommand Not =
                ToastCommand.Create<bool, bool>("not", x => !x);

        public static readonly ToastCommand BitwiseAnd =
                ToastCommand.Create<int, int, int>("bitand", (x, y) => x & y);

        public static readonly ToastCommand BitwiseOr =
                ToastCommand.Create<int, int, int>("bitor", (x, y) => x | y);

        public static readonly ToastCommand BitwiseXor =
                ToastCommand.Create<int, int, int>("bitxor", (x, y) => x ^ y);

        public static readonly ToastCommand BitwiseNot =
                ToastCommand.Create<int, int>("bitnot", x => ~x);

        public static readonly ToastCommand LeftShift =
                ToastCommand.Create<int, int, int>("lshift", (x, y) => x << y);

        public static readonly ToastCommand RightShift =
                ToastCommand.Create<int, int, int>("rshift", (x, y) => x >> y);

        public static readonly ToastCommand True =
            ToastCommand.Create<bool>("true", () => true);

        public static readonly ToastCommand False =
            ToastCommand.Create<bool>("false", () => false);

        public static readonly ToastCommand Null =
            ToastCommand.Create<object>("null", () => null);

        private BasicCommands() { }
    }
}
