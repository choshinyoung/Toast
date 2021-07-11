using System;
using System.Collections.Generic;
using Toast.Elements;
using Toast.Exceptions;

namespace Toast
{
    public class Toast
    {
        public List<ToastCommand> Commands = new();

        public Action<ToastCommand> AddCommand;

        public Toast()
        {
            AddCommand = Commands.Add;
        }

        public object Execute(string line)
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
                        ToastCommand cmd = GetCommand(c.GetValue());

                        parameters.Add(ExecuteCommand(cmd, ExecuteParameters(elements, cmd.Parameters.Length, ref index)));

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

        private static object ExecuteCommand(ToastCommand cmd, object[] parameters)
        {
            object result = cmd.Method.Invoke(cmd.Target, parameters);

            return result;
        }
    }
}
