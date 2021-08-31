namespace Toast.Nodes
{
    public class ListNode : INode
    {
        public readonly INode[] Value;

        internal ListNode(INode[] list)
        {
            Value = list;
        }
    }
}
