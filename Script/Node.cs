using System;
using System.Collections.Generic;
using System.Text;

namespace PdxScriptPlusPlus.Script
{
    abstract class Node
    {
        public KeyCollectionNode Parent { get; private set; }

        public virtual void SetParent(KeyCollectionNode parent)
        {
            KeyCollectionNode oldParent = Parent;
            this.Parent = parent;
            oldParent.Children.Remove(this);
            this.Parent.Children.Add(this);
        }
    }

    class KeyNode
    {
        public String Key { get; set; }
    }

    class KeyCollectionNode : KeyNode
    {
        public List<Node> Children = new List<Node>();

        public void AddChild(Node child)
        {
            child.SetParent(this);
        }
    }

    class KeyValueNode : Node
    {

    }
}
