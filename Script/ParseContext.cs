﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PdxScriptPlusPlus.Script
{
	class ParseContext
	{
		public KeyCollectionNode Root { get; private set; }
		public String File { get; set; }

		public String CurrentKey { get; set; }
		public String CurrentValue { get; set; }
		public Char CurrentOperator { get; set; }
		public ContextState State { get; set; }
		public NodeState BuildingNodeState { get; set; }
		public Int32 BraceCount { get; set; }
		public String Comment { get; internal set; }

		public KeyCollectionNode CurrentParent;

		public static ParseContext ConstructContext(KeyCollectionNode root)
		{
			ParseContext construct = new ParseContext();
			if (root.SetContext(construct))
			{
				construct.Root = root;
				construct.CurrentParent = construct.Root;
				return construct;
			}
			else { return null; }
		}

		private ParseContext()
		{
			BraceCount = 0;
			CurrentKey = "";
			CurrentValue = "";
		}

		public Boolean IsRootNode(KeyCollectionNode root)
		{
			return this.Root == root;
		}

		private void ResetNodeConstruction()
		{
			this.CurrentKey = "";
			this.CurrentValue = "";
			this.Comment = "";
			this.BuildingNodeState = NodeState.Unknown;
		}

		public void MoveUpParent()
		{
			if (this.CurrentParent == this.Root) { }
			else
			{
				this.CurrentParent = this.CurrentParent.Parent; // Let us move up.
			}
		}

		public void Finalize()
		{
			ResetNodeConstruction();

			Stack<Node> nodes = new Stack<Node>();
			nodes.Push(this.Root);

			while (nodes.Count > 0)
			{
				Node currentNode = nodes.Pop();
				if (currentNode.GetType() == typeof(KeyCollectionNode))
				{
					KeyCollectionNode collectionNode = (KeyCollectionNode)currentNode;
					foreach (Node child in collectionNode.Children) {
						if (!collectionNode.IsRoot()) {
							child.Depth = (byte)(collectionNode.Depth + 1);
						}
						nodes.Push(child);
					}

				}
			}

			this.Root.Children.Add(new CapNode(this.File));
		}

		public void ForceDiscardNode()
		{
			this.CurrentKey = "";
			this.CurrentValue = "";
			this.Comment = "";
		}

		public SingleValueNode CreateSingleNode( )
		{
			SingleValueNode construct = new SingleValueNode(this.File);
			construct.SetParent(this.CurrentParent);
			construct.Value = this.CurrentKey;
			this.ResetNodeConstruction();
			return construct;
		}

		public CommentNode CreateCommentNode()
		{
			CommentNode construct = new CommentNode(this.File, this.Comment);
			construct.SetParent(this.CurrentParent);
			this.ResetNodeConstruction();
			return construct;
		}

		public KeyValueNode CreateKeyValueNode()
		{
			KeyValueNode construct = new KeyValueNode(this.File);
			construct.SetParent(this.CurrentParent);
			construct.Key = this.CurrentKey;
			construct.Value = this.CurrentValue;
			construct.Operator = this.CurrentOperator;
			this.ResetNodeConstruction();
			return construct;
		}

		public KeyCollectionNode CreateKeyCollectionNode()
		{
			KeyCollectionNode construct = new KeyCollectionNode(File, this.CurrentKey);
			construct.SetParent(this.CurrentParent);
			this.ResetNodeConstruction();
			this.CurrentParent = construct;
			return construct;
		}

		public enum ContextState { Unknown, KeyBuilding, StringBuilding, ValueBuilding, StateFinding, CommentBuilding, AfterOperator }
		public enum NodeState { Unknown, KeyValue, KeyCollection, Comment, SingleValue }
	}

	
}
