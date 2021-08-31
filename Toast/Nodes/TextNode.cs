namespace Toast.Nodes
{
    public class TextNode : INode
    {
        public readonly object[] Values;

        internal TextNode(object[] v)
        {
            Values = v;
        }
    }
}
