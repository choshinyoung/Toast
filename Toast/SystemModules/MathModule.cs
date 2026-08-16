namespace Toast.SystemModules;

[ToastModule("math", Description = "Standard math functions.")]
public class MathModule : IToastModule
{
    [ToastObject("math")]
    public static class MathObject
    {
        public static readonly NumberValue PI = new(Math.PI);
        public static readonly NumberValue E = new(Math.E);

        [ToastCommand("sqrt", Description = "Returns the square root of a number.")]
        public static NumberValue Sqrt(NumberValue val)
        {
            return new NumberValue(Math.Sqrt(val.Value));
        }

        [ToastCommand(
            "floorDiv",
            Precedence = 8,
            IsInfix = true,
            Description = "Floor division operator."
        )]
        public static NumberValue FloorDiv(NumberValue x, NumberValue y)
        {
            if (y.Value == 0)
            {
                throw new ToastException(RuntimeError.DivisionByZero());
            }
            return new NumberValue(Math.Floor(x.Value / y.Value));
        }
    }
}
