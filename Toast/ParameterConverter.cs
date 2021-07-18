using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toast.Elements;
using Toast.Exceptions;

namespace Toast
{
    internal class ParameterConverter
    {
        private readonly Toaster Toaster;
        private readonly List<ToastCommand> Commands;
        private readonly List<ToastConverter> Converters;

        public ParameterConverter(Toaster toaster)
        {
            Toaster = toaster;
            Commands = toaster.GetCommands().ToList();
            Converters = toaster.GetConverters().ToList();
        }

        public object[] ConvertParameters(Type[] targets, object[] parameters)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                parameters[i] = ConvertParameter(targets[i], parameters[i]);
            }

            return parameters;
        }

        public object ConvertParameter(Type target, object parameter)
        {
            if (parameter is null) return parameter;

            Type targetType = target;
            Type paramType = parameter.GetType();

            if (paramType == targetType) return parameter;

            if (Converters.ToList().Find(c => c.From == paramType && c.To == targetType) is not null and ToastConverter c1)
            {
                return ExecuteConverter(c1, parameter);
            }
            else if (IsNumber(paramType) && Converters.Find(c => IsNumber(c.From) && c.To == targetType) is not null and ToastConverter c2)
            {
                return ExecuteConverter(c2, Convert.ChangeType(parameter, c2.From));
            }
            else if (IsNumber(targetType) && Converters.Find(c => IsNumber(c.To) && c.From == paramType) is not null and ToastConverter c3)
            {
                return Convert.ChangeType(ExecuteConverter(c3, parameter), targetType);
            }
            else if (IsNumber(targetType) && IsNumber(paramType))
            {
                return Convert.ChangeType(parameter, targetType);
            }
            else if (parameter is Variable c4)
            {
                return Toaster.ExecuteCommand(Toaster.GetCommand(c4.GetValue()), Array.Empty<object>());

                throw new CommandNotFoundException(c4.GetValue());
            }
            else if (targetType is not object)
            {
                throw new ParameterConvertException(paramType, targetType);
            }
            else
            {
                return parameter;
            }
        }

        public static object ExecuteConverter(ToastConverter cvt, object parameter)
        {
            return cvt.Method.Invoke(cvt.Target, new[] { parameter });
        }

        public static bool IsNumber(Type type)
            => type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) || type == typeof(ushort)
            || type == typeof(uint) || type == typeof(ulong) || type == typeof(float) || type == typeof(double) || type == typeof(decimal);
    }
}
