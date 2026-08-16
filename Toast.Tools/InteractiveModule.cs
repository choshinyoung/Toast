namespace Toast.Tools;

[ToastModule("interactive", Description = "Interactive REPL helper commands.")]
public class InteractiveModule : IToastModule
{
    [ToastCommand("exit", Description = "Exits the interactive session.")]
    public static void Exit()
    {
        Environment.Exit(0);
    }
}
