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
        var toaster = new Toaster([]);

        // 1. Load 'import' module first to be able to import others
        toaster.Load<SystemModules.ImportModule>();

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
        toaster.Execute("import \"converter\"");
        Assert.True(toaster.Converters.ContainsKey((ToastType.Number, ToastType.String)));
    }

    [Fact]
    public void TestImportSystemModuleIngestsAllModulesDirectly()
    {
        var toaster = new Toaster([
            new SystemModules.ImportModule(),
            new SystemModules.SystemModule(),
        ]);
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
        var toaster = new Toaster([
            new SystemModules.ImportModule(),
            new SystemModules.SystemModule(),
        ]);
        var result = toaster.Execute(
            """
            import "math"
            var direct = math.sqrt(64)
            var floor = math.floorDiv 15 2
            direct + floor
            """
        );

        Assert.True(result is NumberValue);
        Assert.Equal(15, ((NumberValue)result).Value);
    }

    [Fact]
    public void TestImportDateTimeModule()
    {
        var toaster = new Toaster([
            new SystemModules.ImportModule(),
            new SystemModules.SystemModule(),
        ]);
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
        var toaster = new Toaster([
            new SystemModules.ImportModule(),
            new SystemModules.SystemModule(),
        ]);

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

            var toaster = new Toaster([
                new SystemModules.ImportModule(),
                new SystemModules.SystemModule(),
            ]);
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
        Assert.Contains(items, i => i.Label == "math");
    }

    [Fact]
    public void TestImportInsideBlockThrowsSyntaxError()
    {
        var toaster = new Toaster([
            new SystemModules.ImportModule(),
            new SystemModules.SystemModule(),
        ]);
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
        var toaster = new Toaster([
            new SystemModules.ImportModule(),
            new SystemModules.SystemModule(),
        ]);
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

    [Fact]
    public void TestTopLevelImportInReplAndScriptSucceeds()
    {
        var toaster = new Toaster([
            new SystemModules.ImportModule(),
            new SystemModules.SystemModule(),
        ]);
        var result = toaster.Execute("""import "math" """);
        Assert.True(result is NullValue);

        var sqrtResult = toaster.Execute("math.sqrt(25)");
        Assert.True(sqrtResult is NumberValue);
        Assert.Equal(5, ((NumberValue)sqrtResult).Value);
    }

    [Fact]
    public void TestBareSandboxWithoutImportModule()
    {
        var bareToaster = new Toaster([]);
        var ex = Assert.Throws<ToastException>(() =>
        {
            bareToaster.Execute("import \"math\"");
        });
        Assert.Equal("RuntimeError", ex.Error.ErrorType);
        Assert.Contains("import", ex.Error.Message);
    }

    [Fact]
    public void TestLoadGenericAndTypeReflection()
    {
        var toaster = new Toaster([]);
        toaster.Load<SystemModules.ObjectModule>();
        toaster.Load<SystemModules.MathModule>();
        var res = toaster.Execute("math.sqrt(100)");
        Assert.Equal(10, ((NumberValue)res).Value);

        toaster.Load(typeof(SystemModules.DateTimeModule));
        var res2 = toaster.Execute("datetime.now().year");
        Assert.Equal(DateTime.Now.Year, ((NumberValue)res2).Value);
    }

    [Fact]
    public void TestLoadStringFromManager()
    {
        var toaster = new Toaster([]);
        toaster.Load<SystemModules.ObjectModule>();
        toaster.Load("math");
        var res = toaster.Execute("math.sqrt(49)");
        Assert.Equal(7, ((NumberValue)res).Value);
    }

    [Fact]
    public void TestDeclarativeAttributesModule()
    {
        var toaster = new Toaster([]);
        toaster.Load<SystemModules.ObjectModule>(); // for dot operator
        toaster.Load<SystemModules.DefaultModule>(); // for arithmetic
        toaster.Load<SampleCustomModule>();

        // 1. ToastCommand function (no Context needed - pure computation)
        var res1 = toaster.Execute("greet(\"World\")");
        Assert.Equal("Hello, World!", ((StringValue)res1).Value);

        // 2. ToastCommand operator
        var res2 = toaster.Execute("5 +++ 3");
        Assert.Equal(18, ((NumberValue)res2).Value);

        // 3. ToastObject namespace
        var res3 = toaster.Execute("calc.double(21)");
        Assert.Equal(42, ((NumberValue)res3).Value);

        // 4. Context DI (Context injected automatically into method parameter)
        var res4 = toaster.Execute("evalSelf(\"10 + 20\")");
        Assert.Equal(30, ((NumberValue)res4).Value);

        // 5. ToastMember (registered ONLY on string type, not as global command)
        var res5 = toaster.Execute("\"hello\".shout()");
        Assert.Equal("hello!!!", ((StringValue)res5).Value);

        // Global call shout("hello") must fail because it's only a member
        Assert.Throws<ToastException>(() =>
        {
            toaster.Execute("shout(\"hello\")");
        });

        // 6. ToastConverter with automatic parameter/return type inference
        Assert.True(toaster.Converters.ContainsKey((ToastType.String, ToastType.Number)));
    }
}

[ToastModule("sample", "Sample module for testing.")]
public class SampleCustomModule : IToastModule
{
    [ToastType("person")]
    public static class PersonType { }

    [ToastType("string")]
    public static class StringExtensions
    {
        [ToastCommand("shout", "Shouts a string with exclamation marks.")]
        public static StringValue Shout(StringValue s)
        {
            return new StringValue(s.Value + "!!!");
        }
    }

    [ToastCommand("greet", "Greets a person.")]
    public static StringValue Greet(StringValue name)
    {
        return new StringValue($"Hello, {name.Value}!");
    }

    [ToastCommand("+++", "Custom triple plus operator.", Precedence = 11)]
    public static NumberValue TripleAdd(NumberValue a, NumberValue b)
    {
        return new NumberValue(a.Value + b.Value + 10);
    }

    [ToastCommand("evalSelf", "Evals code with injected Context.")]
    public static ToastValue EvalSelf(Context ctx, StringValue expr)
    {
        return ctx.Toaster.Execute(expr.Value, ctx);
    }

    [ToastConverter]
    public static NumberValue StringToNumberAuto(StringValue s)
    {
        return new NumberValue(double.Parse(s.Value));
    }

    [ToastObject("calc")]
    public static class CalcNamespace
    {
        [ToastCommand("double", "Doubles a number.")]
        public static NumberValue Double(NumberValue x)
        {
            return new NumberValue(x.Value * 2);
        }
    }
}
