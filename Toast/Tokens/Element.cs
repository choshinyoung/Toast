namespace Toast.Tokens
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
