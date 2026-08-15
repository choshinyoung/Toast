using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

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
        var uri = request.TextDocument.Uri;
        var text = DocumentManager.Instance.GetDocument(uri);
        if (text != null)
        {
            ValidateDocument(uri, text);
        }
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
            Save = new OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities.SaveOptions
            {
                IncludeText = true,
            },
        };
    }

    private void ValidateDocument(DocumentUri uri, string text)
    {
        var toast = DocumentManager.Instance.GetToasterForDocument(uri);
        var diagnostics = ScopeAnalyzer.ValidateDocumentStatically(text, toast);

        _router.TextDocument.PublishDiagnostics(
            new PublishDiagnosticsParams { Uri = uri, Diagnostics = diagnostics }
        );
    }
}
