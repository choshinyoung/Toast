namespace Toast.Elements
{
    public class Variable : Element
    {
        public Variable(string value) : base(value) { }

        public new string GetValue()
        {
            return (string)base.GetValue();
        }
    }
}
