using System.Linq;
using Toast.Exceptions;

namespace Toast
{
    public class BasicConverters
    {
        public static ToastConverter[] All =>
            StringToNumber.Concat(Others).ToArray();

        public static ToastConverter[] StringToNumber => new ToastConverter[]
        {
            StringToByte, StringToSbyte, StringToShort, StringToUshort, StringToInt, StringToUint, StringToLong, StringToUlong, StringToFloat, StringToDouble, StringToDecimal
        };

        public static ToastConverter[] Others => new ToastConverter[]
        {
            StringToObjectArray, StringToChar,
        };

        public static readonly ToastConverter StringToByte = ToastConverter.Create<string, byte>((ctx, x) => byte.Parse(x));
        public static readonly ToastConverter StringToSbyte = ToastConverter.Create<string, sbyte>((ctx, x) => sbyte.Parse(x));
        public static readonly ToastConverter StringToShort = ToastConverter.Create<string, short>((ctx, x) => short.Parse(x));
        public static readonly ToastConverter StringToUshort = ToastConverter.Create<string, ushort>((ctx, x) => ushort.Parse(x));
        public static readonly ToastConverter StringToInt = ToastConverter.Create<string, int>((ctx, x) => int.Parse(x));
        public static readonly ToastConverter StringToUint = ToastConverter.Create<string, uint>((ctx, x) => uint.Parse(x));
        public static readonly ToastConverter StringToLong = ToastConverter.Create<string, long>((ctx, x) => long.Parse(x));
        public static readonly ToastConverter StringToUlong = ToastConverter.Create<string, ulong>((ctx, x) => ulong.Parse(x));
        public static readonly ToastConverter StringToFloat = ToastConverter.Create<string, float>((ctx, x) => float.Parse(x));
        public static readonly ToastConverter StringToDouble = ToastConverter.Create<string, double>((ctx, x) => double.Parse(x));
        public static readonly ToastConverter StringToDecimal = ToastConverter.Create<string, decimal>((ctx, x) => decimal.Parse(x));

        public static readonly ToastConverter StringToObjectArray = ToastConverter.Create<string, object[]>((ctx, x) => x.Select(_x => (object)_x).ToArray());
        public static readonly ToastConverter StringToChar = ToastConverter.Create<string, char>((ctx, x) => x.Length == 1 ? x[0] : throw new ParameterConvertException(typeof(string), typeof(char)));

        private BasicConverters() { }
    }
}