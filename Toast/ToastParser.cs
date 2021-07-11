using System;
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
            from first in Parse.Letter.Or(Parse.Char('_')).Once().Text()
            from rest in Parse.LetterOrDigit.Many().Text()
            select new Command(first + rest);

        static readonly Parser<Element> NumberParser =
            from n in Parse.Decimal
            select new Number(float.Parse(n));

        static readonly Parser<Element> SignedNumberParser =
            from s in Parse.Char('+').Or(Parse.Char('-'))
            from n in NumberParser
            select s == '+' ? n : new Number((float)n.GetValue() * -1);

        static readonly Parser<char> QuoteParser = Parse.Char('"');
        static readonly Parser<char> SlashQuoteParser = Parse.String("\\\"").Select(c => '"');

        static readonly Parser<Element> TextParser =
            from start in QuoteParser
            from s in SlashQuoteParser.Or(Parse.AnyChar).Except(QuoteParser).Many()
            from end in QuoteParser
            select new Text(string.Concat(s));

        static readonly Parser<Element> GroupParser =
            from start in Parse.Char('(')
            from g in ElementParser.Many()
            from end in Parse.Char(')')
            select new Group(g.ToArray());

        static readonly Parser<Element> ElementParser =
            NumberParser.Or(SignedNumberParser).Or(TextParser).Or(CommandParser).Or(GroupParser);

        static readonly Parser<Element[]> LineParser =
            from e in ElementParser.DelimitedBy(Parse.WhiteSpace)
            select e.ToArray();

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
            var result = LineParser.TryParse(line);

            if (result.Remainder.AtEnd)
            {
                return result.Value;
            }
            else
            {
                throw new InvalidCommandLineException(line);
            }
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
