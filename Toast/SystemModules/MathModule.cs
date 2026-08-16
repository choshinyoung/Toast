namespace Toast.SystemModules;

[ToastModule("math", "Standard math functions.")]
public class MathModule : IToastModule
{
    [ToastObject("math", "Math namespace")]
    public static class MathObject
    {
        public static readonly NumberValue PI = new(Math.PI);
        public static readonly NumberValue E = new(Math.E);

        [ToastCommand("sqrt", "Returns the square root of a number.")]
        public static NumberValue Sqrt(NumberValue val)
        {
            return new NumberValue(Math.Sqrt(val.Value));
        }

        [ToastCommand("floorDiv", "Floor division operator.", Precedence = 8, IsInfix = true)]
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
