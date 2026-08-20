using System.Collections.Immutable;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Toast.LanguageServer.Handlers;

public class SemanticTokensHandler : ISemanticTokensFullHandler
{
    private static readonly string[] TokenTypes =
    [
        SemanticTokenType.Keyword,
        SemanticTokenType.Function,
        SemanticTokenType.Variable,
        SemanticTokenType.Type,
        SemanticTokenType.Class,
        SemanticTokenType.Parameter,
        SemanticTokenType.Property,
        SemanticTokenType.String,
        SemanticTokenType.Number,
        SemanticTokenType.Operator,
        SemanticTokenType.Comment,
    ];

    private static readonly string[] TokenModifiers =
    [
        SemanticTokenModifier.Declaration,
        SemanticTokenModifier.Readonly,
        SemanticTokenModifier.DefaultLibrary,
    ];

    public static readonly SemanticTokensLegend Legend = new()
    {
        TokenTypes = new Container<SemanticTokenType>(
            TokenTypes.Select(t => new SemanticTokenType(t))
        ),
        TokenModifiers = new Container<SemanticTokenModifier>(
            TokenModifiers.Select(m => new SemanticTokenModifier(m))
        ),
    };

    private static readonly Dictionary<string, int> TokenTypeIndices = TokenTypes
        .Select((t, i) => (t, i))
        .ToDictionary(x => x.t, x => x.i);

