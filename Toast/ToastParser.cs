using System;
using System.Linq;
using Sprache;
using Toast.Elements;
using Toast.Exceptions;

namespace Toast
{
    internal class ToastParser
    {
        static readonly Parser<char> UnderLine = Parse.Char('_');
        static readonly Parser<Element> CommandParser =
            from first in Parse.Letter.Or(UnderLine).Once().Text()
            from rest in Parse.LetterOrDigit.Or(UnderLine).Many().Text()
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
            from g in LineParser
            from end in Parse.Char(')')
            select new Group(g.ToArray());

        static readonly Parser<string[]> FunctionParameterParser =
            from start in Parse.Char('(')
            from startSpace in Parse.WhiteSpace.Many()
            from g in CommandParser.DelimitedBy(Parse.WhiteSpace.AtLeastOnce()).Optional()
            from endSpace in Parse.WhiteSpace.Many()
            from end in Parse.Char(')')
            select g.IsEmpty ? Array.Empty<string>() : g.Get().Select(c => ((Command)c).GetValue()).ToArray();

        static readonly Parser<Element> FunctionParser =
            from parameters in FunctionParameterParser
            from space in Parse.WhiteSpace.Many()
            from start in Parse.Char('{')
            from startSpace in Parse.WhiteSpace.Many()
            from g in LineParser.DelimitedBy(Parse.LineEnd.Or(Parse.String(";")).AtLeastOnce()).Optional()
            from endSpace in Parse.WhiteSpace.Many()
            from end in Parse.Char('}')
            select new Function(g.IsEmpty ? Array.Empty<Element[]>() : g.Get().ToArray(), parameters);

        static readonly Parser<char> CommaDividerParser =
            from startSpace in Parse.WhiteSpace.Many()
            from comma in Parse.Char(',')
            from endSpace in Parse.WhiteSpace.Many()
            select comma;

        static readonly Parser<Element> ListParser =
            from start in Parse.Char('[')
            from l in LineParser.DelimitedBy(CommaDividerParser)
            from end in Parse.Char(']')
            select new List(l.ToArray());

        static readonly Parser<Element> ElementParser =
            NumberParser.Or(SignedNumberParser).Or(TextParser).Or(CommandParser).Or(FunctionParser).Or(ListParser).Or(GroupParser);

        static readonly Parser<Element[]> LineParser =
            from startSpace in Parse.WhiteSpace.Many()
            from e in ElementParser.DelimitedBy(Parse.WhiteSpace.AtLeastOnce())
            from endSpace in Parse.WhiteSpace.Many()
            select e.ToArray();

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
    }
}
