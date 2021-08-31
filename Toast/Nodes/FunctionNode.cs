namespace Toast.Nodes
{
    public class FunctionNode : INode
    {
        public readonly string[] Parameters;
        public readonly INode[] Lines;

        internal FunctionNode(string[] p, INode[] l)
        {
            Parameters = p;
            Lines = l;
        }
    }
}
