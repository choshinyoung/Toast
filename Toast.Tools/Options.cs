namespace Toast.Tools;

public enum ExecutionMode
{
    Repl,
    RunFile,
    EvalCode,
    ReadStdin,
    ShowHelp,
    ShowVersion,
    LanguageServer,
    InstallModule,
    ListModules,
    UninstallModule,
}

public sealed class Options
{
    public ExecutionMode Mode { get; init; } = ExecutionMode.Repl;
    public string? ScriptPath { get; init; }
    public string? EvalCode { get; init; }
    public string? InstallSource { get; init; }
    public string? TargetModuleName { get; init; }
    public string? UninstallModuleName { get; init; }
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

        var first = args[0].ToLowerInvariant();

        if (first == "module")
        {
            if (args.Length < 2)
            {
                return new Options
                {
                    Mode = ExecutionMode.ShowHelp,
                    ErrorMessage = "Usage: toast module (install|list|uninstall) [arguments...]",
                };
            }

            var subCommand = args[1].ToLowerInvariant();
            if (subCommand == "install" || subCommand == "add")
            {
                if (args.Length < 3)
                {
                    return new Options
                    {
                        Mode = ExecutionMode.ShowHelp,
                        ErrorMessage =
                            "Usage: toast module install <url-or-file-path> [--name <moduleName>]",
                    };
                }

                string source = args[2];
                string? name = null;
                for (int i = 3; i < args.Length; i++)
                {
                    if (args[i] == "--name" && i + 1 < args.Length)
                    {
                        name = args[++i];
                    }
                }

                return new Options
                {
                    Mode = ExecutionMode.InstallModule,
                    InstallSource = source,
                    TargetModuleName = name,
                };
            }

            if (subCommand == "list")
            {
                return new Options { Mode = ExecutionMode.ListModules };
            }

            if (subCommand == "uninstall" || subCommand == "remove")
            {
                if (args.Length < 3)
                {
                    return new Options
                    {
                        Mode = ExecutionMode.ShowHelp,
                        ErrorMessage = "Usage: toast module uninstall <moduleName>",
                    };
                }
                return new Options
                {
                    Mode = ExecutionMode.UninstallModule,
                    UninstallModuleName = args[2],
                };
            }

            return new Options
            {
                Mode = ExecutionMode.ShowHelp,
                ErrorMessage =
                    $"Unknown module subcommand '{args[1]}'. Use 'toast module (install|list|uninstall)'.",
            };
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
                case "--lsp":
                case "lsp":
                    return new Options { Mode = ExecutionMode.LanguageServer };
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
