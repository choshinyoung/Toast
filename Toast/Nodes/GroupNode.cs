namespace Toast.Nodes
{
    public class GroupNode : INode
    {
        public readonly INode[] Values;

        internal GroupNode(INode[] values)
        {
            Values = values;
        }
    }
}
