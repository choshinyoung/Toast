using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast
{
    public class BasicConverters
    {
        public static ToastConverter[] All =>
            NumberToNumber.Concat(StringToNumber).Concat(Others).ToArray();

        public static ToastConverter[] NumberToNumber => new ToastConverter[]
        {
            ByteToSbyte,
            SbyteToByte, SbyteToUshort, SbyteToUint, SbyteToUlong,
            ShortToByte, ShortToSbyte, ShortToUshort, ShortToUint, ShortToUlong,
            UshortToByte, UshortToSbyte, UshortToShort,
            IntToByte, IntToSbyte, IntToShort, IntToUshort, IntToUint, IntToUlong,
            UintToByte, UintToSbyte, UintToShort, UintToUshort, UintToInt,
            LongToByte, LongToSbyte, LongToShort, LongToUshort, LongToInt, LongToUint, LongToUlong,
            UlongToByte, UlongToSbyte, UlongToShort, UlongToUshort, UlongToInt, UlongToUint, UlongToLong,
            FloatToByte, FloatToSbyte, FloatToShort, FloatToUshort, FloatToInt, FloatToUint, FloatToLong, FloatToUlong, FloatToDecimal,
            DoubleToByte, DoubleToSbyte, DoubleToShort, DoubleToUshort, DoubleToInt, DoubleToUint, DoubleToLong, DoubleToUlong, DoubleToFloat, DoubleToDecimal,
            DecimalToByte, DecimalToSbyte, DecimalToShort, DecimalToUshort, DecimalToInt, DecimalToUint, DecimalToLong, DecimalToUlong, DecimalToFloat, DecimalToDouble
        };

        public static ToastConverter[] StringToNumber => new ToastConverter[]
        {
            StringToByte, StringToSbyte, StringToShort, StringToUshort, StringToInt, StringToUint, StringToLong, StringToUlong, StringToFloat, StringToDouble, StringToDecimal
        };

        public static ToastConverter[] Others => new ToastConverter[]
        {

        };

        public static readonly ToastConverter ByteToSbyte = ToastConverter.Create<byte, sbyte>(x => (sbyte)x);

        public static readonly ToastConverter SbyteToByte = ToastConverter.Create<sbyte, byte>(x => (byte)x);
        public static readonly ToastConverter SbyteToUshort = ToastConverter.Create<sbyte, ushort>(x => (ushort)x);
        public static readonly ToastConverter SbyteToUint = ToastConverter.Create<sbyte, uint>(x => (uint)x);
        public static readonly ToastConverter SbyteToUlong = ToastConverter.Create<sbyte, ulong>(x => (ulong)x);

        public static readonly ToastConverter ShortToByte = ToastConverter.Create<short, byte>(x => (byte)x);
        public static readonly ToastConverter ShortToSbyte= ToastConverter.Create<short, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter ShortToUshort = ToastConverter.Create<short, ushort>(x => (ushort)x);
        public static readonly ToastConverter ShortToUint = ToastConverter.Create<short, uint>(x => (uint)x);
        public static readonly ToastConverter ShortToUlong = ToastConverter.Create<short, ulong>(x => (ulong)x);

        public static readonly ToastConverter UshortToByte = ToastConverter.Create<ushort, byte>(x => (byte)x);
        public static readonly ToastConverter UshortToSbyte = ToastConverter.Create<ushort, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter UshortToShort = ToastConverter.Create<ushort, short>(x => (short)x);

        public static readonly ToastConverter IntToByte = ToastConverter.Create<int, byte>(x => (byte)x);
        public static readonly ToastConverter IntToSbyte= ToastConverter.Create<int, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter IntToShort = ToastConverter.Create<int, short>(x => (short)x);
        public static readonly ToastConverter IntToUshort = ToastConverter.Create<int, ushort>(x => (ushort)x);
        public static readonly ToastConverter IntToUint = ToastConverter.Create<int, uint>(x => (uint)x);
        public static readonly ToastConverter IntToUlong = ToastConverter.Create<int, ulong>(x => (ulong)x);

        public static readonly ToastConverter UintToByte = ToastConverter.Create<uint, byte>(x => (byte)x);
        public static readonly ToastConverter UintToSbyte = ToastConverter.Create<uint, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter UintToShort = ToastConverter.Create<uint, short>(x => (short)x);
        public static readonly ToastConverter UintToUshort = ToastConverter.Create<uint, ushort>(x => (ushort)x);
        public static readonly ToastConverter UintToInt = ToastConverter.Create<uint, int>(x => (int)x);

        public static readonly ToastConverter LongToByte = ToastConverter.Create<long, byte>(x => (byte)x);
        public static readonly ToastConverter LongToSbyte= ToastConverter.Create<long, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter LongToShort = ToastConverter.Create<long, short>(x => (short)x);
        public static readonly ToastConverter LongToUshort = ToastConverter.Create<long, ushort>(x => (ushort)x);
        public static readonly ToastConverter LongToInt = ToastConverter.Create<long, int>(x => (int)x);
        public static readonly ToastConverter LongToUint = ToastConverter.Create<long, uint>(x => (uint)x);
        public static readonly ToastConverter LongToUlong = ToastConverter.Create<long, ulong>(x => (ulong)x);

        public static readonly ToastConverter UlongToByte = ToastConverter.Create<ulong, byte>(x => (byte)x);
        public static readonly ToastConverter UlongToSbyte = ToastConverter.Create<ulong, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter UlongToShort = ToastConverter.Create<ulong, short>(x => (short)x);
        public static readonly ToastConverter UlongToUshort = ToastConverter.Create<ulong, ushort>(x => (ushort)x);
        public static readonly ToastConverter UlongToInt = ToastConverter.Create<ulong, int>(x => (int)x);
        public static readonly ToastConverter UlongToUint = ToastConverter.Create<ulong, uint>(x => (uint)x);
        public static readonly ToastConverter UlongToLong = ToastConverter.Create<ulong, long>(x => (long)x);

        public static readonly ToastConverter FloatToByte = ToastConverter.Create<float, byte>(x => (byte)x);
        public static readonly ToastConverter FloatToSbyte= ToastConverter.Create<float, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter FloatToShort = ToastConverter.Create<float, short>(x => (short)x);
        public static readonly ToastConverter FloatToUshort = ToastConverter.Create<float, ushort>(x => (ushort)x);
        public static readonly ToastConverter FloatToInt = ToastConverter.Create<float, int>(x => (int)x);
        public static readonly ToastConverter FloatToUint = ToastConverter.Create<float, uint>(x => (uint)x);
        public static readonly ToastConverter FloatToLong = ToastConverter.Create<float, long>(x => (long)x);
        public static readonly ToastConverter FloatToUlong = ToastConverter.Create<float, ulong>(x => (ulong)x);
        public static readonly ToastConverter FloatToDecimal = ToastConverter.Create<float, decimal>(x => (decimal)x);

        public static readonly ToastConverter DoubleToByte = ToastConverter.Create<double, byte>(x => (byte)x);
        public static readonly ToastConverter DoubleToSbyte= ToastConverter.Create<double, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter DoubleToShort = ToastConverter.Create<double, short>(x => (short)x);
        public static readonly ToastConverter DoubleToUshort = ToastConverter.Create<double, ushort>(x => (ushort)x);
        public static readonly ToastConverter DoubleToInt = ToastConverter.Create<double, int>(x => (int)x);
        public static readonly ToastConverter DoubleToUint = ToastConverter.Create<double, uint>(x => (uint)x);
        public static readonly ToastConverter DoubleToLong = ToastConverter.Create<double, long>(x => (long)x);
        public static readonly ToastConverter DoubleToUlong = ToastConverter.Create<double, ulong>(x => (ulong)x);
        public static readonly ToastConverter DoubleToFloat = ToastConverter.Create<double, float>(x => (float)x);
        public static readonly ToastConverter DoubleToDecimal = ToastConverter.Create<double, decimal>(x => (decimal)x);

        public static readonly ToastConverter DecimalToByte = ToastConverter.Create<decimal, byte>(x => (byte)x);
        public static readonly ToastConverter DecimalToSbyte= ToastConverter.Create<decimal, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter DecimalToShort = ToastConverter.Create<decimal, short>(x => (short)x);
        public static readonly ToastConverter DecimalToUshort = ToastConverter.Create<decimal, ushort>(x => (ushort)x);
        public static readonly ToastConverter DecimalToInt = ToastConverter.Create<decimal, int>(x => (int)x);
        public static readonly ToastConverter DecimalToUint = ToastConverter.Create<decimal, uint>(x => (uint)x);
        public static readonly ToastConverter DecimalToLong = ToastConverter.Create<decimal, long>(x => (long)x);
        public static readonly ToastConverter DecimalToUlong = ToastConverter.Create<decimal, ulong>(x => (ulong)x);
        public static readonly ToastConverter DecimalToFloat = ToastConverter.Create<decimal, float>(x => (float)x);
        public static readonly ToastConverter DecimalToDouble = ToastConverter.Create<decimal, double>(x => (double)x);

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

        private BasicConverters() { }
    }
}