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

        var toaster = DocumentManager.Instance.GetToasterForDocument(uri);
        var availableSymbols = ScopeAnalyzer.GetAvailableSymbols(
            text,
            new Position(int.MaxValue, int.MaxValue),
            toaster
        );
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

        var data = new List<int>();
        int lastLine = 0;
        int lastChar = 0;

        foreach (var tok in tokens)
        {
            if (tok.Kind == TokenKind.NewLine || string.IsNullOrEmpty(tok.Value))
                continue;

            int lineIndex = tok.Location.Line - 1; // 0-based
            int charIndex = tok.Location.Column - 1; // 0-based
            int length = tok.Value.Length;

            string? tokenType = null;

            switch (tok.Kind)
            {
                case TokenKind.String:
                    tokenType = SemanticTokenType.String;
                    break;
                case TokenKind.Integer:
                case TokenKind.Float:
                    tokenType = SemanticTokenType.Number;
                    break;
                case TokenKind.Symbol:
                    tokenType = SemanticTokenType.Operator;
                    break;
                case TokenKind.Identifier:
                    var val = tok.Value;
                    if (symbols.TryGetValue(val, out var sym))
                    {
                        tokenType = sym.Kind switch
                        {
                            "type" or "class" => SemanticTokenType.Class,
                            "function" => SemanticTokenType.Function,
                            "parameter" => SemanticTokenType.Parameter,
                            _ => SemanticTokenType.Variable,
                        };
                    }
                    else if (
                        toaster.PrefixCommands.ContainsKey(val)
                        || toaster.InfixCommands.ContainsKey(val)
                    )
                    {
                        tokenType = SemanticTokenType.Function;
                    }
                    else
                    {
                        tokenType = SemanticTokenType.Variable;
                    }
                    break;
            }

            if (tokenType != null && TokenTypeIndices.TryGetValue(tokenType, out var typeIdx))
            {
                int deltaLine = lineIndex - lastLine;
                int deltaChar = deltaLine == 0 ? charIndex - lastChar : charIndex;

                data.Add(deltaLine);
                data.Add(deltaChar);
                data.Add(length);
                data.Add(typeIdx);
                data.Add(0); // modifier bitmask

                lastLine = lineIndex;
                lastChar = charIndex;
            }
        }

        return Task.FromResult<SemanticTokens?>(
            new SemanticTokens { Data = ImmutableArray.CreateRange(data) }
        );
    }
}
