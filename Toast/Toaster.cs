using System;
using System.Collections.Generic;
using System.Linq;
using Toast.Elements;
using Toast.Exceptions;

namespace Toast
{
    public class Toaster
    {
        private readonly List<ToastCommand> Commands;
        private readonly List<ToastConverter> Converters;

        public Toaster()
        {
            Commands = new();
            Converters = new();
        }

        public void AddCommand(params ToastCommand[] commands)
        {
            foreach (ToastCommand cmd in commands)
            {
                if (Commands.Any(c => c.Name == cmd.Name))
                {
                    throw new CommandAlreadyExistException(cmd.Name);
                }

                Commands.Add(cmd);
            }
        }

        public void AddConverter(params ToastConverter[] converters)
        {
            foreach (ToastConverter cvt in converters)
            {
                if (Converters.Any(c => c.From == cvt.From && c.To == cvt.From))
                {
                    throw new ConverterAlreadyExistException(cvt.From, cvt.To);
                }

                Converters.Add(cvt);
            }
        }

        public IReadOnlyList<ToastCommand> GetCommands()
            => Commands.AsReadOnly();

        public IReadOnlyList<ToastConverter> GetConverters()
            => Converters.AsReadOnly();

        public object Execute(string line)
        {
            var parseResult = ToastParser.ParseRaw(line);

            if (parseResult[0] is not Command)
            {
                throw new InvalidCommandLineException(line);
            }

            return ExecuteParsedLine(parseResult);
        }

        public object ExecuteParsedLine(Element[] parsed)
        {
            ToastCommand cmd = GetCommand(((Command)parsed[0]).GetValue());

            int index = 0;
            object[] parameters = ExecuteParameters(parsed, cmd.Parameters.Length, ref index);

            if (++index != parsed.Length)
            {
                throw new ParameterCountException(parsed.Length - 1, index - 1);
            }

            return ExecuteCommand(cmd, parameters);
        }
        
        private object[] ExecuteParameters(Element[] elements, int count, ref int index, bool isGroup = false)
        {
            List<object> parameters = new();

            while (parameters.Count < count || isGroup)
            {
                index++;

                if (elements.Length <= index)
                {
                    if (isGroup)
                    {
                        return parameters.ToArray();
                    }

                    throw new ParameterCountException(elements.Length - 1, count);
                }

                Element ele = elements[index];

                switch (ele)
                {
                    case Number or Text:
                        parameters.Add(ele.GetValue());

                        break;
                    case Command c:
                        if (Commands.Any(cc => cc.Name == c.GetValue()))
                        {
                            ToastCommand cmd = GetCommand(c.GetValue());
                            parameters.Add(ExecuteCommand(cmd, ExecuteParameters(elements, cmd.Parameters.Length, ref index)));
                        }
                        else
                        {
                            parameters.Add(c);
                        }

                        break;
                    case Group g:
                        int i = -1;
                        object[] groupParameters = ExecuteParameters(g.GetValue(), 0, ref i, true);

                        if (parameters.Count + groupParameters.Length > count && !isGroup)
                        {
                            throw new ParameterCountException(parameters.Count + groupParameters.Length, count);
                        }

                        parameters.AddRange(groupParameters);

                        break;
                    case Function f:
                        parameters.Add(f);

                        break;
                    default:
                        throw new InvalidParameterTypeException(ele);
                }
            }

            return parameters.ToArray();
        }

        private ToastCommand GetCommand(string name)
        {
            ToastCommand cmd = Commands.Find(c => c.Name == name);
            if (cmd is null)
            {
                throw new CommandNotFoundException(name);
            }

            return cmd;
        }

        private object ExecuteCommand(ToastCommand cmd, object[] parameters)
        {
            parameters = ConvertParameters(cmd.Parameters, parameters);

            object result = cmd.Method.Invoke(cmd.Target, new[] { new ToastContext(this) }.Union(parameters).ToArray());

            return result;
        }

        private object[] ConvertParameters(Type[] targets, object[] parameters)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (parameters[i] is null) continue;

                Type targetType = targets[i];
                Type paramType = parameters[i].GetType();

                if (paramType == targetType) continue;

                if (Converters.Find(c => c.From == paramType && c.To == targetType) is not null and ToastConverter c1)
                {
                    parameters[i] = ExecuteConverter(c1, parameters[i]);
                }
                else if (IsNumber(paramType) && Converters.Find(c => IsNumber(c.From) && c.To == targetType) is not null and ToastConverter c2)
                {
                    parameters[i] = ExecuteConverter(c2, Convert.ChangeType(parameters[i], c2.From));
                }
                else if (IsNumber(targetType) && Converters.Find(c => IsNumber(c.To) && c.From == paramType) is not null and ToastConverter c3)
                {
                    parameters[i] = Convert.ChangeType(ExecuteConverter(c3, parameters[i]), targetType);
                }
                else if (IsNumber(targetType) && IsNumber(paramType))
                {
                    parameters[i] = Convert.ChangeType(parameters[i], targetType);
                }
                else if (parameters[i] is Command c4)
                {
                    throw new CommandNotFoundException(c4.GetValue());
                }
                else if (targetType is not object)
                {
                    throw new ParameterConvertException(paramType, targetType);
                }
            }

            return parameters;
        }

        private static object ExecuteConverter(ToastConverter cvt, object parameter)
        {
            return cvt.Method.Invoke(cvt.Target, new[] { parameter });
        }

        private static bool IsNumber(Type type)
            => type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) || type == typeof(ushort)
            || type == typeof(uint) || type == typeof(ulong) || type == typeof(float) || type == typeof(double) || type == typeof(decimal);
    }
}
