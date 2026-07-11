using Toast;

Console.WriteLine("========================================");
Console.WriteLine("  Toast DSL Interactive REPL v2.0");
Console.WriteLine("========================================");
Console.WriteLine("Type exit or quit to end the session.\n");

var toast = new Toaster(useBuiltIn: true);
toast.RegisterFunction("exit", (Context context) => Environment.Exit(0));

while (true)
{
    Console.Write("toast> ");

    var input = Console.ReadLine();
    if (input == null)
    {
        break;
    }

    var trimmed = input.Trim();
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
            var toastType = Executor.GetToastType(result);
            if (toast.Converters.TryGetValue((toastType, ToastType.String), out var converter))
            {
                Console.WriteLine(converter.ConvertFunc(toast.GlobalContext, result));
            }
            else
            {
                Console.WriteLine(result);
            }
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
