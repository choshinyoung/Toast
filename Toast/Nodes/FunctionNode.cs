namespace Toast.Nodes
{
    public class FunctionNode : INode
    {
        public readonly string[] Parameters;
        public readonly INode[] Lines;

        public FunctionNode(string[] p, INode[] l)
        {
            Parameters = p;
            Lines = l;
        }
    }
}
