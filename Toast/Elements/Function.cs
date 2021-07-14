namespace Toast.Elements
{
    public class Function : Element
    {
        public Function(Element[][] value) : base(value) { }

        public new Element[][] GetValue()
        {
            return (Element[][])base.GetValue();
        }
    }
}
