using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Toast;

public record ModuleMetadata(
    IReadOnlyList<ToastType> Types,
    IReadOnlyList<TypeConverter> Converters,
    IReadOnlyList<Command> Commands,
    IReadOnlyList<(ToastType TargetType, string MemberName, Command Command)> Members,
    IReadOnlyList<(
        string ObjectName,
        IReadOnlyList<(string Name, ToastValue Value)> Values
    )> Objects
);

public static class ModuleLoader
{
    private static readonly ConcurrentDictionary<Type, ModuleMetadata> Cache = new();
    private const BindingFlags AllMembers =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    public static void Load(IToastModule module, Toaster toaster, Context callerContext)
    {
        var metadata = GetOrAnalyzeMetadata(module.GetType());

        foreach (var type in metadata.Types)
        {
            toaster.RegisterType(type);
        }

        foreach (var conv in metadata.Converters)
        {
            toaster.RegisterConverter(conv);
        }

        foreach (var cmd in metadata.Commands)
        {
            toaster.RegisterCommand(cmd);
        }

        foreach (var (targetType, memberName, cmd) in metadata.Members)
        {
            toaster.RegisterTypeMember(targetType, memberName, new CommandValue(cmd));
        }

        foreach (var (objName, objValues) in metadata.Objects)
        {
            var objCtx = new Context(toaster.GlobalContext);
            foreach (var (propName, propVal) in objValues)
            {
                objCtx.SetValueDirect(propName, propVal);
            }
            var objVal = new ObjectValue(objCtx, new ToastType(objName));
            callerContext.SetValueDirect(objName, objVal);
        }

        module.OnLoad(toaster, callerContext);
    }

    public static string GetModuleName(Type type)
    {
        var attr = type.GetCustomAttribute<ToastModuleAttribute>();
        if (!string.IsNullOrEmpty(attr?.Name))
        {
            return attr.Name;
        }

        var name = type.Name;
        if (name.EndsWith("Module", StringComparison.OrdinalIgnoreCase) && name.Length > 6)
        {
            name = name[..^6];
        }

        return name.ToLowerInvariant();
    }

    public static string GetModuleDescription(Type type)
    {
        var attr = type.GetCustomAttribute<ToastModuleAttribute>();
        return attr?.Description ?? "";
    }

    public static ModuleMetadata GetOrAnalyzeMetadata(Type moduleType)
    {
        return Cache.GetOrAdd(moduleType, AnalyzeMetadata);
    }

    private static ModuleMetadata AnalyzeMetadata(Type moduleType)
    {
        var types = new List<ToastType>();
        var converters = new List<TypeConverter>();
        var commands = new List<Command>();
        var members = new List<(ToastType TargetType, string MemberName, Command Command)>();
        var objects =
            new List<(string ObjectName, IReadOnlyList<(string Name, ToastValue Value)> Values)>();

        foreach (var field in moduleType.GetFields(AllMembers))
        {
            if (field.FieldType == typeof(ToastType) && field.IsStatic)
            {
                if (field.GetValue(null) is ToastType tt)
                {
                    types.Add(tt);
                }
            }
        }

        foreach (var prop in moduleType.GetProperties(AllMembers))
        {
            if (
                prop.PropertyType == typeof(ToastType)
                && prop.GetMethod?.IsStatic == true
                && prop.CanRead
            )
            {
                if (prop.GetValue(null) is ToastType tt)
                {
                    types.Add(tt);
                }
            }
        }

        foreach (var field in moduleType.GetFields(AllMembers))
        {
            if (typeof(TypeConverter).IsAssignableFrom(field.FieldType) && field.IsStatic)
            {
                if (field.GetValue(null) is TypeConverter conv)
                {
                    converters.Add(conv);
                }
            }
        }

        foreach (var method in moduleType.GetMethods(AllMembers))
        {
            var convAttr = method.GetCustomAttribute<ToastConverterAttribute>();
            if (convAttr != null)
            {
                var conv = CreateConverterFromMethod(method, convAttr);
                converters.Add(conv);
            }
        }

        foreach (var field in moduleType.GetFields(AllMembers))
        {
            if (typeof(Command).IsAssignableFrom(field.FieldType) && field.IsStatic)
            {
                if (field.GetValue(null) is Command cmd)
                {
                    commands.Add(cmd);
                }
            }
        }

        foreach (var method in moduleType.GetMethods(AllMembers))
        {
            var cmdAttr = method.GetCustomAttribute<ToastCommandAttribute>();
            if (cmdAttr == null)
                continue;

            var cmd = CreateCommandFromMethod(null, method, cmdAttr);
            commands.Add(cmd);
        }

        foreach (var nestedType in moduleType.GetNestedTypes(AllMembers))
        {
            var typeAttr = nestedType.GetCustomAttribute<ToastTypeAttribute>();
            if (typeAttr != null)
            {
                var typeName = typeAttr.Name ?? nestedType.Name;
                if (
                    typeName.EndsWith("Type", StringComparison.OrdinalIgnoreCase)
                    && typeName.Length > 4
                )
                {
                    typeName = typeName[..^4];
                }
                var toastType = new ToastType(typeName.ToLowerInvariant());
                types.Add(toastType);

                foreach (var method in nestedType.GetMethods(AllMembers))
                {
                    var cmdAttr = method.GetCustomAttribute<ToastCommandAttribute>();
                    if (cmdAttr != null)
                    {
                        var cmd = CreateCommandFromMethod(null, method, cmdAttr);
                        members.Add((toastType, cmd.Name, cmd));
                    }
                }

                foreach (var field in nestedType.GetFields(AllMembers))
                {
                    if (typeof(Command).IsAssignableFrom(field.FieldType) && field.IsStatic)
                    {
                        if (field.GetValue(null) is Command cmd)
                        {
                            members.Add((toastType, cmd.Name, cmd));
                        }
                    }
                }

                continue;
            }

            var objAttr = nestedType.GetCustomAttribute<ToastObjectAttribute>();
            if (objAttr != null)
            {
                var objName = objAttr.Name ?? nestedType.Name;
                var objValues = new List<(string Name, ToastValue Value)>();

                foreach (var method in nestedType.GetMethods(AllMembers))
                {
                    var cmdAttr = method.GetCustomAttribute<ToastCommandAttribute>();
                    if (cmdAttr != null)
                    {
                        var cmd = CreateCommandFromMethod(null, method, cmdAttr);
                        objValues.Add((cmd.Name, new CommandValue(cmd)));
                    }
                }

                foreach (var field in nestedType.GetFields(AllMembers))
                {
                    if (!field.IsStatic)
                        continue;
                    if (typeof(Command).IsAssignableFrom(field.FieldType))
                    {
                        if (field.GetValue(null) is Command cmd)
                        {
                            objValues.Add((cmd.Name, new CommandValue(cmd)));
                        }
                    }
                    else if (typeof(ToastValue).IsAssignableFrom(field.FieldType))
                    {
                        if (field.GetValue(null) is ToastValue tv)
                        {
                            objValues.Add((field.Name, tv));
                        }
                    }
                }

                foreach (var prop in nestedType.GetProperties(AllMembers))
                {
                    if (
                        prop.GetMethod?.IsStatic == true
                        && typeof(ToastValue).IsAssignableFrom(prop.PropertyType)
                        && prop.CanRead
                    )
                    {
                        if (prop.GetValue(null) is ToastValue tv)
                        {
                            objValues.Add((prop.Name, tv));
                        }
                    }
                }

                objects.Add((objName, objValues));
            }
        }

        return new ModuleMetadata(types, converters, commands, members, objects);
    }

