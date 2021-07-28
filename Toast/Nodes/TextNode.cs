namespace Toast.Nodes
{
    public class TextNode : INode
    {
        public readonly object[] Values;

        public TextNode(object[] v)
        {
            Values = v;
        }
    }
}
