namespace Toast.Tokens
{
    public class GroupToken : Token
    {
        public GroupToken(Token[][] value) : base(value) { }

        public new Token[][] GetValue()
        {
            return (Token[][])base.GetValue();
        }
    }
}
