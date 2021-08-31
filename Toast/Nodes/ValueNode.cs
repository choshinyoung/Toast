namespace Toast.Nodes
{
    public class ValueNode : INode
    {
        public readonly object Value;

        internal ValueNode(object v)
        {
            Value = v;
        }
    }
}
