namespace Toast.Elements
{
    public class Element
    {
        private readonly object value;

        public Element(object v)
        {
            value = v;
        }

        public object GetValue()
        {
            return value;
        }
    }
}
