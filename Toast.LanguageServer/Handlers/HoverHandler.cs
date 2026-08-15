using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Toast.LanguageServer.Handlers;

public class HoverHandler : HoverHandlerBase
{
    protected override HoverRegistrationOptions CreateRegistrationOptions(
        HoverCapability capability,
        ClientCapabilities clientCapabilities
    )
    {
        return new HoverRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("toast"),
        };
    }

    public override Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri;
        var text = DocumentManager.Instance.GetDocument(uri) ?? "";
        var toaster = DocumentManager.Instance.GetToasterForDocument(uri);

        var word = GetWordAtPosition(text, request.Position);
        if (string.IsNullOrEmpty(word))
        {
            return Task.FromResult<Hover?>(null);
        }

        // 1. Check local scope symbols
        var localSymbols = ScopeAnalyzer.GetAvailableSymbols(text, toaster);
        var localSym = localSymbols.FirstOrDefault(s => s.Name == word);
        if (localSym != null)
        {
            var content = new System.Text.StringBuilder();
            content.AppendLine("```toast");
            if (localSym.Parameters != null)
            {
                content.AppendLine(
                    $"{localSym.Name}({string.Join(", ", localSym.Parameters)}): {localSym.Type.Name}"
                );
            }
            else
            {
                content.AppendLine($"{localSym.Name}: {localSym.Type.Name}");
            }
            content.AppendLine("```");
            if (!string.IsNullOrEmpty(localSym.Description))
            {
                content.AppendLine();
                content.AppendLine(localSym.Description);
            }
            if (localSym.Members != null && localSym.Members.Count > 0)
            {
                content.AppendLine();
                content.AppendLine(
                    "**Members:** " + string.Join(", ", localSym.Members.Select(m => $"`{m}`"))
                );
            }

            return Task.FromResult<Hover?>(
                new Hover
                {
                    Contents = new MarkedStringsOrMarkupContent(
                        new MarkupContent
                        {
                            Kind = MarkupKind.Markdown,
                            Value = content.ToString().Trim(),
                        }
                    ),
                }
            );
        }

        // 2. Check Toaster commands & globals
        Command? cmd = null;
        if (toaster.PrefixCommands.TryGetValue(word, out var prefixCmd))
            cmd = prefixCmd;
        else if (toaster.InfixCommands.TryGetValue(word, out var infixCmd))
            cmd = infixCmd;
        else if (toaster.GlobalContext.GetBindings().TryGetValue(word, out var binding))
        {
            if (binding.Value is CommandValue cv)
                cmd = cv.Command;
            else if (binding.Value is TypeValue tv)
            {
                var typeDoc = new System.Text.StringBuilder();
                typeDoc.AppendLine("```toast");
                typeDoc.AppendLine($"type {word}");
                typeDoc.AppendLine("```");
                if (tv.DeclaredMembers.Count > 0)
                {
                    typeDoc.AppendLine();
                    typeDoc.AppendLine("**Declared Members:**");
                    foreach (var m in tv.DeclaredMembers)
                    {
                        typeDoc.AppendLine($"- `{m}`");
                    }
                }
                return Task.FromResult<Hover?>(
                    new Hover
                    {
                        Contents = new MarkedStringsOrMarkupContent(
                            new MarkupContent
                            {
                                Kind = MarkupKind.Markdown,
                                Value = typeDoc.ToString().Trim(),
                            }
                        ),
                    }
                );
            }
            else
            {
                var varDoc =
                    $"```toast\n{word}: {binding.Value.Type.Name}\n```\n\nGlobal value `{word}`";
                return Task.FromResult<Hover?>(
                    new Hover
                    {
                        Contents = new MarkedStringsOrMarkupContent(
                            new MarkupContent { Kind = MarkupKind.Markdown, Value = varDoc }
                        ),
                    }
                );
            }
        }

        if (cmd != null)
        {
            var content = new System.Text.StringBuilder();
            content.AppendLine("```toast");
            content.AppendLine(cmd.GetSignature());
            content.AppendLine("```");

            if (!string.IsNullOrEmpty(cmd.Description))
            {
                content.AppendLine();
                content.AppendLine(cmd.Description);
            }

            if (cmd.Parameters.Count > 0)
            {
                content.AppendLine();
                content.AppendLine("**Parameters:**");
                foreach (var p in cmd.Parameters)
                {
                    content.AppendLine($"- `{p.Name}` ({p.Type.Name})");
                }
            }

            if (cmd.ReturnType != ToastType.Any)
            {
                content.AppendLine();
                content.AppendLine($"**Returns:** `{cmd.ReturnType.Name}`");
            }

            return Task.FromResult<Hover?>(
                new Hover
                {
                    Contents = new MarkedStringsOrMarkupContent(
                        new MarkupContent
                        {
                            Kind = MarkupKind.Markdown,
                            Value = content.ToString().Trim(),
                        }
                    ),
                }
            );
        }

        return Task.FromResult<Hover?>(null);
    }

    private static string? GetWordAtPosition(string text, Position position)
    {
        var lines = text.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        if (position.Line >= lines.Length)
            return null;

        var line = lines[position.Line];
        var col = position.Character;
        if (col > line.Length)
            col = line.Length;

        // Find symbol or identifier boundaries around cursor
        int start = col;
        while (start > 0 && (char.IsLetterOrDigit(line[start - 1]) || line[start - 1] == '_'))
        {
            start--;
        }

        int end = col;
        while (end < line.Length && (char.IsLetterOrDigit(line[end]) || line[end] == '_'))
        {
            end++;
        }

        if (start < end)
        {
            return line[start..end];
        }

        // If not identifier, check for operator symbols
        if (col < line.Length && "!@#$%^&*-=+.?/<>|:~`".Contains(line[col]))
        {
            int symStart = col;
            while (symStart > 0 && "!@#$%^&*-=+.?/<>|:~`".Contains(line[symStart - 1]))
                symStart--;
            int symEnd = col;
            while (symEnd < line.Length && "!@#$%^&*-=+.?/<>|:~`".Contains(line[symEnd]))
                symEnd++;
            return line[symStart..symEnd];
        }

        return null;
    }
}
