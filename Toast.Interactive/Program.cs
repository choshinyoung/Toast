using System.Text;
using Toast;

Console.WriteLine("========================================");
Console.WriteLine("  Toast Interactive REPL v2.0");
Console.WriteLine("========================================");
Console.WriteLine("Type exit or quit to end the session.\n");

var toast = new Toaster(useBuiltIn: true);
toast.RegisterFunction("exit", (Context context) => Environment.Exit(0));

while (true)
{
    var buffer = new StringBuilder();
    string prompt = "toast> ";
    bool eofReached = false;

    while (true)
    {
        Console.Write(prompt);
        var line = Console.ReadLine();
        if (line == null)
        {
            eofReached = true;
            break;
        }

        buffer.Append(line);

        if (IsIncomplete(buffer.ToString(), toast))
        {
            buffer.Append('\n');
            prompt = "       ";
        }
        else
        {
            break;
        }
    }

    if (eofReached && buffer.Length == 0)
    {
        break;
    }

    var input = buffer.ToString();
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

static bool IsIncomplete(string input, Toaster toast)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        return false;
    }

    try
    {
        var tokens = Lexer.Tokenize(input);
        Parser.Parse(tokens, toast.GetInfixInfo, toast.IsPrefix);
        return false;
    }
    catch (Exception ex) when (ex.Message.Contains("end of file"))
    {
        return true;
    }
    catch
    {
        return false;
    }
}
