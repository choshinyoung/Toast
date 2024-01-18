using System;
using System.Collections.Generic;
using System.Linq;
using Toast.Exceptions;
using Toast.Nodes;
using Toast.Tokens;

namespace Toast
{
    internal class ToastParser
    {
        public static INode Parse(Toaster toaster, Token[] tokens)
        {
            List<INode> nodes = new List<INode>();
            List<(ToastCommand command, CommandNode node)> commands = new List<(ToastCommand command, CommandNode node)>();

            foreach (Token token in tokens)
            {
                switch (token)
                {
                    case CommandToken c:
                        if (toaster.GetCommands().Any(_c => _c.Name == c.GetValue()))
                        {
                            ToastCommand toastCmd = toaster.GetCommand(c.GetValue());
                            if (toastCmd.Parameters.Length > 0)
                            {
                                CommandNode node = new CommandNode(toastCmd, Array.Empty<INode>());

                                nodes.Add(node);
                                commands.Add((toastCmd, node));

                                continue;
                            }
                        }

                        nodes.Add(new VariableNode(c.GetValue()));

                        break;
                    case GroupToken g:
                        List<INode> values = new List<INode>();

                        foreach (Token[] e in g.GetValue())
                        {
                            values.Add(Parse(toaster, e));
                        }

                        nodes.Add(new GroupNode(values.ToArray()));

                        break;
                    case FunctionToken f:
                        List<INode> lines = new List<INode>();

                        foreach (Token[] e in f.GetValue())
                        {
                            lines.Add(Parse(toaster, e));
                        }

                        nodes.Add(new FunctionNode(f.Parameters, lines.ToArray()));

                        break;
                    case ListToken l:
                        List<INode> members = new List<INode>();

                        foreach (Token[] e in l.GetValue())
                        {
                            members.Add(Parse(toaster, e));
                        }

                        nodes.Add(new ListNode(members.ToArray()));

                        break;
                    case TextToken t:
                        List<object> contents = new List<object>();

                        string tmp = "";
                        foreach (object o in t.GetValue())
                        {
                            if (o is char c)
                            {
                                tmp += c;
                            }
                            else if (o is Token[] tkn)
                            {
                                contents.Add(tmp);
                                tmp = "";

                                contents.Add(Parse(toaster, tkn));
                            }
                            else
                            {
                                throw new InvalidParameterTypeException(o);
                            }
                        }
                        contents.Add(tmp);

                        nodes.Add(new TextNode(contents.ToArray()));

                        break;
                    default:
                        if (token is NumberToken)
                        {
                            nodes.Add(new ValueNode(token.GetValue()));

                            break;
                        }

                        throw new InvalidParameterTypeException(token);
                }
            }

            commands.Sort((c1, c2) => c2.command.Priority.CompareTo(c1.command.Priority));

            foreach (var (command, node) in commands)
            {
                int index = nodes.IndexOf(node);
                if (index == -1) continue;

                INode[] parameters = GetParameters(nodes, index - 1, false, command.NamePosition);

                index = nodes.IndexOf(node);
                parameters = parameters.Concat(GetParameters(nodes, index + 1, true, command.Parameters.Length - command.NamePosition)).ToArray();

                index = nodes.IndexOf(node);
                nodes[index] = new CommandNode(command, parameters);
            }

            if (nodes.Count != 1)
            {
                throw new ParameterCountException(nodes.Count, 1);
            }

            return nodes[0];
        }

        private static INode[] GetParameters(List<INode> nodes, int start, bool isRight, int count)
        {
            List<INode> parameters = new List<INode>();

            int index = start;

            while (parameters.Count < count)
            {
                if (index < 0 || index >= nodes.Count)
                {
                    throw new ParameterCountException(parameters.Count, count);
                }

                switch (nodes[index])
                {
                    case CommandNode c:
                        if (c.Parameters.Length > 0)
                        {
                            parameters.Add(c);
                        }
                        else
                        {
                            if ((isRight && c.Command.NamePosition != 0) || (!isRight && c.Command.NamePosition != c.Command.Parameters.Length))
                            {
                                throw new InvalidCommandParameterException(c.Command.Name);
                            }

                            parameters.Add(new CommandNode(c.Command, GetParameters(nodes, index + (isRight ? 1 : -1), isRight, c.Command.Parameters.Length)));
                        }

                        break;
                    case GroupNode g:
                        if (parameters.Count + g.Values.Length > count)
                        {
                            throw new ParameterCountException(parameters.Count + g.Values.Length, count);
                        }

                        parameters.AddRange(g.Values);

                        break;
                    default:
                        parameters.Add(nodes[index]);

                        break;
                }

                index += isRight ? 1 : -1;
            }

            if (isRight)
            {
                nodes.RemoveRange(start, index - start);
            }
            else
            {
                nodes.RemoveRange(index + 1, start - index);
            }

            if (!isRight)
            {
                parameters.Reverse();
            }

            return parameters.ToArray();
        }
    }
}
