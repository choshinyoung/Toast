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
        DocumentManager.Instance.UpdateDocument(uri, "import \"system\"\nprint(1)");

        var handler = new CompletionHandler();
        var result = await handler.Handle(
            new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position(1, 5),
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
    public async Task TestCompletionDoesNotContainSystemCommandsWithoutImport()
    {
        var uri = DocumentUri.From("file:///unimported.toast");
        DocumentManager.Instance.UpdateDocument(uri, "if (true) { print(\"hello\") }");

        var handler = new CompletionHandler();
        var result = await handler.Handle(
            new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position(0, 10),
            },
            CancellationToken.None
        );

        var items = result.Items.ToList();
        // Without `import "system"`, neither 'print' nor 'if' should be in completion list
        Assert.DoesNotContain(items, i => i.Label == "print");
        Assert.DoesNotContain(items, i => i.Label == "if");
    }

    [Fact]
    public async Task TestCompletionContainsLocalVariablesAndFunctions()
    {
        var uri = DocumentUri.From("file:///script.toast");
        var script = """
            import "system"
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
                Position = new Position(5, 0),
            },
            CancellationToken.None
        );

        var items = result.Items.ToList();
        Assert.Contains(items, i => i.Label == "myGreeting");
        Assert.Contains(items, i => i.Label == "calculateArea");
    }

    [Fact]
    public async Task TestCompletionTypeMemberAccess()
    {
        var uri = DocumentUri.From("file:///member_test.toast");
        var script = """
            import "system"
            class Person(name, age) => {}
            var p = Person("Bob", 30)
            p.
            """;
        DocumentManager.Instance.UpdateDocument(uri, script);

        var handler = new CompletionHandler();
        var result = await handler.Handle(
            new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position(3, 2), // right after 'p.'
            },
            CancellationToken.None
        );

        var items = result.Items.ToList();
        Assert.NotEmpty(items);
        Assert.Contains(items, i => i.Label == "name");
        Assert.Contains(items, i => i.Label == "age");
    }

    [Fact]
    public async Task TestCompletionStringExtensionMethods()
    {
        var uri = DocumentUri.From("file:///string_test.toast");
        var script = """
            import "system"
            var s = "hello"
            s.
            """;
        DocumentManager.Instance.UpdateDocument(uri, script);

        var handler = new CompletionHandler();
        var result = await handler.Handle(
            new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position(2, 2), // right after 's.'
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
        DocumentManager.Instance.UpdateDocument(uri, "import \"system\"\nprint(\"hello\")");

        var handler = new HoverHandler();
        var hover = await handler.Handle(
            new HoverParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position(1, 2), // over 'print'
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
            import "system"
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

    [Fact]
    public void TestStaticDiagnosticDetectsUnimportedSystemCommands()
    {
        var toaster = new Toaster();
        var unimportedCode = """
            if (true) {
                print("hello")
            }
            """;

        var diags = ScopeAnalyzer.ValidateDocumentStatically(unimportedCode, toaster);
        Assert.NotEmpty(diags);
        Assert.Contains(diags, d => d.Message.Contains("if"));
        Assert.Contains(diags, d => d.Message.Contains("true"));
        Assert.Contains(diags, d => d.Message.Contains("print"));
    }

    [Fact]
    public void TestStaticDiagnosticPassesWithImport()
    {
        var toaster = new Toaster();
        var importedCode = """
            import "system"
            if (true) {
                print("hello")
            }
            """;

        var diags = ScopeAnalyzer.ValidateDocumentStatically(importedCode, toaster);
        Assert.Empty(diags);
    }

    [Fact]
    public async Task TestSemanticTokensStringInterpolation()
    {
        var uri = DocumentUri.From("file:///interpolation_semantic.toast");
        var script = "var name = \"World\"\nprint(\"Hello {name}, count is {1 + 2}!\")";
        DocumentManager.Instance.UpdateDocument(uri, script);

        var handler = new SemanticTokensHandler();
        var result = await handler.Handle(
            new SemanticTokensParams { TextDocument = new TextDocumentIdentifier { Uri = uri } },
            CancellationToken.None
        );

        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public void TestMemberGetterOperatorNoDiagnostics()
    {
        var toaster = new Toaster();
        var code = """
            import "system"
            class Point(x, y) => {
                function magnitude() => 0
            }
            var arr = 1 to 10 |> map { Point(1, 2) }
            arr |> sortAs ..magnitude
            """;

        var diags = ScopeAnalyzer.ValidateDocumentStatically(code, toaster);
        Assert.Empty(diags);
    }
}
