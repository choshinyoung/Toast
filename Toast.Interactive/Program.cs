using Toast;

Console.WriteLine("========================================");
Console.WriteLine("  Toast DSL Interactive REPL v2.0");
Console.WriteLine("========================================");
Console.WriteLine("Type exit or quit to end the session.\n");

var toast = new Toaster(useBuiltIn: true);

while (true)
{
    Console.Write("toast> ");

    var input = Console.ReadLine();
    if (input == null)
    {
        break;
    }

    var trimmed = input.Trim();
    if (trimmed == "exit")
    {
        break;
    }
    if (string.IsNullOrWhiteSpace(trimmed))
    {
        continue;
    }

    try
    {
        var result = toast.Execute(input);
        if (result == null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("(null)");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(result);
        }

        Console.ResetColor();
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");

        Console.ResetColor();
        Console.WriteLine();
    }
}
