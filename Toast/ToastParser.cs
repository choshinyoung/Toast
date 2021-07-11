using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            from g in ElementParser.Many()
            from end in Parse.Char(')')
            select new Group(g.ToArray());

        static readonly Parser<Element> ElementParser =
            NumberParser.Or(SignedNumberParser).Or(TextParser).Or(CommandParser).Or(GroupParser);

        static readonly Parser<Element[]> LineParser =
            from e in ElementParser.DelimitedBy(Parse.WhiteSpace.AtLeastOnce())
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
