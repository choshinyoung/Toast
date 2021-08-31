namespace Toast.Nodes
{
    public class CommandNode : INode
    {
        public readonly ToastCommand Command;
        public readonly INode[] Parameters;

        internal CommandNode(ToastCommand c, INode[] p)
        {
            Command = c;
            Parameters = p;
        }
    }
}
