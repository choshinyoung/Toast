namespace Toast.Tokens
{
    public class Number : Element
    {
        public Number(float value) : base(value) { }

        public new float GetValue()
        {
            return (float)base.GetValue();
        }
    }
}
