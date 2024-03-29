﻿using System;
using System.Collections.Generic;
using System.Linq;
using Toast.Exceptions;
using Toast.Nodes;
using Toast.Tokens;

namespace Toast
{
    public class Toaster
    {
        private readonly List<ToastCommand> Commands;
        private readonly List<ToastConverter> Converters;

        public readonly Dictionary<string, Type> TypeAliases = new Dictionary<string, Type>()
        {
            { "text", typeof(string) },
            { "number", typeof(long) },
            { "float", typeof(float) },
            { "bool", typeof(bool) },
            { "list", typeof(object[]) },
        };

        public int MaxDepth = 256;

        public Toaster()
        {
            Commands = new List<ToastCommand>();
            Converters = new List<ToastConverter>();
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

        public INode Parse(string line)
        {
            var lexerResult = ToastLexer.Lexicalize(line);
            var parserResult = ToastParser.Parse(this, lexerResult);

            return parserResult;
        }

        public INode Parse(Token[] lexerResult)
        {
            var parserResult = ToastParser.Parse(this, lexerResult);

            return parserResult;
        }

        public object Execute(string line, ToastContext context = null)
        {
            context = GetContext(context);

            var parserResult = Parse(line);
            var executeResult = ToastExecutor.Execute(context, parserResult);

            return executeResult;
        }

        public object ExecuteNode(INode node, ToastContext context = null)
        {
            context = GetContext(context);

            if (context is null)
            {
                context = new ToastContext(this);
            }
            else if (context.Toaster is null)
            {
                context.Toaster = this;
            }

            var executeResult = ToastExecutor.Execute(context, node);

            return executeResult;
        }

        public object ExecuteCommand(ToastCommand cmd, object[] parameters, ToastContext context = null)
        {
            context = GetContext(context);

            var prms = ToastExecutor.ConvertParameters(parameters, cmd.Parameters, context).ToList();

            prms.Insert(cmd.NamePosition, context);

            object result = cmd.Method.Invoke(cmd.Target, prms.ToArray());

            return result;
        }

        public object ExecuteFunction(FunctionNode func, object[] parameters, ToastContext context)
        {
            object result = null;

            if (func.Parameters.Length != parameters.Length)
            {
                throw new FunctionParameterLengthException(func.ToString(), parameters.Length, func.Parameters.Length);
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                ToastCommand cmd = Commands.Find(c => c.Name == func.Parameters[i]);
                if (cmd != null)
                {
                    RemoveCommand(cmd);
                }

                object value = parameters[i];

                AddCommand(ToastCommand.CreateFunc<ToastContext, object>(func.Parameters[i], (ctx) => value));
            }

            foreach (INode line in func.Lines)
            {
                result = ToastExecutor.Execute(context, line);
            }

            return result;
        }

        public T ExecuteConverter<T>(object obj, ToastContext context = null)
        {
            context = GetContext(context);

            return (T)ToastExecutor.ConvertParameter(obj, typeof(T), context);
        }

        public object ExecuteConverter(object obj, Type type, ToastContext context = null)
        {
            context = GetContext(context);

            return ToastExecutor.ConvertParameter(obj, type, context);
        }

        public object ExecuteConverter(ToastConverter cvt, object parameter, ToastContext context = null)
        {
            context = GetContext(context);

            return cvt.Method.Invoke(cvt.Target, new[] { context, parameter });
        }

        private ToastContext GetContext(ToastContext context)
        {
            if (context is null)
            {
                context = new ToastContext(this);
            }
            else if (context.Toaster is null)
            {
                context.Toaster = this;
            }

            return context;
        }
    }
}
