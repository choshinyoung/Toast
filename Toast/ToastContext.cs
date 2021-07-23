namespace Toast
{
    public class ToastContext
    {
        public readonly Toaster Toaster;

        internal ToastContext(Toaster toaster)
        {
            Toaster = toaster;
        }
    }
}
