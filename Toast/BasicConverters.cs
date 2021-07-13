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
            NumberToNumber.Concat(NumberToString).Concat(StringToNumber).Concat(Others).ToArray();

        public static ToastConverter[] NumberToNumber => new ToastConverter[]
        {

        };

        public static ToastConverter[] NumberToString => new ToastConverter[]
        {

        };

        public static ToastConverter[] StringToNumber => new ToastConverter[]
        {

        };

        public static ToastConverter[] Others => new ToastConverter[]
        {

        };

        public static readonly ToastConverter ByteToSbyte = ToastConverter.Create<byte, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter ByteToShort = ToastConverter.Create<byte, short>(x => (short)x);
        public static readonly ToastConverter ByteToUshort = ToastConverter.Create<byte, ushort>(x => (ushort)x);
        public static readonly ToastConverter ByteToInt = ToastConverter.Create<byte, int>(x => (int)x);
        public static readonly ToastConverter ByteToUint = ToastConverter.Create<byte, uint>(x => (uint)x);
        public static readonly ToastConverter ByteToLong = ToastConverter.Create<byte, long>(x => (long)x);
        public static readonly ToastConverter ByteToUlong = ToastConverter.Create<byte, ulong>(x => (ulong)x);
        public static readonly ToastConverter ByteToFloat = ToastConverter.Create<byte, float>(x => (float)x);
        public static readonly ToastConverter ByteToDouble = ToastConverter.Create<byte, double>(x => (double)x);
        public static readonly ToastConverter ByteToDecimal = ToastConverter.Create<byte, decimal>(x => (decimal)x);

        public static readonly ToastConverter SbyteToByte = ToastConverter.Create<sbyte, byte>(x => (byte)x);
        public static readonly ToastConverter SbyteToShort = ToastConverter.Create<sbyte, short>(x => (short)x);
        public static readonly ToastConverter SbyteToUshort = ToastConverter.Create<sbyte, ushort>(x => (ushort)x);
        public static readonly ToastConverter SbyteToInt = ToastConverter.Create<sbyte, int>(x => (int)x);
        public static readonly ToastConverter SbyteToUint = ToastConverter.Create<sbyte, uint>(x => (uint)x);
        public static readonly ToastConverter SbyteToLong = ToastConverter.Create<sbyte, long>(x => (long)x);
        public static readonly ToastConverter SbyteToUlong = ToastConverter.Create<sbyte, ulong>(x => (ulong)x);
        public static readonly ToastConverter SbyteToFloat = ToastConverter.Create<sbyte, float>(x => (float)x);
        public static readonly ToastConverter SbyteToDouble = ToastConverter.Create<sbyte, double>(x => (double)x);
        public static readonly ToastConverter SbyteToDecimal = ToastConverter.Create<sbyte, decimal>(x => (decimal)x);

        public static readonly ToastConverter ShortTobyte = ToastConverter.Create<short, byte>(x => (byte)x);
        public static readonly ToastConverter ShortTosbyte= ToastConverter.Create<short, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter ShortToUshort = ToastConverter.Create<short, ushort>(x => (ushort)x);
        public static readonly ToastConverter ShortToInt = ToastConverter.Create<short, int>(x => (int)x);
        public static readonly ToastConverter ShortToUint = ToastConverter.Create<short, uint>(x => (uint)x);
        public static readonly ToastConverter ShortToLong = ToastConverter.Create<short, long>(x => (long)x);
        public static readonly ToastConverter ShortToUlong = ToastConverter.Create<short, ulong>(x => (ulong)x);
        public static readonly ToastConverter ShortToFloat = ToastConverter.Create<short, float>(x => (float)x);
        public static readonly ToastConverter ShortToDouble = ToastConverter.Create<short, double>(x => (double)x);
        public static readonly ToastConverter ShortToDecimal = ToastConverter.Create<short, decimal>(x => (decimal)x);

        public static readonly ToastConverter UshortToByte = ToastConverter.Create<ushort, byte>(x => (byte)x);
        public static readonly ToastConverter UshortToSbyte = ToastConverter.Create<ushort, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter UshortToShort = ToastConverter.Create<ushort, short>(x => (short)x);
        public static readonly ToastConverter UshortToInt = ToastConverter.Create<ushort, int>(x => (int)x);
        public static readonly ToastConverter UshortToUint = ToastConverter.Create<ushort, uint>(x => (uint)x);
        public static readonly ToastConverter UshortToLong = ToastConverter.Create<ushort, long>(x => (long)x);
        public static readonly ToastConverter UshortToUlong = ToastConverter.Create<ushort, ulong>(x => (ulong)x);
        public static readonly ToastConverter UshortToFloat = ToastConverter.Create<ushort, float>(x => (float)x);
        public static readonly ToastConverter UshortToDouble = ToastConverter.Create<ushort, double>(x => (double)x);
        public static readonly ToastConverter UshortToDecimal = ToastConverter.Create<ushort, decimal>(x => (decimal)x);

        public static readonly ToastConverter IntTobyte = ToastConverter.Create<int, byte>(x => (byte)x);
        public static readonly ToastConverter IntTosbyte= ToastConverter.Create<int, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter IntToShort = ToastConverter.Create<int, short>(x => (short)x);
        public static readonly ToastConverter IntToUshort = ToastConverter.Create<int, ushort>(x => (ushort)x);
        public static readonly ToastConverter IntToUint = ToastConverter.Create<int, uint>(x => (uint)x);
        public static readonly ToastConverter IntToLong = ToastConverter.Create<int, long>(x => (long)x);
        public static readonly ToastConverter IntToUlong = ToastConverter.Create<int, ulong>(x => (ulong)x);
        public static readonly ToastConverter IntToFloat = ToastConverter.Create<int, float>(x => (float)x);
        public static readonly ToastConverter IntToDouble = ToastConverter.Create<int, double>(x => (double)x);
        public static readonly ToastConverter IntToDecimal = ToastConverter.Create<int, decimal>(x => (decimal)x);

        public static readonly ToastConverter UintToByte = ToastConverter.Create<uint, byte>(x => (byte)x);
        public static readonly ToastConverter UintToSbyte = ToastConverter.Create<uint, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter UintToShort = ToastConverter.Create<uint, short>(x => (short)x);
        public static readonly ToastConverter UintToUshort = ToastConverter.Create<uint, ushort>(x => (ushort)x);
        public static readonly ToastConverter UintToInt = ToastConverter.Create<uint, int>(x => (int)x);
        public static readonly ToastConverter UintToLong = ToastConverter.Create<uint, long>(x => (long)x);
        public static readonly ToastConverter UintToUlong = ToastConverter.Create<uint, ulong>(x => (ulong)x);
        public static readonly ToastConverter UintToFloat = ToastConverter.Create<uint, float>(x => (float)x);
        public static readonly ToastConverter UintToDouble = ToastConverter.Create<uint, double>(x => (double)x);
        public static readonly ToastConverter UintToDecimal = ToastConverter.Create<uint, decimal>(x => (decimal)x);

        public static readonly ToastConverter LongTobyte = ToastConverter.Create<long, byte>(x => (byte)x);
        public static readonly ToastConverter LongTosbyte= ToastConverter.Create<long, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter LongToShort = ToastConverter.Create<long, short>(x => (short)x);
        public static readonly ToastConverter LongToUshort = ToastConverter.Create<long, ushort>(x => (ushort)x);
        public static readonly ToastConverter LongToInt = ToastConverter.Create<long, int>(x => (int)x);
        public static readonly ToastConverter LongToUint = ToastConverter.Create<long, uint>(x => (uint)x);
        public static readonly ToastConverter LongToUlong = ToastConverter.Create<long, ulong>(x => (ulong)x);
        public static readonly ToastConverter LongToFloat = ToastConverter.Create<long, float>(x => (float)x);
        public static readonly ToastConverter LongToDouble = ToastConverter.Create<long, double>(x => (double)x);
        public static readonly ToastConverter LongToDecimal = ToastConverter.Create<long, decimal>(x => (decimal)x);

        public static readonly ToastConverter UlongToByte = ToastConverter.Create<ulong, byte>(x => (byte)x);
        public static readonly ToastConverter UlongToSbyte = ToastConverter.Create<ulong, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter UlongToShort = ToastConverter.Create<ulong, short>(x => (short)x);
        public static readonly ToastConverter UlongToUshort = ToastConverter.Create<ulong, ushort>(x => (ushort)x);
        public static readonly ToastConverter UlongToInt = ToastConverter.Create<ulong, int>(x => (int)x);
        public static readonly ToastConverter UlongToUint = ToastConverter.Create<ulong, uint>(x => (uint)x);
        public static readonly ToastConverter UlongToLong = ToastConverter.Create<ulong, long>(x => (long)x);
        public static readonly ToastConverter UlongToFloat = ToastConverter.Create<ulong, float>(x => (float)x);
        public static readonly ToastConverter UlongToDouble = ToastConverter.Create<ulong, double>(x => (double)x);
        public static readonly ToastConverter UlongToDecimal = ToastConverter.Create<ulong, decimal>(x => (decimal)x);

        public static readonly ToastConverter FloatTobyte = ToastConverter.Create<float, byte>(x => (byte)x);
        public static readonly ToastConverter FloatTosbyte= ToastConverter.Create<float, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter FloatToShort = ToastConverter.Create<float, short>(x => (short)x);
        public static readonly ToastConverter FloatToUshort = ToastConverter.Create<float, ushort>(x => (ushort)x);
        public static readonly ToastConverter FloatToInt = ToastConverter.Create<float, int>(x => (int)x);
        public static readonly ToastConverter FloatToUint = ToastConverter.Create<float, uint>(x => (uint)x);
        public static readonly ToastConverter FloatToLong = ToastConverter.Create<float, long>(x => (long)x);
        public static readonly ToastConverter FloatToUlong = ToastConverter.Create<float, ulong>(x => (ulong)x);
        public static readonly ToastConverter FloatToDouble = ToastConverter.Create<float, double>(x => (double)x);
        public static readonly ToastConverter FloatToDecimal = ToastConverter.Create<float, decimal>(x => (decimal)x);

        public static readonly ToastConverter DoubleTobyte = ToastConverter.Create<double, byte>(x => (byte)x);
        public static readonly ToastConverter DoubleTosbyte= ToastConverter.Create<double, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter DoubleToShort = ToastConverter.Create<double, short>(x => (short)x);
        public static readonly ToastConverter DoubleToUshort = ToastConverter.Create<double, ushort>(x => (ushort)x);
        public static readonly ToastConverter DoubleToInt = ToastConverter.Create<double, int>(x => (int)x);
        public static readonly ToastConverter DoubleToUint = ToastConverter.Create<double, uint>(x => (uint)x);
        public static readonly ToastConverter DoubleToLong = ToastConverter.Create<double, long>(x => (long)x);
        public static readonly ToastConverter DoubleToUlong = ToastConverter.Create<double, ulong>(x => (ulong)x);
        public static readonly ToastConverter DoubleToFloat = ToastConverter.Create<double, float>(x => (float)x);
        public static readonly ToastConverter DoubleToDecimal = ToastConverter.Create<double, decimal>(x => (decimal)x);

        public static readonly ToastConverter DecimalTobyte = ToastConverter.Create<decimal, byte>(x => (byte)x);
        public static readonly ToastConverter DecimalTosbyte= ToastConverter.Create<decimal, sbyte>(x => (sbyte)x);
        public static readonly ToastConverter DecimalToShort = ToastConverter.Create<decimal, short>(x => (short)x);
        public static readonly ToastConverter DecimalToUshort = ToastConverter.Create<decimal, ushort>(x => (ushort)x);
        public static readonly ToastConverter DecimalToInt = ToastConverter.Create<decimal, int>(x => (int)x);
        public static readonly ToastConverter DecimalToUint = ToastConverter.Create<decimal, uint>(x => (uint)x);
        public static readonly ToastConverter DecimalToLong = ToastConverter.Create<decimal, long>(x => (long)x);
        public static readonly ToastConverter DecimalToUlong = ToastConverter.Create<decimal, ulong>(x => (ulong)x);
        public static readonly ToastConverter DecimalToFloat = ToastConverter.Create<decimal, float>(x => (float)x);
        public static readonly ToastConverter DecimalToDouble = ToastConverter.Create<decimal, double>(x => (double)x);

        private BasicConverters() { }
    }
}

// byte sbyte short ushort int uint long ulong float double decimal