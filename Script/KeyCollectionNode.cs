using System;
using System.Collections.Generic;
using System.Text;

namespace PdxScriptPlusPlus.Script
{
	class KeyCollectionNode : Node, IKeyNode
	{
		HashSet<Node> child_unique = new HashSet<Node>();

		public KeyCollectionNode(String fileOrigin, String key) : base(fileOrigin)
		{
			this.Key = key;
		}

		public override Node Clone()
		{
			KeyCollectionNode clone = new KeyCollectionNode(this.FileOrigin, this.Key);
			clone.Depth = this.Depth;
			foreach (var child in this.Children)
			{
				Node childClone = child.Clone();
				childClone.SetParent(clone);
				
			}

			return clone;
		}

		public Boolean IsRoot()
		{
			if (Context != null)
				return Context.IsRootNode(this);
			else
				return false;
		}

		public override string GetString()
		{
			return this.Key + " = {";
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
		}

		public void Print()
		{
			Stack<Node> nodeStack = new Stack<Node>();
			nodeStack.Push(this);

			Node currentNode = null;
			while(nodeStack.Count > 0)
			{
				Node previousNode = currentNode;
				currentNode = nodeStack.Pop();

				if (previousNode != null && previousNode.Depth > currentNode.Depth)
				{
					int depthDifference = previousNode.Depth - currentNode.Depth;
					for (int i = 0; i != depthDifference; i++)
					{
						for (int j = 0; j < (previousNode.Depth-1) - i; j++)
						{
							Console.Write('\t');
						}
						Console.WriteLine("}");
					}
				}

				for (int i = 0; i < currentNode.Depth; i++)
				{
					Console.Write('\t');
				}

				if (currentNode.GetType() == typeof(KeyCollectionNode))
				{
					KeyCollectionNode keyCollectionNode = (KeyCollectionNode)currentNode;
					List<Node> reverse = new List<Node>(keyCollectionNode.Children);
					reverse.Reverse();
					foreach (Node child in reverse) { nodeStack.Push(child); }
					if (!keyCollectionNode.IsRoot()) { Console.WriteLine(currentNode.GetString()); }
				}
				else
				{
					Console.WriteLine(currentNode.GetString());
				}


			}
		}

		public String PrintString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Stack<Node> nodeStack = new Stack<Node>();

			CapNode cap = new CapNode(this.FileOrigin);
			cap.Depth = this.Depth;

			if (!this.IsRoot()) { nodeStack.Push(cap); }

			nodeStack.Push(this);
			int startingDepth = this.Depth;

			Node currentNode = null;
			while (nodeStack.Count > 0)
			{
				Node previousNode = currentNode;
				currentNode = nodeStack.Pop();
				
				if (previousNode != null && previousNode.Depth > currentNode.Depth)
				{
					int depthDifference = previousNode.Depth - currentNode.Depth;
					for (int i = 0; i != depthDifference; i++)
					{
						for (int j = startingDepth; j < (previousNode.Depth - 1) - i; j++)
						{
							stringBuilder.Append('\t');
						}
						stringBuilder.AppendLine("}");
					}
				}
				if (currentNode.GetType() == typeof(CapNode)) { continue; }
				for (int i = startingDepth; i < currentNode.Depth; i++) {
					stringBuilder.Append('\t');
				}

				if (currentNode.GetType() == typeof(KeyCollectionNode))
				{
					KeyCollectionNode keyCollectionNode = (KeyCollectionNode)currentNode;
					List<Node> reverse = new List<Node>(keyCollectionNode.Children);
					reverse.Reverse();
					foreach (Node child in reverse) { nodeStack.Push(child); }
					if (!keyCollectionNode.IsRoot()) { stringBuilder.AppendLine(currentNode.GetString()); }
				}
				else
				{
					stringBuilder.AppendLine(currentNode.GetString());
				}


			}

			String built = stringBuilder.ToString();
			return built;
		}
	}
}
