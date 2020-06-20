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

    class CommentNode : Node
    {
        public String Comment { get; set; }
    }

    class KeyCollectionNode : Node, IKeyNode
    {
        public String Key { get; set; }
        public List<Node> Children = new List<Node>();

        public void AddChild(Node child)
        {
            child.SetParent(this);
        }
    }

    class KeyValueNode : Node, IKeyNode, IValueNode
    {
        public String Key { get; set; }
        public String Value { get; set; }
    }

    class SingleValueNode : Node, IValueNode
    {
        public String Value { get; set; }
    }

}
