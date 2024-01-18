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
            context.Depth++;

            if (context.Depth > context.Toaster.MaxDepth)
            {
                throw new CommandDepthLimitException();
            }

            if (target is null)
            {
                target = typeof(object);
            }

            object result;

            if (target == typeof(INode))
            {
                context.Depth--;

                return node;
            }

            switch (node)
            {
                case CommandNode c:
                    if (target == typeof(CommandNode))
                    {
                        result = c;

                        break;
                    }

                    List<object> parameters = new List<object>();

                    for (int i = 0; i < c.Parameters.Length; i++)
                    {
                        parameters.Add(Execute(context, c.Parameters[i], c.Command.Parameters[i]));
                    }

                    result = ConvertParameter(context.Toaster.ExecuteCommand(c.Command, parameters.ToArray(), context: context), target, context);

                    break;
                case VariableNode v:
                    if (target == typeof(VariableNode))
                    {
                        result = v;

                        break;
                    }

                    result = ConvertParameter(context.Toaster.ExecuteCommand(context.Toaster.GetCommand(v.Name), Array.Empty<object>(), context: context), target, context);

                    break;
                case FunctionNode f:
                    result = ConvertParameter(f, target, context);

                    break;
                case ListNode l:
                    List<object> list = new List<object>();

                    foreach (INode n in l.Value)
                    {
                        list.Add(Execute(context, n));
                    }

                    result = ConvertParameter(list.ToArray(), target, context);

                    break;
                case TextNode t:
                    string res = "";

                    foreach (object o in t.Values)
                    {
                        if (o is string s)
                        {
                            res += s;
                        }
                        else if (o is INode n)
                        {
                            res += Execute(context, n, typeof(string));
                        }
                    }

                    result = res;

                    break;
                case ValueNode v:
                    result = ConvertParameter(v.Value, target, context);

                    break;
                default:
                    throw new InvalidParameterTypeException(node);
            }

            context.Depth--;

            return result;
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

            List<ToastConverter> converters = context.Toaster.GetConverters().ToList();

            ToastConverter c1 = converters.Find(c => c.From == paramType && c.To == targetType),
                c2 = converters.Find(c => IsNumber(c.From) && c.To == targetType),
                c3 = converters.Find(c => IsNumber(c.To) && c.From == paramType);

            if (c1 != null)
            {
                return context.Toaster.ExecuteConverter(c1, parameter, context);
            }
            else if (paramType == targetType)
            {
                return parameter;
            }
            else if (IsNumber(paramType) && c2 != null)
            {
                return context.Toaster.ExecuteConverter(c2, Convert.ChangeType(parameter, c2.From), context);
            }
            else if (IsNumber(targetType) && c3 != null)
            {
                return Convert.ChangeType(context.Toaster.ExecuteConverter(c3, parameter, context), targetType);
            }
            else if (IsNumber(targetType) && IsNumber(paramType))
            {
                return Convert.ChangeType(parameter, targetType);
            }
            else if (targetType == typeof(INode) && typeof(INode).IsAssignableFrom(paramType))
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
