namespace Toast.Tokens
{
    public class TextToken : Token
    {
        public TextToken(string value) : base(value) { }

        public new string GetValue()
        {
            return (string)base.GetValue();
        }
    }
}
