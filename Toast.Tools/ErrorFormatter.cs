namespace Toast.Tools;

public static class ErrorFormatter
{
    public static void PrintError(ErrorValue error, string input, string sourceName)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"[{error.ErrorType}] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(error.Message);

        var loc = error.Location;
        if (loc != null && loc.Line > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  at {sourceName} (Line {loc.Line}, Column {loc.Column})");

            var lines = input.Split('\n');
            if (loc.Line <= lines.Length)
            {
                string srcLine = lines[loc.Line - 1].Replace('\r', ' ');
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"  {loc.Line} | ");
                Console.ResetColor();
                Console.WriteLine(srcLine);

                int col = Math.Max(1, loc.Column);
                int indent = loc.Line.ToString().Length + 5 + (col - 1);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(new string(' ', indent) + "^");
            }
        }

        Console.ResetColor();
        Console.WriteLine();
    }
}
