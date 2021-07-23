using System;
using System.Linq;
using Sprache;
using Toast.Exceptions;
using Toast.Tokens;

namespace Toast
{
    internal class ToastLexer
    {
        static readonly Parser<char> UnderLine = Parse.Char('_');
        static readonly Parser<Token> CommandParser =
            from first in Parse.Letter.Or(UnderLine).Once().Text()
            from rest in Parse.LetterOrDigit.Or(UnderLine).Many().Text()
            select new CommandToken(first + rest);

        static readonly Parser<Token> NumberParser =
            from n in Parse.Decimal
            select new NumberToken(float.Parse(n));

        static readonly Parser<Token> SignedNumberParser =
            from s in Parse.Char('+').Or(Parse.Char('-'))
            from n in NumberParser
            select s == '+' ? n : new NumberToken((float)n.GetValue() * -1);

        static readonly Parser<char> QuoteParser = Parse.Char('"');
        static readonly Parser<char> SlashQuoteParser = Parse.String("\\\"").Select(c => '"');
        static readonly Parser<Token> TextParser =
            from start in QuoteParser
            from s in SlashQuoteParser.Or(Parse.AnyChar).Except(QuoteParser).Many()
            from end in QuoteParser
            select new TextToken(string.Concat(s));

        static readonly Parser<Token> GroupParser =
            from start in Parse.Char('(')
            from g in LineParser.DelimitedBy(CommaDividerParser)
            from end in Parse.Char(')')
            select new GroupToken(g.ToArray());

        static readonly Parser<string[]> FunctionParameterParser =
            from start in Parse.Char('(')
            from startSpace in Parse.WhiteSpace.Many()
            from g in CommandParser.DelimitedBy(CommaDividerParser).Optional()
            from endSpace in Parse.WhiteSpace.Many()
            from end in Parse.Char(')')
            select g.IsEmpty ? Array.Empty<string>() : g.Get().Select(c => ((CommandToken)c).GetValue()).ToArray();

        static readonly Parser<Token> FunctionParser =
            from parameters in FunctionParameterParser
            from space in Parse.WhiteSpace.Many()
            from start in Parse.Char('{')
            from startSpace in Parse.WhiteSpace.Many()
            from g in LineParser.DelimitedBy(Parse.LineEnd.Or(Parse.String(";")).AtLeastOnce()).Optional()
            from endSpace in Parse.WhiteSpace.Many()
            from end in Parse.Char('}')
            select new FunctionToken(g.IsEmpty ? Array.Empty<Token[]>() : g.Get().ToArray(), parameters);
        
        static readonly Parser<char> CommaDividerParser =
            from startSpace in Parse.WhiteSpace.Many()
            from comma in Parse.Char(',')
            from endSpace in Parse.WhiteSpace.Many()
            select comma;

        static readonly Parser<Token> ListParser =
            from start in Parse.Char('[')
            from l in LineParser.DelimitedBy(CommaDividerParser).Optional()
            from end in Parse.Char(']')
            select new ListToken(l.IsEmpty ? Array.Empty<Token[]>() : l.Get().ToArray());

        static readonly Parser<Token> ElementParser =
            NumberParser.Or(SignedNumberParser).Or(TextParser).Or(CommandParser).Or(FunctionParser).Or(ListParser).Or(GroupParser);

        static readonly Parser<Token[]> LineParser =
            from startSpace in Parse.WhiteSpace.Many()
            from e in ElementParser.DelimitedBy(Parse.WhiteSpace.AtLeastOnce())
            from endSpace in Parse.WhiteSpace.Many()
            select e.ToArray();

        public static Token[] Lexicalize(string line)
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
