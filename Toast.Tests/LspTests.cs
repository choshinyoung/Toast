using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Toast.LanguageServer;
using Toast.LanguageServer.Handlers;

namespace Toast.Tests;

public class LspTests
{
    [Fact]
    public async Task TestCompletionContainsDynamicBuiltInsWithSignatures()
    {
        var uri = DocumentUri.From("file:///test.toast");
        DocumentManager.Instance.UpdateDocument(uri, "print(1)");

        var handler = new CompletionHandler();
        var result = await handler.Handle(
            new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position(0, 5),
            },
            CancellationToken.None
        );

        var items = result.Items.ToList();
        Assert.NotEmpty(items);

        // Verify 'print' item exists and has signature & description
        var printItem = items.FirstOrDefault(i => i.Label == "print");
        Assert.NotNull(printItem);
        Assert.Contains("print", printItem.Detail);
        Assert.NotNull(printItem.Documentation);
        Assert.Contains("Prints a value", printItem.Documentation.MarkupContent?.Value);

        // Verify 'random' item exists and has number return type
        var randomItem = items.FirstOrDefault(i => i.Label == "random");
        Assert.NotNull(randomItem);
        Assert.Contains("number", randomItem.Detail);
    }

    [Fact]
    public async Task TestCompletionContainsLocalVariablesAndFunctions()
    {
        var uri = DocumentUri.From("file:///script.toast");
        var script = """
            var myGreeting = "Hello"
            function calculateArea(w, h) => {
                w * h
            }

            """;
        DocumentManager.Instance.UpdateDocument(uri, script);

        var handler = new CompletionHandler();
        var result = await handler.Handle(
            new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position(4, 0),
            },
            CancellationToken.None
        );

        var items = result.Items.ToList();

        // Check myGreeting
        var varItem = items.FirstOrDefault(i => i.Label == "myGreeting");
        Assert.NotNull(varItem);

        // Check calculateArea
        var funcItem = items.FirstOrDefault(i => i.Label == "calculateArea");
        Assert.NotNull(funcItem);
        Assert.Contains("w", funcItem.Detail);
        Assert.Contains("h", funcItem.Detail);
    }

    [Fact]
    public async Task TestDotAccessCompletion()
    {
        var uri = DocumentUri.From("file:///dot.toast");
        var script = """
            var str = "hello"
            string.
            """;
        DocumentManager.Instance.UpdateDocument(uri, script);

        var handler = new CompletionHandler();
        var result = await handler.Handle(
            new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position(1, 7), // right after 'string.'
            },
            CancellationToken.None
        );

        var items = result.Items.ToList();
        Assert.NotEmpty(items);
        Assert.Contains(items, i => i.Label == "length");
        Assert.Contains(items, i => i.Label == "substring");
        Assert.Contains(items, i => i.Label == "split");
    }

    [Fact]
    public async Task TestHoverHandlerReturnsCommandDocs()
    {
        var uri = DocumentUri.From("file:///hover.toast");
        DocumentManager.Instance.UpdateDocument(uri, "print(\"hello\")");

        var handler = new HoverHandler();
        var hover = await handler.Handle(
            new HoverParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position(0, 2), // over 'print'
            },
            CancellationToken.None
        );

        Assert.NotNull(hover);
        var markup = hover.Contents.MarkupContent;
        Assert.NotNull(markup);
        Assert.Contains("print", markup.Value);
        Assert.Contains("Prints a value", markup.Value);
    }

    [Fact]
    public async Task TestSemanticTokensHandlerReturnsDynamicTokens()
    {
        var uri = DocumentUri.From("file:///semantic.toast");
        var script = """
            import "math"
            var root = sqrt(256)
            class Point(x, y) => {}
            """;
        DocumentManager.Instance.UpdateDocument(uri, script);

        var handler = new SemanticTokensHandler();
        var result = await handler.Handle(
            new SemanticTokensParams { TextDocument = new TextDocumentIdentifier { Uri = uri } },
            CancellationToken.None
        );

        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
    }
}
