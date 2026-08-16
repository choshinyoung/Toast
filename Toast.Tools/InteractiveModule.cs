namespace Toast.Tools;

[ToastModule("interactive", "Interactive REPL helper commands.")]
public class InteractiveModule : IToastModule
{
    [ToastCommand("exit", "Exits the interactive session.")]
    public static void Exit()
    {
        Environment.Exit(0);
    }
}
