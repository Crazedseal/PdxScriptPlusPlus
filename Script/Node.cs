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
		public Boolean IsBeingCloned = false;
		

        public Node(String fileOrigin)
        {
            this.FileOrigin = fileOrigin;
        }

		public virtual String GetString()
		{
			return "";
		}

		public abstract Node Clone();


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

		public override Node Clone()
		{
			var clone = new CapNode(this.FileOrigin);
			clone.Depth = this.Depth;
			return clone;
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

		public override Node Clone()
		{
			var clone = new CommentNode(this.FileOrigin, this.Comment);
			clone.Depth = this.Depth;
			return clone;
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

		public bool HasStringValue()
		{
			return this.Value.StartsWith('"') && this.Value.EndsWith('"');
		}

		public override Node Clone()
		{
			KeyValueNode clone = new KeyValueNode(this.FileOrigin);
			clone.Key = this.Key;
			clone.Value = this.Value;
			clone.Operator = this.Operator;
			clone.Depth = this.Depth;
			return clone;
		}
	}

    class SingleValueNode : Node, IValueNode
    {
		public SingleValueNode(string fileOrigin) : base(fileOrigin) { }

		public String Value { get; set; }

		public bool HasStringValue()
		{
			return this.Value.StartsWith('"') && this.Value.EndsWith('"');
		}

		public override string GetString()
		{
			return Value;
		}

		public override Node Clone()
		{
			SingleValueNode clone = new SingleValueNode(this.FileOrigin);
			clone.Value = this.Value;
			clone.Depth = this.Depth;
			return clone;

		}
	}

}
