namespace Toast.Tokens
{
    public class NumberToken : Token
    {
        public NumberToken(object value) : base(value) { }

        public new object GetValue()
        {
            return base.GetValue();
        }
    }
}
