namespace Toast.Tools;

public static class ModuleRunner
{
    public static async Task<int> InstallAsync(string source, string? name)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Installing module from '{source}'...");
            Console.ResetColor();

            if (
                source.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || source.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            )
            {
                await ModuleManager.Instance.InstallFromUrlAsync(source, name);
            }
            else
            {
                ModuleManager.Instance.InstallLocalFile(source, name);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
                $"Successfully installed module to global directory ({ModuleManager.Instance.GlobalModulesDirectory})."
            );
            Console.ResetColor();
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error installing module: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    public static int List()
    {
        var allModules = ModuleManager.Instance.GetAllModules();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== Toast Modules ===");
        Console.ResetColor();

        var systemModules = allModules.Where(m => m.IsSystem).ToList();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n[System Modules] (Protected)");
        Console.ResetColor();
        foreach (var mod in systemModules)
        {
            Console.WriteLine($"  * {mod.Name, -12} - {mod.Description}");
        }

        var installed = allModules.Where(m => !m.IsSystem).ToList();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(
            $"\n[Globally Installed Modules] ({ModuleManager.Instance.GlobalModulesDirectory})"
        );
        Console.ResetColor();
        if (installed.Count == 0)
        {
            Console.WriteLine("  (No installed modules)");
        }
        else
        {
            foreach (var mod in installed)
            {
                Console.WriteLine($"  * {mod.Name, -12} - {mod.Path}");
            }
        }
        Console.WriteLine();
        return 0;
    }

    public static int Uninstall(string name)
    {
        try
        {
            bool removed = ModuleManager.Instance.UninstallModule(name);
            if (removed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Successfully uninstalled module '{name}'.");
                Console.ResetColor();
                return 0;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Module '{name}' was not found in global directory.");
                Console.ResetColor();
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }
}
