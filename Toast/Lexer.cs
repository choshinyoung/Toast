using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace Toast;

public sealed record Token(TokenKind Kind, string? Value);

public enum TokenKind
{
    Identifier,
    Symbol,
    Integer,
    Float,
    String,
    NewLine,
    LParen,
    RParen,
    LBrace,
    RBrace,
    LDoubleBrace,
    RDoubleBrace,
    LBracket,
    RBracket,
    Comma,
}

public static class Lexer
{
    private static readonly Tokenizer<TokenKind> Tokenizer = new TokenizerBuilder<TokenKind>()
        .Ignore(Span.WithAll(IsHorizontalWhitespace))
        .Ignore(Comment.CPlusPlusStyle)
        .Ignore(Comment.CStyle)
        .Match(Span.Regex(@"(?:\r\n|\n|\r)"), TokenKind.NewLine)
        .Match(CustomStringParser('"'), TokenKind.String)
        .Match(CustomStringParser('\''), TokenKind.String)
        .Match(Span.Regex(@"(?:\d+\.\d+|\.\d+)"), TokenKind.Float)
        .Match(Numerics.Natural, TokenKind.Integer)
        .Match(Span.Regex(@"[A-Za-z_][A-Za-z0-9_]*"), TokenKind.Identifier)
        .Match(Character.EqualTo('('), TokenKind.LParen)
        .Match(Character.EqualTo(')'), TokenKind.RParen)
        .Match(Span.EqualTo("{{"), TokenKind.LDoubleBrace)
        .Match(Span.EqualTo("}}"), TokenKind.RDoubleBrace)
        .Match(Character.EqualTo('{'), TokenKind.LBrace)
        .Match(Character.EqualTo('}'), TokenKind.RBrace)
        .Match(Character.EqualTo('['), TokenKind.LBracket)
        .Match(Character.EqualTo(']'), TokenKind.RBracket)
        .Match(Character.EqualTo(','), TokenKind.Comma)
        .Match(Span.WithAll("!@#$%^&*-=+.?/<>|:~`".Contains), TokenKind.Symbol)
        .Build();

    public static List<Token> Tokenize(string source)
    {
        var tokens = Tokenizer.Tokenize(source);
        return [.. tokens.Select(x => new Token(x.Kind, x.HasValue ? x.ToStringValue() : null))];
    }

    private static bool IsHorizontalWhitespace(char ch) => ch is ' ' or '	' or '\f' or '\v';

    private static TextParser<TextSpan> CustomStringParser(char quote) => input =>
    {
        if (input.IsAtEnd)
        {
            return Result.Empty<TextSpan>(input);
        }

        var str = input.Source;
        if (str == null)
        {
            return Result.Empty<TextSpan>(input);
        }

        int start = input.Position.Absolute;
        if (start >= str.Length || str[start] != quote)
        {
            return Result.Empty<TextSpan>(input);
        }

        int i = start + 1;
        int len = str.Length;
        int braceDepth = 0;
        bool inString = false;
        char inStringQuote = '\0';

        while (i < len)
        {
            char c = str[i];

            if (braceDepth > 0)
            {
                if (inString)
                {
                    if (c == '\\' && i + 1 < len)
                    {
                        i += 2;
                        continue;
                    }
                    if (c == inStringQuote)
                    {
                        inString = false;
                    }
                }
                else
                {
                    if (c == '"' || c == '\'')
                    {
                        inString = true;
                        inStringQuote = c;
                    }
                    else if (c == '{')
                    {
                        braceDepth++;
                    }
                    else if (c == '}')
                    {
                        braceDepth--;
                    }
                }
                i++;
                continue;
            }

            if (c == '\\' && i + 1 < len)
            {
                i += 2;
                continue;
            }

            if (c == '{')
            {
                braceDepth++;
                i++;
                continue;
            }

            if (c == quote)
            {
                i++;
                var remainder = input.Skip(i - start);
                return Result.Value(input.Until(remainder), input, remainder);
            }

            i++;
        }

        return Result.Empty<TextSpan>(input, $"Unterminated string literal starting with '{quote}'.");
    };
}
