namespace Toast.Elements
{
    public class Function : Element
    {
        public string[] Parameters;

        public Function(Element[][] value, string[] parameters) : base(value) 
        {
            Parameters = parameters;
        }

        public new Element[][] GetValue()
        {
            return (Element[][])base.GetValue();
        }

        public string[] GetParameters() => Parameters;
    }
}
