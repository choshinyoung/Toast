using Superpower;
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
        .Match(QuotedString.CStyle, TokenKind.String)
        .Match(QuotedString.SqlStyle, TokenKind.String)
        .Match(Span.Regex(@"(?:\d+\.\d+|\.\d+)"), TokenKind.Float)
        .Match(Numerics.Natural, TokenKind.Integer)
        .Match(Span.Regex(@"[A-Za-z_][A-Za-z0-9_]*"), TokenKind.Identifier)
        .Match(Character.EqualTo('('), TokenKind.LParen)
        .Match(Character.EqualTo(')'), TokenKind.RParen)
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
}
