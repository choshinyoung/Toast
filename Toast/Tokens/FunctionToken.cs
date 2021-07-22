namespace Toast.Tokens
{
    public class FunctionToken : Token
    {
        public string[] Parameters;

        public FunctionToken(Token[][] value, string[] parameters) : base(value)
        {
            Parameters = parameters;
        }

        public new Token[][] GetValue()
        {
            return (Token[][])base.GetValue();
        }

        public string[] GetParameters() => Parameters;
    }
}
