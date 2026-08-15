using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Toast.LanguageServer.Handlers;

public class CompletionHandler : CompletionHandlerBase
{
    protected override CompletionRegistrationOptions CreateRegistrationOptions(
        CompletionCapability capability,
        ClientCapabilities clientCapabilities
    )
    {
        return new CompletionRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("toast"),
            ResolveProvider = false,
            TriggerCharacters = new[] { ".", ":", "\"", "'" },
        };
    }

    public override Task<CompletionList> Handle(
        CompletionParams request,
        CancellationToken cancellationToken
    )
    {
        var uri = request.TextDocument.Uri;
        var documentText = DocumentManager.Instance.GetDocument(uri) ?? "";
        var toaster = DocumentManager.Instance.GetToasterForDocument(uri);

        var items = new List<CompletionItem>();

        // 1. Local scope symbols (variables, functions, types, parameters)
        var localSymbols = ScopeAnalyzer.GetAvailableSymbols(documentText, toaster);

        // Check if cursor is right after a dot (e.g. `obj.`)
        var dotTarget = ScopeAnalyzer.GetTargetBeforeDot(documentText, request.Position);
        if (!string.IsNullOrEmpty(dotTarget))
        {
            AddDotAccessCompletions(dotTarget, toaster, localSymbols, items);
            return Task.FromResult(new CompletionList(items));
        }

        // Check if cursor is after `import ` or `import "`
        var isImportContext = IsImportContext(documentText, request.Position, out bool inQuotes);
        if (isImportContext)
        {
            var modules = ModuleManager.Instance.GetAllModules();
            foreach (var mod in modules)
            {
                var label = inQuotes ? mod.Name : $"\"{mod.Name}\"";
                var insertText = inQuotes ? mod.Name : $"\"{mod.Name}\"";
                var kindLabel = mod.IsSystem ? "[system module]" : "[installed module]";

                items.Add(
                    new CompletionItem
                    {
                        Label = mod.Name,
                        InsertText = insertText,
                        Kind = CompletionItemKind.Module,
                        Detail = $"{mod.Name} {kindLabel}",
                        Documentation = new MarkupContent
                        {
                            Kind = MarkupKind.Markdown,
                            Value = $"**Module `{mod.Name}`**\n\n{mod.Description}",
                        },
                    }
                );
            }
            return Task.FromResult(new CompletionList(items));
        }

        foreach (var sym in localSymbols)
        {
            var kind = sym.Kind switch
            {
                "function" => CompletionItemKind.Function,
                "type" => CompletionItemKind.Class,
                "parameter" => CompletionItemKind.Variable,
                _ => CompletionItemKind.Variable,
            };

            var detail =
                sym.Parameters != null
                    ? $"{sym.Name}({string.Join(", ", sym.Parameters)}): {sym.Type.Name}"
                    : $"{sym.Name}: {sym.Type.Name}";

            items.Add(
                new CompletionItem
                {
                    Label = sym.Name,
                    Kind = kind,
                    Detail = detail,
                    Documentation = new MarkupContent
                    {
                        Kind = MarkupKind.Markdown,
                        Value = sym.Description ?? $"{sym.Kind} `{sym.Name}`",
                    },
                }
            );
        }

        // 2. Global Context Bindings (Functions, Types, Variables, Commands)
        var bindings = toaster.GlobalContext.GetBindings();
        foreach (var (name, binding) in bindings)
        {
            if (items.Any(x => x.Label == name))
                continue;

            var val = binding.Value;
            if (val is CommandValue cv)
            {
                items.Add(CreateCommandCompletionItem(cv.Command));
            }
            else if (val is TypeValue tv)
            {
                items.Add(CreateTypeCompletionItem(name, tv));
            }
            else if (val is FunctionValue fv)
            {
                var paramStrs = fv.Parameters.Select(p => p.Name);
                items.Add(
                    new CompletionItem
                    {
                        Label = name,
                        Kind = CompletionItemKind.Function,
                        Detail = $"{name}({string.Join(", ", paramStrs)})",
                        Documentation = new MarkupContent
                        {
                            Kind = MarkupKind.Markdown,
                            Value = $"Global function `{name}`",
                        },
                    }
                );
            }
            else
            {
                items.Add(
                    new CompletionItem
                    {
                        Label = name,
                        Kind = CompletionItemKind.Variable,
                        Detail = $"{name}: {val.Type.Name}",
                        Documentation = new MarkupContent
                        {
                            Kind = MarkupKind.Markdown,
                            Value = $"Global variable `{name}` of type `{val.Type.Name}`",
                        },
                    }
                );
            }
        }

        // 3. Prefix Commands (e.g. `var`, `if`, `while`, `typeof`, etc.)
        foreach (var (name, cmd) in toaster.PrefixCommands)
        {
            if (items.All(x => x.Label != name))
            {
                items.Add(CreateCommandCompletionItem(cmd));
            }
        }

        // 4. Infix Commands (e.g. `is`, `in`, `to`, `floorDiv`, `+`, etc.)
        foreach (var (name, cmd) in toaster.InfixCommands)
        {
            if (items.All(x => x.Label != name))
            {
                items.Add(CreateCommandCompletionItem(cmd));
            }
        }

        return Task.FromResult(new CompletionList(items));
    }

    private static void AddDotAccessCompletions(
        string targetName,
        Toaster toaster,
        IReadOnlyList<DeclaredSymbol> localSymbols,
        List<CompletionItem> items
    )
    {
        // Check if target is a known type
        var type = ToastType.FromName(targetName);
        if (toaster.ExtensionMembers.TryGetValue(type, out var members))
        {
            foreach (var (memberName, memberVal) in members)
            {
                if (memberVal is CommandValue cmdVal)
                {
                    items.Add(CreateCommandCompletionItem(cmdVal.Command, memberName));
                }
                else
                {
                    items.Add(
                        new CompletionItem
                        {
                            Label = memberName,
                            Kind = CompletionItemKind.Field,
                            Detail = $"{memberName}: {memberVal.Type.Name}",
                        }
                    );
                }
            }
        }

        // Check local symbols for type/class/module declared members
        var targetSymbol = localSymbols.FirstOrDefault(s => s.Name == targetName);
        if (targetSymbol != null)
        {
            if (targetSymbol.MemberCommands != null && targetSymbol.MemberCommands.Count > 0)
            {
                foreach (var (cmdName, cmd) in targetSymbol.MemberCommands)
                {
                    items.Add(CreateCommandCompletionItem(cmd, cmdName));
                }
            }
            else if (targetSymbol.Members != null)
            {
                foreach (var member in targetSymbol.Members)
                {
                    items.Add(
                        new CompletionItem
                        {
                            Label = member,
                            Kind = CompletionItemKind.Property,
                            Detail = $"{targetName}.{member}",
                            Documentation = new MarkupContent
                            {
                                Kind = MarkupKind.Markdown,
                                Value = $"Member property `{member}` of `{targetName}`",
                            },
                        }
                    );
                }
            }

            // Check if symbol type has extension members
            if (toaster.ExtensionMembers.TryGetValue(targetSymbol.Type, out var typeMembers))
            {
                foreach (var (memberName, memberVal) in typeMembers)
                {
                    if (memberVal is CommandValue cmdVal)
                    {
                        items.Add(CreateCommandCompletionItem(cmdVal.Command, memberName));
                    }
                    else
                    {
                        items.Add(
                            new CompletionItem
                            {
                                Label = memberName,
                                Kind = CompletionItemKind.Field,
                                Detail = $"{memberName}: {memberVal.Type.Name}",
                            }
                        );
                    }
                }
            }
        }

        // If targetName matches built-in type names directly (e.g. String, List, datetime)
        foreach (var (extType, extMembers) in toaster.ExtensionMembers)
        {
            if (extType.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var (memberName, memberVal) in extMembers)
                {
                    if (memberVal is CommandValue cmdVal)
                    {
                        items.Add(CreateCommandCompletionItem(cmdVal.Command, memberName));
                    }
                }
            }
        }
    }

    private static CompletionItem CreateCommandCompletionItem(
        Command cmd,
        string? overrideLabel = null
    )
    {
        var label = overrideLabel ?? cmd.Name;
        var kind =
            cmd.IsInfix && !char.IsLetter(cmd.Name[0])
                ? CompletionItemKind.Operator
                : CompletionItemKind.Function;

        var signature = cmd.GetSignature();
        if (overrideLabel != null)
        {
            var paramStrs = cmd.Parameters.Select(p => $"{p.Name}: {p.Type.Name}");
            signature = $"{overrideLabel}({string.Join(", ", paramStrs)}): {cmd.ReturnType.Name}";
        }

        var docBuilder = new System.Text.StringBuilder();
        if (!string.IsNullOrEmpty(cmd.Description))
        {
            docBuilder.AppendLine(cmd.Description);
            docBuilder.AppendLine();
        }

        docBuilder.AppendLine("```toast");
        docBuilder.AppendLine(signature);
        docBuilder.AppendLine("```");

        if (cmd.Parameters.Count > 0)
        {
            docBuilder.AppendLine();
            docBuilder.AppendLine("**Parameters:**");
            foreach (var p in cmd.Parameters)
            {
                docBuilder.AppendLine($"- `{p.Name}` ({p.Type.Name})");
            }
        }

        if (cmd.ReturnType != ToastType.Any)
        {
            docBuilder.AppendLine();
            docBuilder.AppendLine($"**Returns:** `{cmd.ReturnType.Name}`");
        }

        return new CompletionItem
        {
            Label = label,
            Kind = kind,
            Detail = signature,
            Documentation = new MarkupContent
            {
                Kind = MarkupKind.Markdown,
                Value = docBuilder.ToString().Trim(),
            },
        };
    }

    private static CompletionItem CreateTypeCompletionItem(string name, TypeValue typeValue)
    {
        var docBuilder = new System.Text.StringBuilder();
        docBuilder.AppendLine($"Type definition for `{name}`.");
        if (typeValue.DeclaredMembers.Count > 0)
        {
            docBuilder.AppendLine();
            docBuilder.AppendLine("**Declared Members:**");
            foreach (var m in typeValue.DeclaredMembers)
            {
                docBuilder.AppendLine($"- `{m}`");
            }
        }

        return new CompletionItem
        {
            Label = name,
            Kind = CompletionItemKind.Class,
            Detail = $"type {name}",
            Documentation = new MarkupContent
            {
                Kind = MarkupKind.Markdown,
                Value = docBuilder.ToString().Trim(),
            },
        };
    }

    private static bool IsImportContext(string text, Position position, out bool inQuotes)
    {
        inQuotes = false;
        var lines = text.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        if (position.Line >= lines.Length)
            return false;

        var line = lines[position.Line];
        var col = Math.Min(position.Character, line.Length);
        var beforeCursor = line[..col].TrimStart();

        if (beforeCursor.StartsWith("import "))
        {
            var rest = beforeCursor[7..].TrimStart();
            if (rest.StartsWith('"') || rest.StartsWith('\''))
            {
                inQuotes = true;
            }
            return true;
        }

        return false;
    }

    public override Task<CompletionItem> Handle(
        CompletionItem request,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(request);
    }
}
