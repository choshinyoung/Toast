namespace Toast.Nodes
{
    public class VariableNode : INode
    {
        public readonly string Name;

        public VariableNode(string n)
        {
            Name = n;
        }
    }
}
