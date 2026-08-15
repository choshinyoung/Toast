using System.Reflection;
using Toast.Tools;

string version = Assembly
    .GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
    .InformationalVersion;

var options = Options.Parse(args);

return options.Mode switch
{
    ExecutionMode.ShowVersion => PrintVersion(version),
    ExecutionMode.ShowHelp => PrintHelp(options.ErrorMessage),
    ExecutionMode.Repl => RunInteractive(version),
    ExecutionMode.LanguageServer => await RunLanguageServerAsync(),
    ExecutionMode.ReadStdin => ScriptRunner.RunStdin(),
    ExecutionMode.InstallModule => await ModuleRunner.InstallAsync(
        options.InstallSource!,
        options.TargetModuleName
    ),
    ExecutionMode.ListModules => ModuleRunner.List(),
    ExecutionMode.UninstallModule => ModuleRunner.Uninstall(options.UninstallModuleName!),
    ExecutionMode.EvalCode => ScriptRunner.ProcessCode(
        options.EvalCode!,
        "<eval>",
        options.PrintTokens,
        options.PrintAst
    ),
    ExecutionMode.RunFile => ScriptRunner.RunFile(
        options.ScriptPath!,
        options.PrintTokens,
        options.PrintAst
    ),
    _ => PrintHelp(),
};

static int PrintVersion(string version)
{
    Console.WriteLine($"Toast v{version}");
    return 0;
}

static int PrintHelp(string? errorMessage = null)
{
    HelpFormatter.PrintHelp(errorMessage);
    return errorMessage == null ? 0 : 1;
}

static int RunInteractive(string version)
{
    InteractiveRunner.Run(version);
    return 0;
}

static async Task<int> RunLanguageServerAsync()
{
    return await Toast.LanguageServer.Program.RunServerAsync();
}
