namespace Toast.Nodes
{
    class FunctionNode : INode
    {
        private readonly string[] Parameters;
        private readonly INode[] Lines;

        public FunctionNode(string[] p, INode[] l)
        {
            Parameters = p;
            Lines = l;
        }
    }
}
