namespace Toast.Tokens
{
    public class NumberToken : Token
    {
        public NumberToken(float value) : base(value) { }

        public new float GetValue()
        {
            return (float)base.GetValue();
        }
    }
}
