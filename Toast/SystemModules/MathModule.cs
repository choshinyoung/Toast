namespace Toast.SystemModules;

public class MathModule : IToastModule
{
    public string Name => "math";
    public string Description =>
        "Standard math operations and functions (sqrt, floorDiv, PI, E, operators)";

    public static readonly Command Sqrt = Command.CreateFunction(
        "sqrt",
        (Context context, NumberValue val) => new NumberValue(Math.Sqrt(val.Value)),
        description: "Returns the square root of a number.",
        returnType: ToastType.Number
    );

    public static readonly Command FloorDivision = Command.CreateFunction(
        "floorDiv",
        (Context ctx, NumberValue x, NumberValue y) =>
        {
            if (y.Value == 0)
            {
                throw new ToastException(RuntimeError.DivisionByZero());
            }
            return new NumberValue(Math.Floor(x.Value / y.Value));
        },
        precedence: 8,
        isInfix: true,
        description: "Floor division operator.",
        returnType: ToastType.Number
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(Sqrt);
        toast.RegisterCommand(FloorDivision);
        toast.GlobalContext.SetValueDirect("PI", new NumberValue(Math.PI));
        toast.GlobalContext.SetValueDirect("E", new NumberValue(Math.E));
    }

    public void Load(Toaster toaster, Context callerContext)
    {
        Register(toaster);
        callerContext.SetValueDirect("PI", new NumberValue(Math.PI));
        callerContext.SetValueDirect("E", new NumberValue(Math.E));
    }
}
