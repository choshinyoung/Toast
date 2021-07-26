namespace Toast
{
    public class ToastContext
    {
        public Toaster Toaster { get; internal set; }

        public ToastContext()
        {

        }

        public ToastContext(Toaster toaster)
        {
            Toaster = toaster;
        }
    }
}
