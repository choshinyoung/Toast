namespace Toast.Tokens
{
    public class Token
    {
        private readonly object value;

        public Token(object v)
        {
            value = v;
        }

        public object GetValue()
        {
            return value;
        }
    }
}
