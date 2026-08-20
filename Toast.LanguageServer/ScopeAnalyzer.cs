using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

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
    public static List<DeclaredSymbol> GetAvailableSymbols(string source, Toaster toaster)
    {
        var symbols = new Dictionary<string, DeclaredSymbol>();

        if (!string.IsNullOrWhiteSpace(source))
        {
            var scope = new StaticScope(null);

            // First attempt to parse whole source
            try
            {
                var tokens = Lexer.Tokenize(source);
                var program = Parser.Parse(
                    tokens,
                    toaster.GetInfixInfo,
                    toaster.IsPrefix,
                    toaster,
                    toaster.GlobalContext
                );
                CollectStaticSymbols(program, scope, toaster, symbols);
            }
            catch
            {
                // If whole source fails due to incomplete typing (e.g. `p.`), parse line-by-line incrementally
                var lines = source.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var tokens = Lexer.Tokenize(line);
                        var stmt = Parser.Parse(
                            tokens,
                            toaster.GetInfixInfo,
                            toaster.IsPrefix,
                            toaster,
                            toaster.GlobalContext
                        );
                        CollectStaticSymbols(stmt, scope, toaster, symbols);
                    }
                    catch
                    {
                        // Incomplete line ignored
                    }
                }
            }
        }

        // Collect all registered commands from Toaster (including newly imported modules)
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

        // Collect all global context bindings (including newly imported types/variables)
        foreach (var (k, v) in toaster.GlobalContext.GetBindings())
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

        return [.. symbols.Values];
    }

    public static List<Diagnostic> ValidateDocumentStatically(string text, Toaster toaster)
    {
        var diagnostics = new List<Diagnostic>();

        if (string.IsNullOrWhiteSpace(text))
            return diagnostics;

        List<Token> allTokens;
        try
        {
            allTokens = Lexer.Tokenize(text);
        }
        catch (ToastException ex)
        {
            var loc = ex.Error.Location;
            int line = Math.Max(0, loc.Line - 1);
            int col = Math.Max(0, loc.Column - 1);

            diagnostics.Add(
                new Diagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Range = new LspRange(new Position(line, col), new Position(line, col + 10)),
                    Message = $"[{ex.Error.ErrorType}] {ex.Error.Message}",
                    Source = "Toast",
                }
            );
            return diagnostics;
        }

        // 1. Resilient parse from Parser
        var parseResult = Parser.ParseResilient(
            allTokens,
            toaster.GetInfixInfo,
            toaster.IsPrefix,
            toaster,
            toaster.GlobalContext
        );

        // 2. Syntax errors collected directly by Parser
        foreach (var error in parseResult.Errors)
        {
            var loc = error.Location;
            int line = Math.Max(0, loc.Line - 1);
            int col = Math.Max(0, loc.Column - 1);

            diagnostics.Add(
                new Diagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Range = new LspRange(new Position(line, col), new Position(line, col + 5)),
                    Message = $"[{error.ErrorType}] {error.Message}",
                    Source = "Toast",
                }
            );
        }

        // 3. Static scope analysis on parsed AST
        var globalScope = new StaticScope(null);
        CheckNodeScope(parseResult.Program, globalScope, toaster, diagnostics);

        return diagnostics;
    }

    private static void CollectStaticSymbols(
        Node node,
        StaticScope scope,
        Toaster toaster,
        Dictionary<string, DeclaredSymbol> symbols
    )
    {
        if (node is ProgramNode prog)
        {
            foreach (var stmt in prog.Statements)
            {
                CollectStaticSymbols(stmt, scope, toaster, symbols);
            }
        }
        else if (node is CallNode call)
        {
            if (call.Callee is IdentifierNode id)
            {
                if (id.Name == "=" && call.Arguments.Count == 2)
                {
                    var left = call.Arguments[0];
                    var right = call.Arguments[1];
                    var varName = ExtractDeclaredIdentifier(left);

                    if (!string.IsNullOrEmpty(varName))
                    {
                        var (inferredType, members) = InferTypeAndMembers(right, symbols);
                        scope.Declare(varName);
                        symbols[varName] = new DeclaredSymbol(
                            varName,
                            inferredType,
                            "variable",
                            Members: members
                        );
                    }
                }
                else if (id.Name == "var" && call.Arguments.Count > 0)
                {
                    var varName = ExtractDeclaredIdentifier(call.Arguments[0]);
                    if (!string.IsNullOrEmpty(varName))
                    {
                        scope.Declare(varName);
                        if (!symbols.ContainsKey(varName))
                        {
                            symbols[varName] = new DeclaredSymbol(
                                varName,
                                ToastType.Any,
                                "variable"
                            );
                        }
                    }
                }
                else if (id.Name == "function" && call.Arguments.Count >= 2)
                {
                    var fnName = ExtractDeclaredIdentifier(call.Arguments[0]);
                    if (!string.IsNullOrEmpty(fnName))
                    {
                        var fnParams = call.Arguments[1] is FunctionNode fn
                            ? fn.Parameters.Select(p => p.Name).ToList()
                            : [];
                        scope.Declare(fnName);
                        symbols[fnName] = new DeclaredSymbol(
                            fnName,
                            ToastType.Function,
                            "function",
                            Parameters: fnParams
                        );
                    }
                }
                else if (id.Name == "class" && call.Arguments.Count >= 2)
                {
                    var clsName = ExtractDeclaredIdentifier(call.Arguments[0]);
                    if (!string.IsNullOrEmpty(clsName))
                    {
                        var members = call.Arguments[1] is FunctionNode fn
                            ? new HashSet<string>(fn.Parameters.Select(p => p.Name))
                            : [];
                        scope.Declare(clsName);
                        symbols[clsName] = new DeclaredSymbol(
                            clsName,
                            new ToastType(clsName),
                            "type",
                            Members: members
                        );
                    }
                }
            }

            foreach (var arg in call.Arguments)
            {
                CollectStaticSymbols(arg, scope, toaster, symbols);
            }
        }
        else if (node is FunctionNode fnNode)
        {
            var fnScope = new StaticScope(scope);
            foreach (var param in fnNode.Parameters)
            {
                fnScope.Declare(param.Name);
                symbols[param.Name] = new DeclaredSymbol(param.Name, ToastType.Any, "parameter");
            }
            foreach (var stmt in fnNode.Statements)
            {
                CollectStaticSymbols(stmt, fnScope, toaster, symbols);
            }
        }
        else if (node is GroupNode gn)
        {
            foreach (var item in gn.Items)
            {
                CollectStaticSymbols(item, scope, toaster, symbols);
            }
        }
        else if (node is InterpolatedStringNode interp)
        {
            foreach (var part in interp.Parts)
            {
                CollectStaticSymbols(part, scope, toaster, symbols);
            }
        }
    }

    private static (ToastType Type, HashSet<string>? Members) InferTypeAndMembers(
        Node node,
        Dictionary<string, DeclaredSymbol> symbols
    )
    {
        if (node is LiteralNode lit)
        {
            return (lit.Type, null);
        }

        if (node is CallNode call && call.Callee is IdentifierNode calleeId)
        {
            if (symbols.TryGetValue(calleeId.Name, out var sym) && sym.Kind == "type")
            {
                return (sym.Type, sym.Members != null ? new HashSet<string>(sym.Members) : null);
            }
        }

        return (ToastType.Any, null);
    }

    private static void CheckNodeScope(
        Node node,
        StaticScope scope,
        Toaster toaster,
        List<Diagnostic> diagnostics
    )
    {
        if (node is ProgramNode prog)
        {
            foreach (var stmt in prog.Statements)
            {
                CheckNodeScope(stmt, scope, toaster, diagnostics);
            }
        }
        else if (node is CallNode call)
        {
            if (call.Callee is IdentifierNode calleeId)
            {
                if (calleeId.Name == "=" && call.Arguments.Count == 2)
                {
                    var varName = ExtractDeclaredIdentifier(call.Arguments[0]);
                    if (!string.IsNullOrEmpty(varName))
                    {
                        scope.Declare(varName);
                    }
                    CheckNodeScope(call.Arguments[1], scope, toaster, diagnostics);
                    return;
                }

                if (calleeId.Name == "var" && call.Arguments.Count > 0)
                {
                    var varName = ExtractDeclaredIdentifier(call.Arguments[0]);
                    if (!string.IsNullOrEmpty(varName))
                    {
                        scope.Declare(varName);
                    }
                    for (int i = 1; i < call.Arguments.Count; i++)
                    {
                        CheckNodeScope(call.Arguments[i], scope, toaster, diagnostics);
                    }
                    return;
                }

                if (calleeId.Name == "function" && call.Arguments.Count >= 2)
                {
                    var fnName = ExtractDeclaredIdentifier(call.Arguments[0]);
                    if (!string.IsNullOrEmpty(fnName))
                    {
                        scope.Declare(fnName);
                    }
                    CheckNodeScope(call.Arguments[1], scope, toaster, diagnostics);
                    return;
                }

                if (calleeId.Name == "class" && call.Arguments.Count >= 2)
                {
                    var clsName = ExtractDeclaredIdentifier(call.Arguments[0]);
                    if (!string.IsNullOrEmpty(clsName))
                    {
                        scope.Declare(clsName);
                    }
                    CheckNodeScope(call.Arguments[1], scope, toaster, diagnostics);
                    return;
                }

                // Check callee itself
                CheckIdentifierResolved(calleeId, scope, toaster, diagnostics);

                // Check arguments dynamically based on command parameter type metadata
                var cmd = FindCommand(calleeId.Name, toaster);
                for (int i = 0; i < call.Arguments.Count; i++)
                {
                    var expectedType =
                        (cmd != null && i < cmd.Parameters.Count)
                            ? cmd.Parameters[i].Type
                            : ToastType.Any;

                    // Identifier and AstNode parameters are consumed as raw metadata rather than evaluated as variables
                    if (expectedType == ToastType.Identifier || expectedType == ToastType.AstNode)
                    {
                        continue;
                    }

                    CheckNodeScope(call.Arguments[i], scope, toaster, diagnostics);
                }
                return;
            }
            else
            {
                CheckNodeScope(call.Callee, scope, toaster, diagnostics);
                foreach (var arg in call.Arguments)
                {
                    CheckNodeScope(arg, scope, toaster, diagnostics);
                }
            }
        }
        else if (node is FunctionNode fnNode)
        {
            var fnScope = new StaticScope(scope);
            foreach (var param in fnNode.Parameters)
            {
                fnScope.Declare(param.Name);
            }
            foreach (var stmt in fnNode.Statements)
            {
                CheckNodeScope(stmt, fnScope, toaster, diagnostics);
            }
        }
        else if (node is GroupNode gn)
        {
            var groupScope = new StaticScope(scope);
            foreach (var item in gn.Items)
            {
                CheckNodeScope(item, groupScope, toaster, diagnostics);
            }
        }
        else if (node is ListNode ln)
        {
            foreach (var elem in ln.Items)
            {
                CheckNodeScope(elem, scope, toaster, diagnostics);
            }
        }
        else if (node is InterpolatedStringNode interp)
        {
            foreach (var part in interp.Parts)
            {
                CheckNodeScope(part, scope, toaster, diagnostics);
            }
        }
        else if (node is IdentifierNode idNode)
        {
            CheckIdentifierResolved(idNode, scope, toaster, diagnostics);
        }
    }

    private static void CheckIdentifierResolved(
        IdentifierNode idNode,
        StaticScope scope,
        Toaster toaster,
        List<Diagnostic> diagnostics
    )
    {
        var name = idNode.Name;

        if (scope.IsDeclared(name))
            return;

        if (toaster.GlobalContext.HasVariable(name))
            return;

        if (toaster.PrefixCommands.ContainsKey(name) || toaster.InfixCommands.ContainsKey(name))
            return;

        var loc = idNode.Location;
        int line = Math.Max(0, loc.Line - 1);
        int col = Math.Max(0, loc.Column - 1);
        int len = name.Length;

        diagnostics.Add(
            new Diagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Range = new LspRange(new Position(line, col), new Position(line, col + len)),
                Message = $"[RuntimeError] Variable or command '{name}' is not defined.",
                Source = "Toast",
            }
        );
    }

    private static string? ExtractDeclaredIdentifier(Node node)
    {
        if (node is IdentifierNode id)
            return id.Name;

        if (node is CallNode call)
        {
            if (call.Callee is IdentifierNode cid)
            {
                if (cid.Name == "var" && call.Arguments.Count > 0)
                {
                    return ExtractDeclaredIdentifier(call.Arguments[0]);
                }
                if (cid.Name == ":" && call.Arguments.Count > 0)
                {
                    return ExtractDeclaredIdentifier(call.Arguments[0]);
                }
            }
        }

        return null;
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

    private class StaticScope(StaticScope? parent)
    {
        private readonly HashSet<string> _declarations = [];
        private readonly StaticScope? _parent = parent;

        public void Declare(string name) => _declarations.Add(name);

        public bool IsDeclared(string name)
        {
            if (_declarations.Contains(name))
                return true;
            return _parent != null && _parent.IsDeclared(name);
        }
    }

    private static Command? FindCommand(string name, Toaster toaster)
    {
        if (toaster.PrefixCommands.TryGetValue(name, out var prefixCmd))
            return prefixCmd;
        if (toaster.InfixCommands.TryGetValue(name, out var infixCmd))
            return infixCmd;
        if (
            toaster.GlobalContext.GetBindings().TryGetValue(name, out var binding)
            && binding.Value is CommandValue cv
        )
            return cv.Command;
        return null;
    }
}
