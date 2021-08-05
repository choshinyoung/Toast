using System;
using System.Linq;
using Sprache;
using Toast.Exceptions;
using Toast.Tokens;

namespace Toast
{
    public class ToastLexer
    {
        public static readonly Parser<char> UnderLine = Parse.Char('_');
        public static readonly Parser<Token> CommandParser =
            from first in Parse.Letter.Or(UnderLine).Once().Text()
            from rest in Parse.LetterOrDigit.Or(UnderLine).Many().Text()
            select new CommandToken(first + rest);

        public static readonly Parser<Token> NumberParser =
            from n in Parse.Decimal
            select new NumberToken(long.TryParse(n, out var l) ? l : (object)float.Parse(n));

        public static readonly Parser<Token> SignedNumberParser =
            from s in Parse.Char('+').Or(Parse.Char('-'))
            from n in NumberParser
            select s == '+' ? n : new NumberToken((n.GetValue() is float m ? m : (object)((long)n.GetValue() * -1)));

        public static readonly Parser<object> BraceStartParser = Parse.Char('{').Select(c => (object)c);
        public static readonly Parser<object> BraceEndParser = Parse.Char('}').Select(c => (object)c);
        public static readonly Parser<object> SlashBracesStartParser = Parse.String("\\{").Select(c => (object)'{');
        public static readonly Parser<object> SlashBracesEndParser = Parse.String("\\}").Select(c => (object)'}');
        public static readonly Parser<object> TextInterpolationParser =
            from start in BraceStartParser
            from node in LineParser
            from end in BraceEndParser
            select node;

        public static readonly Parser<object> DoubleQuoteParser = Parse.Char('"').Select(c => (object)c);
        public static readonly Parser<object> SlashDoubleQuoteParser = Parse.String("\\\"").Select(c => (object)'"');
        public static readonly Parser<Token> DoubleQuoteTextParser =
            from start in DoubleQuoteParser
            from s in SlashDoubleQuoteParser.Or(SlashBracesStartParser).Or(SlashBracesEndParser).Or(TextInterpolationParser).Or(Parse.AnyChar.Select(c => (object)c).Except(BraceStartParser).Except(BraceEndParser)).Except(DoubleQuoteParser).Many()
            from end in DoubleQuoteParser
            select new TextToken(s.ToArray());

        public static readonly Parser<object> SingleQuoteParser = Parse.Char('\'').Select(c => (object)c);
        public static readonly Parser<object> SlashSingleQuoteParser = Parse.String("\\'").Select(c => '\'').Select(c => (object)c);
        public static readonly Parser<Token> SingleQuoteTextParser =
            from start in SingleQuoteParser
            from s in SlashSingleQuoteParser.Or(SlashBracesStartParser).Or(SlashBracesEndParser).Or(TextInterpolationParser).Or(Parse.AnyChar.Select(c => (object)c).Except(BraceStartParser).Except(BraceEndParser)).Except(SingleQuoteParser).Many()
            from end in SingleQuoteParser
            select new TextToken(s.ToArray());

        public static readonly Parser<Token> TextParser = DoubleQuoteTextParser.Or(SingleQuoteTextParser);

        public static readonly Parser<Token> GroupParser =
            from start in Parse.Char('(')
            from g in LineParser.DelimitedBy(CommaDividerParser)
            from end in Parse.Char(')')
            select new GroupToken(g.ToArray());

        public static readonly Parser<string[]> FunctionParameterParser =
            from start in Parse.Char('(')
            from startSpace in Parse.WhiteSpace.Many()
            from g in CommandParser.DelimitedBy(CommaDividerParser).Optional()
            from endSpace in Parse.WhiteSpace.Many()
            from end in Parse.Char(')')
            select g.IsEmpty ? Array.Empty<string>() : g.Get().Select(c => ((CommandToken)c).GetValue()).ToArray();

        public static readonly Parser<Token> FunctionParser =
            from parameters in FunctionParameterParser
            from space in Parse.WhiteSpace.Many()
            from start in Parse.Char('{')
            from startSpace in Parse.WhiteSpace.Many()
            from g in LineParser.DelimitedBy(Parse.LineEnd.Or(Parse.String(";")).AtLeastOnce()).Optional()
            from endSpace in Parse.WhiteSpace.Many()
            from end in Parse.Char('}')
            select new FunctionToken(g.IsEmpty ? Array.Empty<Token[]>() : g.Get().ToArray(), parameters);
        
        public static readonly Parser<char> CommaDividerParser =
            from startSpace in Parse.WhiteSpace.Many()
            from comma in Parse.Char(',')
            from endSpace in Parse.WhiteSpace.Many()
            select comma;

        public static readonly Parser<Token> ListParser =
            from start in Parse.Char('[')
            from l in LineParser.DelimitedBy(CommaDividerParser).Optional()
            from end in Parse.Char(']')
            select new ListToken(l.IsEmpty ? Array.Empty<Token[]>() : l.Get().ToArray());

        public static readonly Parser<Token> ElementParser =
            NumberParser.Or(SignedNumberParser).Or(TextParser).Or(CommandParser).Or(FunctionParser).Or(ListParser).Or(GroupParser);

        public static readonly Parser<Token[]> LineParser =
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