    private static TypeConverter CreateConverterFromMethod(
        MethodInfo method,
        ToastConverterAttribute attr
    )
    {
        var parameters = method.GetParameters();
        var hasContext =
            parameters.Length > 0 && typeof(Context).IsAssignableFrom(parameters[0].ParameterType);
        var valParamIndex = hasContext ? 1 : 0;

        if (parameters.Length <= valParamIndex)
        {
            throw new InvalidOperationException(
                $"ToastConverter method '{method.Name}' must have a value parameter to convert from."
            );
        }

        var srcClrType = parameters[valParamIndex].ParameterType;
        var targetClrType = method.ReturnType;

        var srcType = !string.IsNullOrEmpty(attr.SourceTypeName)
            ? new ToastType(attr.SourceTypeName)
            : (ToastType.TryFromClrType(srcClrType) ?? ToastType.Any);

        var targetType = !string.IsNullOrEmpty(attr.TargetTypeName)
            ? new ToastType(attr.TargetTypeName)
            : (ToastType.TryFromClrType(targetClrType) ?? ToastType.Any);

        var ctxParam = Expression.Parameter(typeof(Context), "ctx");
        var valParam = Expression.Parameter(typeof(ToastValue), "val");
        var castVal = Expression.Convert(valParam, srcClrType);

        Expression callExpr = hasContext
            ? Expression.Call(method, ctxParam, castVal)
            : Expression.Call(method, castVal);

        var castResult = Expression.Convert(callExpr, typeof(ToastValue));
        var lambda = Expression.Lambda<Func<Context, ToastValue, ToastValue>>(
            castResult,
            ctxParam,
            valParam
        );
        var convertFunc = lambda.Compile();

        return new TypeConverter(srcType, targetType, convertFunc);
    }

    private static Command CreateCommandFromMethod(
        object? instance,
        MethodInfo method,
        ToastCommandAttribute attr
    )
    {
        var name = attr.Name ?? method.Name;
        Delegate del;

        if (method.IsStatic)
        {
            del = method.CreateDelegate(
                Expression.GetDelegateType([
                    .. method.GetParameters().Select(p => p.ParameterType),
                    method.ReturnType,
                ])
            );
        }
        else
        {
            instance ??= Activator.CreateInstance(method.DeclaringType!);
            del = method.CreateDelegate(
                Expression.GetDelegateType([
                    .. method.GetParameters().Select(p => p.ParameterType),
                    method.ReturnType,
                ]),
                instance
            );
        }

        if (attr.Precedence > 0 || attr.IsPrefix || attr.IsInfix)
        {
            return Command.CreateOperator(
                name,
                del,
                precedence: attr.Precedence,
                isInfix: attr.IsInfix,
                isRightAssociative: attr.IsRightAssociative,
                isPrefix: attr.IsPrefix,
                description: attr.Description
            );
        }

        return Command.CreateFunction(
            name,
            del,
            description: attr.Description,
            declaresMember: attr.DeclaresMember
        );
    }
}
