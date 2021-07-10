﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sprache;

namespace Toast
{
    internal class ToastParser
    {
        static readonly Parser<Element> CommandParser =
            (from first in Parse.Letter.Or(Parse.Char('_')).Once().Text()
             from rest in Parse.LetterOrDigit.Many().Text()
             select new Command(first + rest))
            .Named("Command");

        static readonly Parser<Element> NumberParser =
            (from n in Parse.Decimal
             select new Number(float.Parse(n)))
            .Named("Number");

        static readonly Parser<Element> BoolParser =
            (from b in Parse.String("true").Or(Parse.String("false")).Text()
             select new Bool(b == "true"))
            .Named("Bool");

        private static readonly Parser<char> QuoteParser = Parse.Char('"');
        static readonly Parser<Element> TextParser =
            (from lquot in QuoteParser
             from s in Parse.AnyChar.Except(QuoteParser).Many().Text()
             from rquot in QuoteParser
             select new Text(s))
            .Named("Text");

        static readonly Parser<Element> GroupParser =
            (from lparen in Parse.Char('(')
             from g in ElementParser.Many()
             from rparen in Parse.Char(')')
             select new Group(g.ToArray()))
            .Named("Group");

        static readonly Parser<Element> ElementParser =
            from lspace in Parse.WhiteSpace.Many()
            from e in BoolParser.Or(NumberParser).Or(TextParser).Or(CommandParser).Or(GroupParser)
            from rspace in Parse.WhiteSpace.Many()
            select e;

        static readonly Parser<Element[]> LineParser =
            ElementParser.Many()
            .Select(e => e.ToArray());

        public static (string name, object[] parameters) ParseLine(string line)
        {
            Element[] elements = ParseRaw(line);

            if (elements[0] is not Command)
            {
                throw new InvalidCommandLineException(line);
            }

            string name = ((Command)elements[0]).GetValue();
            object[] parameters = elements[1..].Select(e => e.GetValue()).ToArray();

            return (name, parameters);
        }

        public static Element[] ParseRaw(string line)
        {
            Element[] result = LineParser.Parse(line);

            return result;
        }

        public class InvalidCommandLineException : Exception
        {
            public InvalidCommandLineException() { }

            public InvalidCommandLineException(string line) : base($"\"{line}\" is not a vaild command line.") { }
        }

        public class Element
        {
            private readonly object value;

            public Element(object v)
            {
                value = v;
            }

            public object GetValue()
            {
                return value;
            }
        }

        public class Command : Element
        {
            public Command(string value) : base(value) { }

            public new string GetValue()
            {
                return (string)base.GetValue();
            }
        }

        public class Number : Element
        {
            public Number(float value) : base(value) { }

            public new float GetValue()
            {
                return (float)base.GetValue();
            }
        }
        
        public class Bool : Element
        {
            public Bool(bool value) : base(value) { }

            public new bool GetValue()
            {
                return (bool)base.GetValue();
            }
        }

        public class Text : Element
        {
            public Text(string value) : base(value) { }

            public new string GetValue()
            {
                return (string)base.GetValue();
            }
        }

        public class Group : Element
        {
            public Group(Element[] value) : base(value) { }

            public new Element[] GetValue()
            {
                return (Element[])base.GetValue();
            }
        }
    }
}