    public SemanticTokensRegistrationOptions GetRegistrationOptions(
        SemanticTokensCapability capability,
        ClientCapabilities clientCapabilities
    )
    {
        return new SemanticTokensRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("toast"),
            Legend = Legend,
            Full = new SemanticTokensCapabilityRequestFull { Delta = false },
            Range = false,
        };
    }

    public Task<SemanticTokens?> Handle(
        SemanticTokensParams request,
        CancellationToken cancellationToken
    )
    {
        var uri = request.TextDocument.Uri;
        var text = DocumentManager.Instance.GetDocument(uri);
        if (string.IsNullOrEmpty(text))
        {
            return Task.FromResult<SemanticTokens?>(
                new SemanticTokens { Data = ImmutableArray<int>.Empty }
            );
        }

        var toaster = new Toaster();
        var availableSymbols = ScopeAnalyzer.GetAvailableSymbols(text, toaster);
        var symbols = availableSymbols.ToDictionary(s => s.Name, s => s);

        List<Token> tokens;
        try
        {
            tokens = Lexer.Tokenize(text);
        }
        catch
        {
            return Task.FromResult<SemanticTokens?>(
                new SemanticTokens { Data = ImmutableArray<int>.Empty }
            );
        }

        var rawTokens = new List<RawSemanticToken>();

        for (int i = 0; i < tokens.Count; i++)
        {
            var tok = tokens[i];
            if (tok.Kind == TokenKind.NewLine || string.IsNullOrEmpty(tok.Value))
                continue;

            if (tok.Kind == TokenKind.String)
            {
                ProcessStringToken(tok, symbols, toaster, rawTokens);
            }
            else
            {
                int lineIndex = tok.Location.Line - 1; // 0-based
                int charIndex = tok.Location.Column - 1; // 0-based
                int length = tok.Value.Length;
                string? tokenType = ClassifyToken(tok, i, tokens, symbols, toaster);

                if (tokenType != null)
                {
                    rawTokens.Add(new RawSemanticToken(lineIndex, charIndex, length, tokenType));
                }
            }
        }

        // Sort tokens by line and column
        rawTokens.Sort(
            (a, b) =>
            {
                int lineCmp = a.Line.CompareTo(b.Line);
                return lineCmp != 0 ? lineCmp : a.Column.CompareTo(b.Column);
            }
        );

        var data = new List<int>();
        int lastLine = 0;
        int lastChar = 0;

        foreach (var item in rawTokens)
        {
            if (TokenTypeIndices.TryGetValue(item.TokenType, out var typeIdx))
            {
                int deltaLine = item.Line - lastLine;
                int deltaChar = deltaLine == 0 ? item.Column - lastChar : item.Column;

                data.Add(deltaLine);
                data.Add(deltaChar);
                data.Add(item.Length);
                data.Add(typeIdx);
                data.Add(item.Modifiers);

                lastLine = item.Line;
                lastChar = item.Column;
            }
        }

        return Task.FromResult<SemanticTokens?>(
            new SemanticTokens { Data = ImmutableArray.CreateRange(data) }
        );
    }

    private static string? ClassifyToken(
        Token tok,
        int i,
        IReadOnlyList<Token> tokens,
        Dictionary<string, DeclaredSymbol> symbols,
        Toaster toaster
    )
    {
        return tok.Kind switch
        {
            TokenKind.String => SemanticTokenType.String,
            TokenKind.Integer or TokenKind.Float => SemanticTokenType.Number,
            TokenKind.Symbol => SemanticTokenType.Operator,
            TokenKind.Identifier => ClassifyIdentifier(tok.Value!, i, tokens, symbols, toaster),
            _ => null,
        };
    }

    private static string ClassifyIdentifier(
        string val,
        int i,
        IReadOnlyList<Token> tokens,
        Dictionary<string, DeclaredSymbol> symbols,
        Toaster toaster
    )
    {
        bool isMemberAccess =
            i > 0
            && tokens[i - 1].Kind == TokenKind.Symbol
            && (tokens[i - 1].Value == "." || tokens[i - 1].Value == "..");
        bool isCall = i + 1 < tokens.Count && tokens[i + 1].Kind == TokenKind.LParen;

        if (isMemberAccess)
        {
            return isCall ? SemanticTokenType.Function : SemanticTokenType.Property;
        }

        if (symbols.TryGetValue(val, out var sym))
        {
            return sym.Kind switch
            {
                "type" or "class" => SemanticTokenType.Class,
                "function" => SemanticTokenType.Function,
                "parameter" => SemanticTokenType.Parameter,
                _ => isCall ? SemanticTokenType.Function : SemanticTokenType.Variable,
            };
        }

        if (
            toaster.PrefixCommands.ContainsKey(val)
            || toaster.InfixCommands.ContainsKey(val)
            || isCall
        )
        {
            return SemanticTokenType.Function;
        }

        return SemanticTokenType.Variable;
    }

    private static void ProcessStringToken(
        Token stringTok,
        Dictionary<string, DeclaredSymbol> symbols,
        Toaster toaster,
        List<RawSemanticToken> resultTokens
    )
    {
        string str = stringTok.Value!;
        int line = stringTok.Location.Line - 1;
        int baseCol = stringTok.Location.Column - 1;

        if (!str.Contains('{'))
        {
            resultTokens.Add(
                new RawSemanticToken(line, baseCol, str.Length, SemanticTokenType.String)
            );
            return;
        }

        int quoteOffset = (str.StartsWith('"') || str.StartsWith('\'')) ? 1 : 0;
        string content =
            (quoteOffset == 1 && str.Length >= 2 && str.EndsWith(str[0])) ? str[1..^1] : str;

        int currentChunkStart = 0;
        int i = 0;
        int len = content.Length;

        while (i < len)
        {
            if (content[i] == '\\' && i + 1 < len)
            {
                i += 2;
                continue;
            }

            if (content[i] == '{')
            {
                int literalLen = i - currentChunkStart;
                int chunkCol =
                    baseCol + (currentChunkStart == 0 ? 0 : quoteOffset + currentChunkStart);
                int chunkLen = (currentChunkStart == 0 ? quoteOffset : 0) + literalLen;
                if (chunkLen > 0)
                {
                    resultTokens.Add(
                        new RawSemanticToken(line, chunkCol, chunkLen, SemanticTokenType.String)
                    );
                }

                int startExpr = i + 1;
                int depth = 1;
                int j = startExpr;
                bool inSubStr = false;
                char subQuote = '\0';

                while (j < len && depth > 0)
                {
                    char c = content[j];
                    if (inSubStr)
                    {
                        if (c == '\\' && j + 1 < len)
                        {
                            j += 2;
                            continue;
                        }
                        if (c == subQuote)
                            inSubStr = false;
                    }
                    else
                    {
                        if (c == '"' || c == '\'')
                        {
                            inSubStr = true;
                            subQuote = c;
                        }
                        else if (c == '{')
                            depth++;
                        else if (c == '}')
                            depth--;
                    }
                    if (depth > 0)
                        j++;
                }

                // '{' operator token
                int openBraceCol = baseCol + quoteOffset + i;
                resultTokens.Add(
                    new RawSemanticToken(line, openBraceCol, 1, SemanticTokenType.Operator)
                );

                if (depth == 0)
                {
                    string exprStr = content[startExpr..j];
                    if (!string.IsNullOrWhiteSpace(exprStr))
                    {
                        try
                        {
                            var innerTokens = Lexer.Tokenize(exprStr);
                            int exprCol = baseCol + quoteOffset + startExpr;
                            for (int k = 0; k < innerTokens.Count; k++)
                            {
                                var it = innerTokens[k];
                                if (it.Kind == TokenKind.NewLine || string.IsNullOrEmpty(it.Value))
                                    continue;
                                int itLine = line + (it.Location.Line - 1);
                                int itCol = (
                                    it.Location.Line == 1
                                        ? exprCol + (it.Location.Column - 1)
                                        : it.Location.Column - 1
                                );

                                string? itType = ClassifyToken(
                                    it,
                                    k,
                                    innerTokens,
                                    symbols,
                                    toaster
                                );
                                if (itType != null)
                                {
                                    resultTokens.Add(
                                        new RawSemanticToken(
                                            itLine,
                                            itCol,
                                            it.Value!.Length,
                                            itType
                                        )
                                    );
                                }
                            }
                        }
                        catch
                        {
                            // Incomplete interpolation parsing ignored
                        }
                    }

                    // '}' operator token
                    int closeBraceCol = baseCol + quoteOffset + j;
                    resultTokens.Add(
                        new RawSemanticToken(line, closeBraceCol, 1, SemanticTokenType.Operator)
                    );
                    i = j + 1;
                    currentChunkStart = i;
                    continue;
                }
                else
                {
                    i = len;
                    break;
                }
            }
            i++;
        }

        int remainingLiteral = len - currentChunkStart;
        if (remainingLiteral > 0 || quoteOffset > 0)
        {
            int chunkCol = baseCol + quoteOffset + currentChunkStart;
            int chunkLen = remainingLiteral + quoteOffset;
            if (chunkLen > 0)
            {
                resultTokens.Add(
                    new RawSemanticToken(line, chunkCol, chunkLen, SemanticTokenType.String)
                );
            }
        }
    }

    private record RawSemanticToken(
        int Line,
        int Column,
        int Length,
        string TokenType,
        int Modifiers = 0
    );
}
