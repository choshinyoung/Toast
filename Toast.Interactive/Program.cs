using System.Text;
using Toast;

Console.WriteLine("===============================");
Console.WriteLine("  Toast Interactive REPL v2.0");
Console.WriteLine("===============================");
Console.WriteLine("Type exit to end the session.\n");

var toast = new Toaster(useBuiltIn: true);
toast.RegisterFunction("exit", (Context context) => Environment.Exit(0));

while (true)
{
    var buffer = new StringBuilder();
    string prompt = "> ";
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
            prompt = "  ";
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
        if (result is NullValue)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("(null)");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            var toastType = result.Type;
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
    catch (ToastException ex)
    {
        PrintError(ex.Error, input);
    }
    catch (Exception ex)
    {
        PrintError(new RuntimeError(ex.Message), input);
    }
}

static void PrintError(ErrorValue error, string input)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write($"[{error.ErrorType}] ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(error.Message);

    var loc = error.Location;
    if (loc != null && loc.Line > 0)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  at Line {loc.Line}, Column {loc.Column}");

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
    catch (ToastException ex) when (ex.Error.Message.Contains("end of file"))
    {
        return true;
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
