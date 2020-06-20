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

		public void Print()
		{
			Stack<Node> nodeStack = new Stack<Node>();
			nodeStack.Push(this);

			while(nodeStack.Count > 0)
			{
				Node currentNode = nodeStack.Pop();

				switch (currentNode)
				{
					case KeyCollectionNode kcn:
						List<Node> reverse = new List<Node>(kcn.Children);
						reverse.Reverse();
						foreach (Node child in reverse) { nodeStack.Push(child); }
						Console.WriteLine(kcn.Key + " = {");
						break;
					case KeyValueNode kvn:
						Console.WriteLine(kvn.Key + " " + kvn.Operator + " " + kvn.Value);
						break;
					case CommentNode cn:
						Console.WriteLine("#" + cn.Comment);
						break;
					case SingleValueNode svn:
						Console.Write(svn.Value + " ");
						break;

				}
			}
		}
	}
}
