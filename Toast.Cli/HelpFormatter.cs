namespace Toast.Cli;

public static class HelpFormatter
{
    public static void PrintHelp(string? errorMessage = null)
    {
        if (errorMessage != null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {errorMessage}\n");
            Console.ResetColor();
        }

        Console.WriteLine(
            """
            Toast Language CLI Tool

            Usage:
              toast                         Start interactive REPL mode
              toast <script.toast>          Execute a Toast script file
              toast -e <code>               Evaluate inline Toast code string
              cat script.toast | toast      Execute script from stdin

            Options:
              -v, --version                 Display version information
              -h, --help                    Display this help message
              -e, --eval <code>             Evaluate inline script code
              -t, --tokens                  Print token stream for script or code
              -a, --ast                     Print AST structure for script or code

            Examples:
              toast
              toast main.toast
              toast -e "var x = 10; x * 2"
              toast -t -e "1 + 2"
              toast -a main.toast
            """
        );
    }
}
