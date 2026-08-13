using System.Reflection;
using Toast.Cli;

string version = (
    Assembly
        .GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion
    ?? "2.0.0-beta"
).Split('+')[0];

var options = Options.Parse(args);

return options.Mode switch
{
    ExecutionMode.ShowVersion => PrintVersion(version),
    ExecutionMode.ShowHelp => PrintHelp(options.ErrorMessage),
    ExecutionMode.Repl => RunInteractive(version),
    ExecutionMode.ReadStdin => ScriptRunner.RunStdin(),
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
