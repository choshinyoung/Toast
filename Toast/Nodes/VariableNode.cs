namespace Toast.Nodes
{
    public class VariableNode : INode
    {
        public readonly string Name;

        internal VariableNode(string n)
        {
            Name = n;
        }
    }
}
