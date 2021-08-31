using System;
using System.Collections.Generic;
using System.Linq;
using Toast.Exceptions;
using Toast.Nodes;

namespace Toast
{
    internal class ToastExecutor
    {
        public static object Execute(ToastContext context, INode node, Type target = null)
        {
            if (target is null)
            {
                target = typeof(object);
            }

            if (target == typeof(INode)) return node;

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
                        parameters.Add(Execute(context, c.Parameters[i],  c.Command.Parameters[i]));
                    }

                    return ConvertParameter(context.Toaster.ExecuteCommand(c.Command, parameters.ToArray(), context: context), target, context);
                case VariableNode v:
                    if (target == typeof(VariableNode))
                    {
                        return v;
                    }

                    return ConvertParameter(context.Toaster.ExecuteCommand(context.Toaster.GetCommand(v.Name), Array.Empty<object>(), context: context), target, context);
                case FunctionNode f:
                    return ConvertParameter(f, target, context);
                case ListNode l:
                    List<object> list = new();

                    foreach (INode n in l.Value)
                    {
                        list.Add(Execute(context, n));
                    }

                    return ConvertParameter(list.ToArray(), target, context);
                case TextNode t:
                    string result = "";

                    foreach (object o in t.Values)
                    {
                        if (o is string s)
                        {
                            result += s;
                        }
                        else if (o is INode n)
                        {
                            result += Execute(context, n, typeof(string));
                        }
                    }

                    return result;
                case ValueNode v:
                    return ConvertParameter(v.Value, target, context);
                default:
                    throw new InvalidParameterTypeException(node);
            }
        }

        public static object[] ConvertParameters(object[] parameters, Type[] targets, ToastContext context)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                parameters[i] = ConvertParameter(parameters[i], targets[i], context);
            }

            return parameters;
        }

        public static object ConvertParameter(object parameter, Type targetType, ToastContext context)
        {
            if (parameter is null) return parameter;

            Type paramType = parameter.GetType();

            if (paramType == targetType) return parameter;

            List<ToastConverter> converters = context.Toaster.GetConverters().ToList();

            if (converters.Find(c => c.From == paramType && c.To == targetType) is not null and ToastConverter c1)
            {
                return context.Toaster.ExecuteConverter(c1, parameter, context);
            }
            else if (IsNumber(paramType) && converters.Find(c => IsNumber(c.From) && c.To == targetType) is not null and ToastConverter c2)
            {
                return context.Toaster.ExecuteConverter(c2, Convert.ChangeType(parameter, c2.From), context);
            }
            else if (IsNumber(targetType) && converters.Find(c => IsNumber(c.To) && c.From == paramType) is not null and ToastConverter c3)
            {
                return Convert.ChangeType(context.Toaster.ExecuteConverter(c3, parameter, context), targetType);
            }
            else if (IsNumber(targetType) && IsNumber(paramType))
            {
                return Convert.ChangeType(parameter, targetType);
            }
            else if (targetType == typeof(INode) && paramType.IsAssignableTo(typeof(INode)))
            {
                return (INode)parameter;
            }
            else if (targetType == typeof(string))
            {
                return parameter switch
                {
                    FunctionNode function => $"funtion ({string.Join(", ", function.Parameters)}) {{ }}",
                    object[] arr => $"[{string.Join(", ", arr.Select(a => ConvertParameter(a, typeof(string), context)))}]",
                    bool b => b ? "true" : "false",
                    _ => parameter.ToString()
                };
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

        public static bool IsNumber(Type type)
            => type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) || type == typeof(ushort)
            || type == typeof(uint) || type == typeof(ulong) || type == typeof(float) || type == typeof(double) || type == typeof(decimal);
    }
}
