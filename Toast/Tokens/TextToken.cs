namespace Toast.Tokens
{
    public class TextToken : Token
    {
        public TextToken(object[] value) : base(value) { }

        public new object[] GetValue()
        {
            return (object[])base.GetValue();
        }
    }
}
