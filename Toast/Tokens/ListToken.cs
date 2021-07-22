namespace Toast.Tokens
{
    public class ListToken : Token
    {
        public ListToken(Token[][] value) : base(value) { }

        public new Token[][] GetValue()
        {
            return (Token[][])base.GetValue();
        }
    }
}
