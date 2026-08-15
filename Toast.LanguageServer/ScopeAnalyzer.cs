using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Toast.LanguageServer;

public record DeclaredSymbol(
    string Name,
    ToastType Type,
    string Kind, // "variable", "function", "type", "parameter"
    IReadOnlyList<string>? Parameters = null,
    HashSet<string>? Members = null,
    string? Description = null,
    Dictionary<string, Command>? MemberCommands = null
);

public static class ScopeAnalyzer
{
    public static List<DeclaredSymbol> GetAvailableSymbols(
        string source,
        Position position,
        Toaster toaster
    )
    {
        var symbols = new Dictionary<string, DeclaredSymbol>();

        // 1. Collect all registered commands from Toaster
        foreach (var (name, cmd) in toaster.PrefixCommands)
        {
            symbols[name] = new DeclaredSymbol(
                name,
                cmd.ReturnType,
                "function",
                Parameters: cmd.Parameters.Select(p => p.Name).ToList(),
                Description: cmd.Description
            );
        }

        // 2. Collect all global context bindings
        CollectContextBindings(toaster.GlobalContext, symbols);

        // 3. Dry-run execute script in a sandbox context to dynamically capture runtime declarations
        if (!string.IsNullOrWhiteSpace(source))
        {
            var sandboxCtx = new Context(toaster, toaster.GlobalContext);
            try
            {
                var tokens = Lexer.Tokenize(source);
                var program = Parser.Parse(tokens, toaster.GetInfixInfo, toaster.IsPrefix);
                toaster.Executor.Evaluate(program, sandboxCtx);
            }
            catch
            {
                // Execution may fail during typing; bindings collected so far remain valid
            }

            CollectContextBindings(sandboxCtx, symbols);
        }

        return [.. symbols.Values];
    }

    private static void CollectContextBindings(
        Context context,
        Dictionary<string, DeclaredSymbol> symbols
    )
    {
        foreach (var (k, v) in context.GetBindings())
        {
            var val = v.Value;
            if (val is TypeValue tv)
            {
                symbols[k] = new DeclaredSymbol(
                    k,
                    tv.TargetType,
                    "type",
                    Members: tv.DeclaredMembers
                );
            }
            else if (val is FunctionValue fv)
            {
                symbols[k] = new DeclaredSymbol(
                    k,
                    ToastType.Function,
                    "function",
                    Parameters: fv.Parameters.Select(p => p.Name).ToList()
                );
            }
            else if (val is CommandValue cv)
            {
                symbols[k] = new DeclaredSymbol(
                    k,
                    cv.Command.ReturnType,
                    "function",
                    Parameters: cv.Command.Parameters.Select(p => p.Name).ToList(),
                    Description: cv.Command.Description
                );
            }
            else
            {
                symbols[k] = new DeclaredSymbol(k, val.Type, "variable");
            }
        }
    }

    public static string? GetTargetBeforeDot(string source, Position position)
    {
        List<Token> tokens;
        try
        {
            tokens = Lexer.Tokenize(source);
        }
        catch
        {
            return null;
        }

        int targetLine = position.Line + 1;
        int targetCol = position.Character + 1;

        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            var tok = tokens[i];
            if (
                tok.Location.Line == targetLine
                && tok.Kind == TokenKind.Symbol
                && tok.Value == "."
                && tok.Location.Column <= targetCol
            )
            {
                if (i > 0 && tokens[i - 1].Kind == TokenKind.Identifier)
                {
                    return tokens[i - 1].Value;
                }
            }
        }

        return null;
    }
}
