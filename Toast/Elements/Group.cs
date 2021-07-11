namespace Toast.Elements
{
    public class Group : Element
    {
        public Group(Element[] value) : base(value) { }

        public new Element[] GetValue()
        {
            return (Element[])base.GetValue();
        }
    }
}
