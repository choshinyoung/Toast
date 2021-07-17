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
        public void RemoveCommand(params ToastCommand[] commands)
        {
            foreach (ToastCommand cmd in commands)
            {
                if (!Commands.Contains(cmd))
                {
                    throw new CannotRemoveCommandException(cmd.Name);
                }

                Commands.Remove(cmd);
            }
        }

        public void RemoveConverter(params ToastConverter[] converters)
        {
            foreach (ToastConverter cvt in converters)
            {
                if (!Converters.Contains(cvt))
                {
                    throw new CannotRemoveConverterException();
                }

                Converters.Remove(cvt);
            }
        }

        public IReadOnlyList<ToastCommand> GetCommands()
            => Commands.AsReadOnly();

        public IReadOnlyList<ToastConverter> GetConverters()
            => Converters.AsReadOnly();

        public object ExecuteLine(string line)
        {
            var parseResult = ToastParser.ParseRaw(line);

            if (parseResult[0] is not Command)
            {
                throw new InvalidCommandLineException(line);
            }

            return ExecuteParsedLine(parseResult);
        }

        private object ExecuteParsedLine(Element[] parsed)
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

        public object Execute(string line)
        {
            var parseResult = ToastParser.ParseRaw(line);

            int index = -1;
            var result = ExecuteParameters(parseResult, 1, ref index)[0];

            if (index != parseResult.Length - 1)
            {
                throw new ParameterCountException(parseResult.Length, index + 1);
            }

            return result;
        }

        public object ExecuteFunction(Function func, object[] parameters)
        {
            object result = null;

            if (func.Parameters.Length != parameters.Length)
            {
                throw new FunctionParameterLengthException(func.ToString(), parameters.Length, func.Parameters.Length);
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                ToastCommand cmd = Commands.Find(c => c.Name == func.Parameters[i]);
                if (cmd is not null)
                {
                    RemoveCommand(cmd);
                }

                object value = parameters[i];

                AddCommand(ToastCommand.CreateFunc<ToastContext, object>(func.Parameters[i], (ctx) => value));
            }

            foreach (Element[] line in func.GetValue())
            {
                if (line[0] is not Command)
                {
                    throw new InvalidCommandLineException($"{line[0].GetValue()}..");
                }

                result = ExecuteParsedLine(line);
            }

            return result;
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
                        if (Commands.Any(cc => cc.Name == c.GetValue()) && GetCommand(c.GetValue()) is ToastCommand cmd && cmd.Parameters.Length > 0)
                        {
                            parameters.Add(ExecuteCommand(cmd, ExecuteParameters(elements, cmd.Parameters.Length, ref index)));
                        }
                        else
                        {
                            parameters.Add(new Variable(c.GetValue()));
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
                    case List l:
                        List<object> lst = new();

                        foreach (Element[] e in l.GetValue())
                        {
                            i = -1;
                            lst.Add(ExecuteParameters(e, 1, ref i)[0]);
                        }

                        parameters.Add(lst.ToArray());

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

            var ppp = new[] { new ToastContext(this) }.Concat(parameters).ToArray();

            object result = cmd.Method.Invoke(cmd.Target, ppp);

            return result;
        }

        private object[] ConvertParameters(Type[] targets, object[] parameters)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                parameters[i] = ConvertParameter(targets[i], parameters[i]);
            }

            return parameters;
        }

        private object ConvertParameter(Type target, object parameter)
        {
            if (parameter is null) return parameter;

            Type targetType = target;
            Type paramType = parameter.GetType();

            if (paramType == targetType) return parameter;

            if (Converters.Find(c => c.From == paramType && c.To == targetType) is not null and ToastConverter c1)
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
                return ExecuteCommand(GetCommand(c4.GetValue()), Array.Empty<object>());

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

        private static object ExecuteConverter(ToastConverter cvt, object parameter)
        {
            return cvt.Method.Invoke(cvt.Target, new[] { parameter });
        }

        private static bool IsNumber(Type type)
            => type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) || type == typeof(ushort)
            || type == typeof(uint) || type == typeof(ulong) || type == typeof(float) || type == typeof(double) || type == typeof(decimal);
    }
}
