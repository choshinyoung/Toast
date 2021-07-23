namespace Toast.Nodes
{
    public class CommandNode : INode
    {
        public readonly ToastCommand Command;
        public readonly INode[] Parameters;

        public CommandNode(ToastCommand c, INode[] p)
        {
            Command = c;
            Parameters = p;
        }
    }
}
