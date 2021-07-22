namespace Toast.Tokens
{
    public class CommandToken : Token
    {
        public CommandToken(string value) : base(value) { }

        public new string GetValue()
        {
            return (string)base.GetValue();
        }
    }
}
