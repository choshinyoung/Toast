using System.Text;

namespace Toast.Tools;

public static class InteractiveRunner
{
    public static void Run(string version)
    {
        Console.WriteLine($"Toast {version}");
        Console.WriteLine("Type \"exit\" to leave.\n");

        var toast = new Toaster([
            new SystemModules.ImportModule(),
            new SystemModules.SystemModule(),
            new InteractiveModule(),
        ]);

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
                    if (
                        toast.Converters.TryGetValue(
                            (toastType, ToastType.String),
                            out var converter
                        )
                    )
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
                ErrorFormatter.PrintError(ex.Error, input, "<repl>");
            }
            catch (Exception ex)
            {
                ErrorFormatter.PrintError(new RuntimeError(ex.Message), input, "<repl>");
            }
        }
    }

    private static bool IsIncomplete(string input, Toaster toast)
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
}
