using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Toast.LanguageServer;
using Toast.LanguageServer.Handlers;

namespace Toast.Tests;

public class ModuleTests
{
    [Fact]
    public void TestImportClassifiedModulesIndividually()
    {
        // Sandbox Toaster with no modules
        var toaster = new Toaster(useSystemModules: false);

        // 1. Import 'import' module first to be able to import others (or register core import)
        SystemModules.ImportModule.Register(toaster);

        // 2. Import object & default
        toaster.Execute("import \"object\"");
        toaster.Execute("import \"default\"");
        var res = toaster.Execute(
            """
            var x = 10 + 5
            x
            """
        );
        Assert.Equal(15, ((NumberValue)res).Value);

        // 3. Import flow
        toaster.Execute("import \"flow\"");
        var resultFlow = toaster.Execute("if (true, () => 99)");
        Assert.Equal(99, ((NumberValue)resultFlow).Value);

        // 4. Import converter
        var resultConv = toaster.Execute(
            """
            import "converter"
            string 456
            """
        );
        Assert.Equal("456", ((StringValue)resultConv).Value);
    }

    [Fact]
    public void TestImportSystemModuleIngestsAllModulesDirectly()
    {
        var toaster = new Toaster(useSystemModules: true);
        var result = toaster.Execute(
            """
            import "system"
            var parts = "hello world".split(" ")
            var doubled = map([10, 20], (x) => x * 2)
            doubled # 0
            """
        );

        Assert.True(result is NumberValue);
        Assert.Equal(20, ((NumberValue)result).Value);
    }

    [Fact]
    public void TestImportMathAllowsDirectCallAndConstants()
    {
        var toaster = new Toaster(useSystemModules: true);
        var result = toaster.Execute(
            """
            import "math"
            var direct = sqrt(64)
            var floor = floorDiv 15 2
            direct + floor
            """
        );

        Assert.True(result is NumberValue);
        Assert.Equal(15, ((NumberValue)result).Value);
    }

    [Fact]
    public void TestImportDateTimeModule()
    {
        var toaster = new Toaster(useSystemModules: true);
        var result = toaster.Execute(
            """
            import "datetime"
            var n = datetime.now()
            n.year
            """
        );

        Assert.True(result is NumberValue);
        Assert.Equal(DateTime.Now.Year, ((NumberValue)result).Value);
    }

    [Fact]
    public void TestSystemModulesCannotBeUninstalled()
    {
        var manager = ModuleManager.Instance;

        Assert.Throws<InvalidOperationException>(() =>
        {
            manager.UninstallModule("system");
        });

        Assert.Throws<InvalidOperationException>(() =>
        {
            manager.UninstallModule("default");
        });

        Assert.Throws<InvalidOperationException>(() =>
        {
            manager.UninstallModule("object");
        });

        Assert.Throws<InvalidOperationException>(() =>
        {
            manager.UninstallModule("converter");
        });
    }

    [Fact]
    public void TestDisallowedPathImportsThrowSecurityError()
    {
        var toaster = new Toaster(useSystemModules: true);

        Assert.Throws<ToastException>(() =>
        {
            toaster.Execute("import \"./dangerous.dll\"");
        });

        Assert.Throws<ToastException>(() =>
        {
            toaster.Execute("import \"../secret.toast\"");
        });
    }

    [Fact]
    public void TestInstallAndImportLocalScriptModuleAllowsDirectCall()
    {
        var tempDir = Path.Combine(
            Path.GetTempPath(),
            "toast_test_modules_" + Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(tempDir);
        ModuleManager.Instance.GlobalModulesDirectory = tempDir;

        try
        {
            var helperScript = Path.Combine(tempDir, "temp_source.toast");
            File.WriteAllText(
                helperScript,
                """
                function addThree(x) => x + 3
                var magicNumber = 42
                """
            );

            ModuleManager.Instance.InstallLocalFile(helperScript, "myhelper");

            var toaster = new Toaster(useSystemModules: true);
            var result = toaster.Execute(
                """
                import "myhelper"
                var res1 = addThree(7)
                res1 + magicNumber
                """
            );

            Assert.True(result is NumberValue);
            Assert.Equal(52, ((NumberValue)result).Value);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task TestLspCompletionsForImportKeyword()
    {
        var uri = DocumentUri.From("file:///import_completion_test.toast");
        var script = "import \"";
        DocumentManager.Instance.UpdateDocument(uri, script);

        var handler = new CompletionHandler();
        var result = await handler.Handle(
            new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position(0, 8), // right after 'import "'
            },
            CancellationToken.None
        );

        var items = result.Items.ToList();
        Assert.NotEmpty(items);
        Assert.Contains(items, i => i.Label == "system");
        Assert.Contains(items, i => i.Label == "default");
        Assert.Contains(items, i => i.Label == "object");
        Assert.Contains(items, i => i.Label == "flow");
        Assert.Contains(items, i => i.Label == "converter");
        Assert.Contains(items, i => i.Label == "math");
        Assert.Contains(items, i => i.Label == "datetime");
    }

    [Fact]
    public async Task TestLspCompletionsDirectSymbolAfterImport()
    {
        var uri = DocumentUri.From("file:///direct_symbol_test.toast");
        var script = """
            import "math"

            """;
        DocumentManager.Instance.UpdateDocument(uri, script);

        var handler = new CompletionHandler();
        var result = await handler.Handle(
            new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position(1, 0),
            },
            CancellationToken.None
        );

        var items = result.Items.ToList();
        Assert.NotEmpty(items);
        Assert.Contains(items, i => i.Label == "sqrt");
        Assert.Contains(items, i => i.Label == "floorDiv");
    }

    [Fact]
    public void TestImportInsideBlockThrowsSyntaxError()
    {
        var toaster = new Toaster(useSystemModules: true);
        var ex = Assert.Throws<ToastException>(() =>
        {
            toaster.Execute(
                """
                if (true) {
                    import "math"
                }
                """
            );
        });

        Assert.Equal("SyntaxError", ex.Error.ErrorType);
        Assert.Contains("depth 0", ex.Error.Message);
    }

    [Fact]
    public void TestImportInsideFunctionThrowsSyntaxError()
    {
        var toaster = new Toaster(useSystemModules: true);
        var ex = Assert.Throws<ToastException>(() =>
        {
            toaster.Execute(
                """
                var f = () => {
                    import "math"
                }
                """
            );
        });

        Assert.Equal("SyntaxError", ex.Error.ErrorType);
        Assert.Contains("depth 0", ex.Error.Message);
    }
}
