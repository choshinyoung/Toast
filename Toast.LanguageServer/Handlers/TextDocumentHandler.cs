using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Toast.LanguageServer.Handlers;

public class TextDocumentHandler(ILanguageServerFacade router) : TextDocumentSyncHandlerBase
{
    private readonly ILanguageServerFacade _router = router;

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        return new TextDocumentAttributes(uri, "toast");
    }

    public override Task<Unit> Handle(
        DidOpenTextDocumentParams request,
        CancellationToken cancellationToken
    )
    {
        var uri = request.TextDocument.Uri;
        var text = request.TextDocument.Text;
        DocumentManager.Instance.UpdateDocument(uri, text);
        ValidateDocument(uri, text);
        return Unit.Task;
    }

    public override Task<Unit> Handle(
        DidChangeTextDocumentParams request,
        CancellationToken cancellationToken
    )
    {
        var uri = request.TextDocument.Uri;
        var text = request.ContentChanges.FirstOrDefault()?.Text;
        if (text != null)
        {
            DocumentManager.Instance.UpdateDocument(uri, text);
            ValidateDocument(uri, text);
        }
        return Unit.Task;
    }

    public override Task<Unit> Handle(
        DidSaveTextDocumentParams request,
        CancellationToken cancellationToken
    )
    {
        return Unit.Task;
    }

    public override Task<Unit> Handle(
        DidCloseTextDocumentParams request,
        CancellationToken cancellationToken
    )
    {
        DocumentManager.Instance.RemoveDocument(request.TextDocument.Uri);
        return Unit.Task;
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities
    )
    {
        return new TextDocumentSyncRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("toast"),
            Change = OmniSharp
                .Extensions
                .LanguageServer
                .Protocol
                .Server
                .Capabilities
                .TextDocumentSyncKind
                .Full,
        };
    }

    private void ValidateDocument(DocumentUri uri, string text)
    {
        var diagnostics = new List<Diagnostic>();
        var toast = DocumentManager.Instance.GetToasterForDocument(uri);

        try
        {
            var tokens = Lexer.Tokenize(text);
            Parser.Parse(tokens, toast.GetInfixInfo, toast.IsPrefix);
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
                    Source = "Toast Language Server",
                }
            );
        }
        catch (Exception ex)
        {
            diagnostics.Add(
                new Diagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Range = new LspRange(new Position(0, 0), new Position(0, 1)),
                    Message = ex.Message,
                    Source = "Toast Language Server",
                }
            );
        }

        _router.TextDocument.PublishDiagnostics(
            new PublishDiagnosticsParams { Uri = uri, Diagnostics = diagnostics }
        );
    }
}
