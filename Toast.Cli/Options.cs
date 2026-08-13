namespace Toast.Cli;

public enum ExecutionMode
{
    Repl,
    RunFile,
    EvalCode,
    ReadStdin,
    ShowHelp,
    ShowVersion,
}

public sealed class Options
{
    public ExecutionMode Mode { get; init; } = ExecutionMode.Repl;
    public string? ScriptPath { get; init; }
    public string? EvalCode { get; init; }
    public bool PrintTokens { get; init; }
    public bool PrintAst { get; init; }
    public string? ErrorMessage { get; init; }

    public static Options Parse(string[] args)
    {
        if (args.Length == 0)
        {
            if (Console.IsInputRedirected)
            {
                return new Options { Mode = ExecutionMode.ReadStdin };
            }
            return new Options { Mode = ExecutionMode.Repl };
        }

        bool printTokens = false;
        bool printAst = false;
        string? evalCode = null;
        string? scriptPath = null;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "-v":
                case "--version":
                    return new Options { Mode = ExecutionMode.ShowVersion };
                case "-h":
                case "--help":
                    return new Options { Mode = ExecutionMode.ShowHelp };
                case "-t":
                case "--tokens":
                    printTokens = true;
                    break;
                case "-a":
                case "--ast":
                    printAst = true;
                    break;
                case "-e":
                case "--eval":
                    if (i + 1 < args.Length)
                    {
                        evalCode = args[++i];
                    }
                    else
                    {
                        return new Options
                        {
                            Mode = ExecutionMode.ShowHelp,
                            ErrorMessage = "Option '-e/--eval' requires a code string argument.",
                        };
                    }
                    break;
                default:
                    if (arg.StartsWith('-'))
                    {
                        return new Options
                        {
                            Mode = ExecutionMode.ShowHelp,
                            ErrorMessage = $"Unknown option '{arg}'. Use --help for usage.",
                        };
                    }
                    scriptPath ??= arg;
                    break;
            }
        }

        if (evalCode != null)
        {
            return new Options
            {
                Mode = ExecutionMode.EvalCode,
                EvalCode = evalCode,
                PrintTokens = printTokens,
                PrintAst = printAst,
            };
        }

        if (scriptPath != null)
        {
            return new Options
            {
                Mode = ExecutionMode.RunFile,
                ScriptPath = scriptPath,
                PrintTokens = printTokens,
                PrintAst = printAst,
            };
        }

        return new Options { Mode = ExecutionMode.ShowHelp };
    }
}
