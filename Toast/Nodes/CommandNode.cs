namespace Toast.Nodes
{
    public class CommandNode : INode
    {
        public readonly ToastCommand Command;
        public readonly INode[] Childs;

        public CommandNode(ToastCommand c, INode[] childs)
        {
            Command = c;
            Childs = childs;
        }
    }
}
