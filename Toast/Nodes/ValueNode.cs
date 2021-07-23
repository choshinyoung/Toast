namespace Toast.Nodes
{
    public class ValueNode : INode
    {
        public readonly object Value;

        public ValueNode(object v)
        {
            Value = v;
        }
    }
}
