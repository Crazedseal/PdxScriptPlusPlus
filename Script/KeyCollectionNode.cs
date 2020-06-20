using System;
using System.Collections.Generic;
using System.Text;

namespace PdxScriptPlusPlus.Script
{
	class KeyCollectionNode : Node, IKeyNode
	{
		public KeyCollectionNode(String fileOrigin, String key) : base(fileOrigin)
		{
			this.Key = key;
		}

		public Boolean IsRoot()
		{
			if (Context != null)
				return Context.IsRootNode(this);
			else
				return false;
		}

		public ParseContext Context { get; private set; }

		public Boolean SetContext(ParseContext context)
		{
			if (this.IsRoot())
			{
				return false; // We cannot change our context if we are the root of another context.
			}
			else if (this.Parent != null && context != this.Parent.Context)
			{
				return false; // We cannot be in a different Parse Context than our parent.
			}
			else
			{
				this.Context = context; return true; // Ok.
			}
		}

		public String Key { get; set; }
		public List<Node> Children = new List<Node>();

		public void AddChild(Node child)
		{
			child.SetParent(this);
			this.Children.Add(child);
		}
	}
}
