using System;
using System.Collections.Generic;
using System.Linq;
using Toast.Exceptions;
using Toast.Nodes;

namespace Toast
{
    public class ToastExecutor
    {
        public static object Execute(Toaster toaster, INode node, Type target = null)
        {
            if (target is null)
            {
                target = typeof(object);
            }

            switch (node)
            {
                case CommandNode c:
                    if (target == typeof(CommandNode))
                    {
                        return c;
                    }

                    List<object> parameters = new();

                    for (int i = 0; i < c.Parameters.Length; i++)
                    {
                        parameters.Add(Execute(toaster, c.Parameters[i], c.Command.Parameters[i]));
                    }

                    return ConvertParameter(toaster, target, toaster.ExecuteCommand(c.Command, parameters.ToArray()));
                case VariableNode v:
                    if (target == typeof(VariableNode))
                    {
                        return v;
                    }

                    return ConvertParameter(toaster, target, toaster.ExecuteCommand(toaster.GetCommand(v.Name), Array.Empty<object>()));
                case FunctionNode f:
                    return ConvertParameter(toaster, target, f);
                case ListNode l:
                    List<object> list = new();

                    foreach (INode n in l.Value)
                    {
                        list.Add(Execute(toaster, n));
                    }

                    return ConvertParameter(toaster, target, list.ToArray());
                case ValueNode v:
                    return ConvertParameter(toaster, target, v.Value);
                default:
                    throw new InvalidParameterTypeException();
            }
        }

        public static object[] ConvertParameters(Toaster toaster, Type[] targets, object[] parameters)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                parameters[i] = ConvertParameter(toaster, targets[i], parameters[i]);
            }

            return parameters;
        }

        public static object ConvertParameter(Toaster toaster, Type targetType, object parameter)
        {
            if (parameter is null) return parameter;

            Type paramType = parameter.GetType();

            if (paramType == targetType) return parameter;

            List<ToastConverter> converters = toaster.GetConverters().ToList();

            if (converters.Find(c => c.From == paramType && c.To == targetType) is not null and ToastConverter c1)
            {
                return ExecuteConverter(c1, parameter);
            }
            else if (IsNumber(paramType) && converters.Find(c => IsNumber(c.From) && c.To == targetType) is not null and ToastConverter c2)
            {
                return ExecuteConverter(c2, Convert.ChangeType(parameter, c2.From));
            }
            else if (IsNumber(targetType) && converters.Find(c => IsNumber(c.To) && c.From == paramType) is not null and ToastConverter c3)
            {
                return Convert.ChangeType(ExecuteConverter(c3, parameter), targetType);
            }
            else if (IsNumber(targetType) && IsNumber(paramType))
            {
                return Convert.ChangeType(parameter, targetType);
            }
            else if (targetType == typeof(string))
            {
                return parameter.ToString();
            }
            else if (targetType == typeof(object))
            {
                return parameter;
            }
            else
            {
                throw new ParameterConvertException(paramType, targetType);
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
