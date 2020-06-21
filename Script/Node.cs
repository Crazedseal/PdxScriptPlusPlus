using System;
using System.Collections.Generic;
using System.Text;

namespace PdxScriptPlusPlus.Script
{
    abstract class Node
    {
        public KeyCollectionNode Parent { get; private set; }
        public String FileOrigin { get; private set; }
		public Byte Depth = 0;
        public Node(String fileOrigin)
        {
            this.FileOrigin = fileOrigin;
        }

		public virtual String GetString()
		{
			return "";
		}

        public virtual void SetParent(KeyCollectionNode parent)
        {
			if (Parent != null)
			{
				KeyCollectionNode oldParent = Parent;
				oldParent.Children.Remove(this);
			}
			this.Parent = parent;
			this.Parent.Children.Add(this);
        }
    }

	class CapNode : Node
	{
		internal CapNode(string fileOrigin) : base(fileOrigin)
		{

		}

		
	}

	class CommentNode : Node
    {
        public CommentNode(String fileOrigin, String comment) : base(fileOrigin)
        {
            this.Comment = comment;
        }

		public override string GetString()
		{
			return "#" + Comment;
		}


		public String Comment { get; set; }
    }

    

    class KeyValueNode : Node, IKeyNode, IValueNode
    {
		public KeyValueNode(string fileOrigin) : base(fileOrigin) { }
        public String Key { get; set; }
        public String Value { get; set; }
		public Char Operator { get; set; }

		public override string GetString()
		{
			return Key + " " + Operator + " " + Value;
		}
	}

    class SingleValueNode : Node, IValueNode
    {
		public SingleValueNode(string fileOrigin) : base(fileOrigin) { }

		public String Value { get; set; }

		public override string GetString()
		{
			return Value;
		}
	}

}
