using Toast;

Console.WriteLine("========================================");
Console.WriteLine("  Toast Interactive REPL v2.0");
Console.WriteLine("========================================");
Console.WriteLine("Type exit or quit to end the session.\n");

var toast = new Toaster(useBuiltIn: true);
toast.RegisterFunction("exit", (Context context) => Environment.Exit(0));

while (true)
{
    Console.Write("toast> ");

    var input = ReadLineInteractive(toast);
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

static string? ReadLineInteractive(Toaster toast)
{
    if (Console.IsInputRedirected)
    {
        return Console.ReadLine();
    }

    var buffer = new System.Text.StringBuilder();

    while (true)
    {
        ConsoleKeyInfo keyInfo;
        try
        {
            keyInfo = Console.ReadKey(true);
        }
        catch (InvalidOperationException)
        {
            return Console.ReadLine();
        }

        if (keyInfo.Key == ConsoleKey.Enter)
        {
            if (IsIncomplete(buffer.ToString(), toast))
            {
                buffer.Append('\n');
                Console.WriteLine();
                Console.Write("       ");
            }
            else
            {
                Console.WriteLine();
                break;
            }
        }
        else if (keyInfo.Key == ConsoleKey.Backspace)
        {
            if (buffer.Length > 0)
            {
                char lastChar = buffer[buffer.Length - 1];
                if (lastChar == '\n')
                {
                    buffer.Remove(buffer.Length - 1, 1);

                    try
                    {
                        if (Console.CursorTop > 0)
                        {
                            Console.CursorLeft = 0;
                            Console.Write("       "); // overwrite 7 spaces
                            Console.CursorTop--;

                            string content = buffer.ToString();
                            int lastNewlineIdx = content.LastIndexOf('\n');
                            string lastLine =
                                lastNewlineIdx == -1
                                    ? content
                                    : content.Substring(lastNewlineIdx + 1);

                            int promptLen = 7; // Both "toast> " and "       " are 7 characters
                            Console.CursorLeft = promptLen + lastLine.Length;
                        }
                    }
                    catch
                    {
                        // Ignore cursor movement errors in non-interactive / restricted consoles
                    }
                }
                else
                {
                    buffer.Remove(buffer.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
        }
        else if (keyInfo.Key == ConsoleKey.Tab)
        {
            buffer.Append("    ");
            Console.Write("    ");
        }
        else
        {
            if (buffer.Length == 0 && (keyInfo.KeyChar == '\u001a' || keyInfo.KeyChar == '\u0004'))
            {
                return null;
            }

            if (keyInfo.KeyChar != '\0' && !char.IsControl(keyInfo.KeyChar))
            {
                buffer.Append(keyInfo.KeyChar);
                Console.Write(keyInfo.KeyChar);
            }
        }
    }

    return buffer.ToString();
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
