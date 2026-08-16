using System.Reflection;
using System.Runtime.Loader;
using Toast.SystemModules;

namespace Toast;

public record ToastModuleInfo(string Name, string Description, bool IsSystem, string? Path = null);

public class ModuleManager
{
    public static readonly ModuleManager Instance = new();

    public string GlobalModulesDirectory { get; set; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".toast",
            "modules"
        );

    private readonly Dictionary<string, IToastModule> _systemModules = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["system"] = new SystemModule(),
        ["default"] = new DefaultModule(),
        ["object"] = new ObjectModule(),
        ["flow"] = new FlowModule(),
        ["converter"] = new ConverterModule(),
        ["import"] = new ImportModule(),
        ["math"] = new MathModule(),
        ["datetime"] = new DateTimeModule(),
        ["string"] = new StringModule(),
        ["list"] = new ListModule(),
        ["utility"] = new UtilityModule(),
    };

    public IReadOnlyDictionary<string, IToastModule> SystemModules => _systemModules;

    public static bool IsValidModuleName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        if (name.Contains('/') || name.Contains('\\') || name.Contains("..") || name.Contains(':'))
        {
            return false;
        }

        return true;
    }

    public void EnsureDefaultModules()
    {
        try
        {
            if (!Directory.Exists(GlobalModulesDirectory))
            {
                Directory.CreateDirectory(GlobalModulesDirectory);
            }

            var systemScriptPath = Path.Combine(GlobalModulesDirectory, "system.toast");
            if (File.Exists(systemScriptPath))
            {
                File.Delete(systemScriptPath);
            }
        }
        catch { }
    }

    public List<ToastModuleInfo> GetAllModules()
    {
        EnsureDefaultModules();
        var list = new List<ToastModuleInfo>();

        foreach (var (name, mod) in _systemModules)
        {
            list.Add(new ToastModuleInfo(name, mod.Description, IsSystem: true));
        }

        if (Directory.Exists(GlobalModulesDirectory))
        {
            foreach (var dir in Directory.GetDirectories(GlobalModulesDirectory))
            {
                var modName = Path.GetFileName(dir);
                if (list.All(m => !m.Name.Equals(modName, StringComparison.OrdinalIgnoreCase)))
                {
                    list.Add(
                        new ToastModuleInfo(
                            modName,
                            "Globally installed module package",
                            IsSystem: false,
                            Path: dir
                        )
                    );
                }
            }
            foreach (var file in Directory.GetFiles(GlobalModulesDirectory))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext is ".dll" or ".toast")
                {
                    var modName = Path.GetFileNameWithoutExtension(file);
                    if (list.All(m => !m.Name.Equals(modName, StringComparison.OrdinalIgnoreCase)))
                    {
                        list.Add(
                            new ToastModuleInfo(
                                modName,
                                $"Globally installed {ext} module",
                                IsSystem: false,
                                Path: file
                            )
                        );
                    }
                }
            }
        }

        return list;
    }

    public void LoadModule(string moduleName, Toaster toaster, Context callerContext)
    {
        if (!IsValidModuleName(moduleName))
        {
            throw new ToastException(
                new RuntimeError(
                    $"Invalid module name '{moduleName}'. Direct path imports are not allowed for security reasons."
                )
            );
        }

        if (_systemModules.TryGetValue(moduleName, out var systemMod))
        {
            toaster.Load(systemMod);
            return;
        }

        if (Directory.Exists(GlobalModulesDirectory))
        {
            var dllCandidate1 = Path.Combine(GlobalModulesDirectory, $"{moduleName}.dll");
            var dllCandidate2 = Path.Combine(
                GlobalModulesDirectory,
                moduleName,
                $"{moduleName}.dll"
            );
            var targetDll = File.Exists(dllCandidate1)
                ? dllCandidate1
                : (File.Exists(dllCandidate2) ? dllCandidate2 : null);

            if (targetDll != null)
            {
                LoadDllModule(targetDll, moduleName, toaster, callerContext);
                return;
            }

            var toastCandidate1 = Path.Combine(GlobalModulesDirectory, $"{moduleName}.toast");
            var toastCandidate2 = Path.Combine(
                GlobalModulesDirectory,
                moduleName,
                $"{moduleName}.toast"
            );
            var targetToast = File.Exists(toastCandidate1)
                ? toastCandidate1
                : (File.Exists(toastCandidate2) ? toastCandidate2 : null);

            if (targetToast != null)
            {
                LoadToastScriptModule(targetToast, toaster, callerContext);
                return;
            }
        }

        throw new ToastException(
            new RuntimeError(
                $"Module '{moduleName}' not found. Please install it using 'toast module install <source>'."
            )
        );
    }

    private static void LoadDllModule(
        string dllPath,
        string moduleName,
        Toaster toaster,
        Context callerContext
    )
    {
        var loadContext = new AssemblyLoadContext(moduleName, isCollectible: true);
        var assembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath(dllPath));

        var moduleType = assembly
            .GetTypes()
            .FirstOrDefault(t =>
                typeof(IToastModule).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface
            );

        if (
            moduleType != null
            && Activator.CreateInstance(moduleType) is IToastModule moduleInstance
        )
        {
            toaster.Load(moduleInstance);
            return;
        }

        var methods = assembly
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static));
        foreach (var m in methods)
        {
            if (m.IsSpecialName)
                continue;
            var cmd = Command.CreateFunction(m.Name, m.CreateDelegate(typeof(Delegate)));
            toaster.RegisterCommand(cmd);
        }
    }

    private static void LoadToastScriptModule(
        string scriptPath,
        Toaster toaster,
        Context callerContext
    )
    {
        var code = File.ReadAllText(scriptPath);
        var moduleCtx = new Context(toaster, toaster.GlobalContext);
        toaster.Execute(code, moduleCtx);

        foreach (var (k, v) in moduleCtx.GetBindings())
        {
            callerContext.SetValueDirect(k, v.Value);
        }
    }

    public void InstallLocalFile(string sourceFilePath, string? targetModuleName = null)
    {
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException($"Source file '{sourceFilePath}' does not exist.");
        }

        var ext = Path.GetExtension(sourceFilePath).ToLowerInvariant();
        if (ext is not (".dll" or ".toast"))
        {
            throw new InvalidOperationException(
                $"Unsupported package file extension '{ext}'. Only .dll and .toast are supported."
            );
        }

        var name = !string.IsNullOrWhiteSpace(targetModuleName)
            ? targetModuleName
            : Path.GetFileNameWithoutExtension(sourceFilePath);

        if (!IsValidModuleName(name))
        {
            throw new InvalidOperationException($"Invalid target module name '{name}'.");
        }

        if (_systemModules.ContainsKey(name))
        {
            throw new InvalidOperationException($"Cannot overwrite system module '{name}'.");
        }

        var targetDir = Path.Combine(GlobalModulesDirectory, name);
        Directory.CreateDirectory(targetDir);

        var destination = Path.Combine(targetDir, $"{name}{ext}");
        File.Copy(sourceFilePath, destination, overwrite: true);
    }

    public async Task InstallFromUrlAsync(string url, string? targetModuleName = null)
    {
        using var client = new HttpClient();
        var uri = new Uri(url);
        var fileName = Path.GetFileName(uri.LocalPath);
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        if (ext is not (".dll" or ".toast"))
        {
            ext = ".dll";
        }

        var name = !string.IsNullOrWhiteSpace(targetModuleName)
            ? targetModuleName
            : Path.GetFileNameWithoutExtension(fileName);

        if (string.IsNullOrWhiteSpace(name))
        {
            name = "module";
        }

        if (!IsValidModuleName(name))
        {
            throw new InvalidOperationException($"Invalid target module name '{name}'.");
        }

        if (_systemModules.ContainsKey(name))
        {
            throw new InvalidOperationException($"Cannot overwrite system module '{name}'.");
        }

        var bytes = await client.GetByteArrayAsync(uri);

        var targetDir = Path.Combine(GlobalModulesDirectory, name);
        Directory.CreateDirectory(targetDir);

        var destination = Path.Combine(targetDir, $"{name}{ext}");
        await File.WriteAllBytesAsync(destination, bytes);
    }

    public bool UninstallModule(string moduleName)
    {
        if (!IsValidModuleName(moduleName))
            return false;

        if (_systemModules.ContainsKey(moduleName))
        {
            throw new InvalidOperationException(
                $"System module '{moduleName}' cannot be uninstalled."
            );
        }

        bool removed = false;
        var dir = Path.Combine(GlobalModulesDirectory, moduleName);
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, recursive: true);
            removed = true;
        }

        var file1 = Path.Combine(GlobalModulesDirectory, $"{moduleName}.dll");
        if (File.Exists(file1))
        {
            File.Delete(file1);
            removed = true;
        }

        var file2 = Path.Combine(GlobalModulesDirectory, $"{moduleName}.toast");
        if (File.Exists(file2))
        {
            File.Delete(file2);
            removed = true;
        }

        return removed;
    }
}
