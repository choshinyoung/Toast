using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Toast.LanguageServer.Handlers;

public class CompletionHandler : CompletionHandlerBase
{
    private static readonly List<CompletionItem> KeywordsCompletion = new()
    {
        new CompletionItem
        {
            Label = "var",
            Kind = CompletionItemKind.Keyword,
            Detail = "Variable declaration",
        },
        new CompletionItem
        {
            Label = "function",
            Kind = CompletionItemKind.Keyword,
            Detail = "Function declaration",
        },
        new CompletionItem
        {
            Label = "type",
            Kind = CompletionItemKind.Keyword,
            Detail = "Type definition",
        },
        new CompletionItem
        {
            Label = "if",
            Kind = CompletionItemKind.Keyword,
            Detail = "Conditional branch",
        },
        new CompletionItem
        {
            Label = "else",
            Kind = CompletionItemKind.Keyword,
            Detail = "Else branch",
        },
        new CompletionItem
        {
            Label = "while",
            Kind = CompletionItemKind.Keyword,
            Detail = "While loop",
        },
        new CompletionItem
        {
            Label = "for",
            Kind = CompletionItemKind.Keyword,
            Detail = "For loop",
        },
        new CompletionItem
        {
            Label = "try",
            Kind = CompletionItemKind.Keyword,
            Detail = "Try block",
        },
        new CompletionItem
        {
            Label = "catch",
            Kind = CompletionItemKind.Keyword,
            Detail = "Catch block",
        },
        new CompletionItem
        {
            Label = "throw",
            Kind = CompletionItemKind.Keyword,
            Detail = "Throw exception",
        },
        new CompletionItem
        {
            Label = "is",
            Kind = CompletionItemKind.Operator,
            Detail = "Type compatibility check",
        },
        new CompletionItem
        {
            Label = "in",
            Kind = CompletionItemKind.Operator,
            Detail = "Range/collection membership check",
        },
        new CompletionItem
        {
            Label = "to",
            Kind = CompletionItemKind.Operator,
            Detail = "Range generator",
        },
        new CompletionItem
        {
            Label = "typeof",
            Kind = CompletionItemKind.Function,
            Detail = "Get type of expression",
        },
        new CompletionItem
        {
            Label = "print",
            Kind = CompletionItemKind.Function,
            Detail = "Print to console",
        },
        new CompletionItem
        {
            Label = "floorDiv",
            Kind = CompletionItemKind.Function,
            Detail = "Floor division",
        },
        new CompletionItem
        {
            Label = "sqrt",
            Kind = CompletionItemKind.Function,
            Detail = "Square root",
        },
        new CompletionItem
        {
            Label = "Error",
            Kind = CompletionItemKind.Class,
            Detail = "Error object constructor",
        },
        new CompletionItem
        {
            Label = "number",
            Kind = CompletionItemKind.Class,
            Detail = "Built-in number type",
        },
        new CompletionItem
        {
            Label = "string",
            Kind = CompletionItemKind.Class,
            Detail = "Built-in string type",
        },
        new CompletionItem
        {
            Label = "boolean",
            Kind = CompletionItemKind.Class,
            Detail = "Built-in boolean type",
        },
        new CompletionItem
        {
            Label = "object",
            Kind = CompletionItemKind.Class,
            Detail = "Built-in object type",
        },
    };

    protected override CompletionRegistrationOptions CreateRegistrationOptions(
        CompletionCapability capability,
        ClientCapabilities clientCapabilities
    )
    {
        return new CompletionRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("toast"),
            ResolveProvider = false,
        };
    }

    public override Task<CompletionList> Handle(
        CompletionParams request,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(new CompletionList(KeywordsCompletion));
    }

    public override Task<CompletionItem> Handle(
        CompletionItem request,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(request);
    }
}
