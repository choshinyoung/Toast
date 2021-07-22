namespace Toast.Tokens
{
    public class VariableNode : Token
    {
        public VariableNode(string value) : base(value) { }

        public new string GetValue()
        {
            return (string)base.GetValue();
        }
    }
}
