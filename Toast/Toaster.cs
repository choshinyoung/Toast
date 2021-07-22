using System;
using System.Collections.Generic;
using System.Linq;
using Toast.Exceptions;
using Toast.Tokens;

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

        public ToastCommand GetCommand(string name)
        {
            ToastCommand cmd = Commands.Find(c => c.Name == name);
            if (cmd is null)
            {
                throw new CommandNotFoundException(name);
            }

            return cmd;
        }

        public object Execute(string line)
        {
            var parseResult = ToastParser.ParseRaw(line);

            var result = ExecuteParameters(parseResult.ToList());

            if (result.Length != 1)
            {
                throw new InvalidCommandLineException(line);
            }

            return result[0];
        }

        public object ExecuteLine(string line)
        {
            var parseResult = ToastParser.ParseRaw(line);

            if (parseResult[0] is not CommandToken)
            {
                throw new InvalidCommandLineException(line);
            }

            ToastCommand cmd = GetCommand(((CommandToken)parseResult[0]).GetValue());

            object[] parameters = ExecuteParameters(parseResult.ToList());

            if (parameters.Length != 1)
            {
                throw new ParameterCountException();
            }

            return ExecuteCommand(cmd, parameters);
        }

        public object ExecuteCommand(ToastCommand cmd, object[] parameters)
        {
            var prms = new ParameterConverter(this).ConvertParameters(cmd.Parameters, parameters).ToList();

            prms.Insert(cmd.NamePosition, new ToastContext(this));

            object result = cmd.Method.Invoke(cmd.Target, prms.ToArray());

            return result;
        }

        public object ExecuteFunction(FunctionToken func, object[] parameters)
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

            foreach (Token[] line in func.GetValue())
            {
                var executeResult = ExecuteParameters(line.ToList());

                if (executeResult.Length != 1)
                {
                    throw new InvalidCommandLineException();
                }
            }

            return result;
        }

        private object[] ExecuteParameters(List<Token> elements)
        {
            if (elements.Count == 1)
            {
                switch (elements[0])
                {
                    case CommandToken c:
                        return new[] { new VariableNode(c.GetValue()) };
                    case ListToken l:
                        List<object> lst = new();

                        foreach (Token[] e in l.GetValue())
                        {
                            object[] members = ExecuteParameters(e.ToList());

                            if (members.Length != 1)
                            {
                                throw new ParameterCountException();
                            }

                            lst.Add(members[0]);
                        }

                        return new[] { lst.ToArray() };
                    case FunctionToken or VariableNode:
                        return new[] { elements[0] };
                    default:
                        return new[] { elements[0].GetValue() };
                }
            }

            List<(ToastCommand command, CommandToken element)> commands = new();

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] is not CommandToken) continue;

                CommandToken cmd = (CommandToken)elements[i];
                if (Commands.Any(c => c.Name == cmd.GetValue()))
                {
                    ToastCommand toastCmd = GetCommand(cmd.GetValue());
                    if (toastCmd.Parameters.Length > 0)
                    {
                        commands.Add((toastCmd, cmd));
                        
                        continue;
                    }
                }

                elements[i] = new VariableNode(cmd.GetValue());
            }

            commands.Sort((c1, c2) => c2.command.Priority.CompareTo(c1.command.Priority));

            foreach (var (command, element) in commands)
            {
                int index = elements.IndexOf(element);

                if (index == -1) continue;

                object[] parameters = GetParameters(elements, index + 1, true, command.Parameters.Length - command.NamePosition)
                              .Concat(GetParameters(elements, index - 1, false, command.NamePosition)).ToArray();

                index = elements.IndexOf(element);
                elements[index] = new Token(ExecuteCommand(command, parameters));
            }

            return elements.Select(e => e.GetValue()).ToArray();
        }

        private object[] GetParameters(List<Token> elements, int start, bool isRight, int count)
        {
            List<object> parameters = new();

            int index = start;

            while (parameters.Count < count)
            {
                if (index < 0 || index >= elements.Count)
                {
                    throw new ParameterCountException(parameters.Count, count);
                }

                switch (elements[index])
                {
                    case CommandToken c:
                        ToastCommand cmd = GetCommand(c.GetValue());

                        if ((isRight && cmd.NamePosition != 0) || (!isRight && cmd.NamePosition != cmd.Parameters.Length))
                        {
                            throw new ParameterCountException();
                        }

                        parameters.Add(ExecuteCommand(cmd, GetParameters(elements, index + (isRight ? 1 : -1), isRight, cmd.Parameters.Length)));

                        break;
                    case GroupToken g:
                        List<object> groupParameters = new();
                        
                        foreach (Token[] line in g.GetValue())
                        {
                            object[] result = ExecuteParameters(line.ToList());

                            if (result.Length != 1)
                            {
                                throw new InvalidCommandLineException();
                            }

                            groupParameters.Add(result[0]);
                        }

                        if (parameters.Count + groupParameters.Count > count)
                        {
                            throw new ParameterCountException(parameters.Count + groupParameters.Count, count);
                        }

                        parameters.AddRange(groupParameters);

                        break;
                    case ListToken l:
                        List<object> lst = new();

                        foreach (Token[] e in l.GetValue())
                        {
                            object[] members = ExecuteParameters(e.ToList());

                            if (members.Length != 1)
                            {
                                throw new ParameterCountException();
                            }

                            lst.Add(members[0]);
                        }

                        parameters.Add(lst.ToArray());

                        break;
                    case VariableNode or FunctionToken:
                        parameters.Add(elements[index]);

                        break;
                    default:
                        parameters.Add(elements[index].GetValue());

                        break;
                }

                index += isRight ? 1 : -1;
            }

            if (isRight)
            {
                elements.RemoveRange(start, index - start);
            }
            else
            {
                elements.RemoveRange(index + 1, start - index);
            }

            if (!isRight)
            {
                parameters.Reverse();
            }

            return parameters.ToArray();
        }
    }
}
