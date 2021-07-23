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
            ObjectToString,
            StringToObjectArray, StringToChar,
        };

        public static readonly ToastConverter StringToByte = ToastConverter.Create<string, byte>(byte.Parse);
        public static readonly ToastConverter StringToSbyte = ToastConverter.Create<string, sbyte>(sbyte.Parse);
        public static readonly ToastConverter StringToShort = ToastConverter.Create<string, short>(short.Parse);
        public static readonly ToastConverter StringToUshort = ToastConverter.Create<string, ushort>(ushort.Parse);
        public static readonly ToastConverter StringToInt = ToastConverter.Create<string, int>(int.Parse);
        public static readonly ToastConverter StringToUint = ToastConverter.Create<string, uint>(uint.Parse);
        public static readonly ToastConverter StringToLong = ToastConverter.Create<string, long>(long.Parse);
        public static readonly ToastConverter StringToUlong = ToastConverter.Create<string, ulong>(ulong.Parse);
        public static readonly ToastConverter StringToFloat = ToastConverter.Create<string, float>(float.Parse);
        public static readonly ToastConverter StringToDouble = ToastConverter.Create<string, double>(double.Parse);
        public static readonly ToastConverter StringToDecimal = ToastConverter.Create<string, decimal>(decimal.Parse);

        public static readonly ToastConverter ObjectToString = ToastConverter.Create<object, string>(x => x.ToString());

        public static readonly ToastConverter StringToObjectArray = ToastConverter.Create<string, object[]>(x => x.Select(_x => (object)_x).ToArray());
        public static readonly ToastConverter StringToChar = ToastConverter.Create<string, char>(x => x.Length == 1 ? x[0] : throw new ParameterConvertException(typeof(string), typeof(char)));

        private BasicConverters() { }
    }
}