﻿namespace Toast.Nodes
{
    public class GroupNode : INode
    {
        public readonly INode[] Values;

        public GroupNode(INode[] values)
        {
            Values = values;
        }
    }
}