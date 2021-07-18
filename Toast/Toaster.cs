﻿using System;
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

        public IReadOnlyList<ToastCommand> GetCommands()
            => Commands.AsReadOnly();

        public IReadOnlyList<ToastConverter> GetConverters()
            => Converters.AsReadOnly();

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

        public object ExecuteLine(string line)
        {
            var parseResult = ToastParser.ParseRaw(line);

            if (parseResult[0] is not Command)
            {
                throw new InvalidCommandLineException(line);
            }

            ToastCommand cmd = GetCommand(((Command)parseResult[0]).GetValue());

            int index = 0;
            object[] parameters = ExecuteParameters(parseResult, cmd.Parameters.Length, ref index);

            if (++index != parseResult.Length)
            {
                throw new ParameterCountException(parseResult.Length - 1, index - 1);
            }

            return ExecuteCommand(cmd, parameters);
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
                int index = -1;
                result = ExecuteParameters(line, 1, ref index)[0];

                if (index != line.Length - 1)
                {
                    throw new ParameterCountException(line.Length, index + 1);
                }
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

        public ToastCommand GetCommand(string name)
        {
            ToastCommand cmd = Commands.Find(c => c.Name == name);
            if (cmd is null)
            {
                throw new CommandNotFoundException(name);
            }

            return cmd;
        }

        public object ExecuteCommand(ToastCommand cmd, object[] parameters)
        {
            parameters = new ParameterConverter(this).ConvertParameters(cmd.Parameters, parameters);

            var ppp = new[] { new ToastContext(this) }.Concat(parameters).ToArray();

            object result = cmd.Method.Invoke(cmd.Target, ppp);

            return result;
        }
    }
}
