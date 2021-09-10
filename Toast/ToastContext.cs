namespace Toast
{
    public class ToastContext
    {
        public Toaster Toaster { get; internal set; }

        public int Depth;

        public ToastContext()
        {

        }

        public ToastContext(Toaster toaster)
        {
            Toaster = toaster;
            Depth = 0;
        }

        public ToastContext(Toaster toaster, int depth)
        {
            Toaster = toaster;
            Depth = depth;
        }
    }
}
