﻿namespace Toast.Nodes
{
    public class ListNode : INode
    {
        public readonly INode[] Value;

        public ListNode(INode[] list)
        {
            Value = list;
        }
    }
}