namespace Toast.Tokens
{
    public class List : Element
    {
        public List(Element[][] value) : base(value) { }

        public new Element[][] GetValue()
        {
            return (Element[][])base.GetValue();
        }
    }
}
