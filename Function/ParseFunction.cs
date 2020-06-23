using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PdxScriptPlusPlus.Function
{
    class ParseFunction
    {
		/************************************************************
         * function = {
         *      name = cube_root
         *      arguments = { cr_arg_number }
         *      return = cr_ret_number
         *      effect = {
         *          ...
         *          return = mid
         *      }
         * }
         ************************************************************/


		public static PDXFunction ParseNode(Script.KeyCollectionNode head)
		{
			PDXFunction construct = new PDXFunction();
			HashSet<String> foundArguments = new HashSet<string>();

			if (head.Key.Trim().ToLower() != "function")
			{
				// Log this.
				return null;
			}

			Script.KeyCollectionNode argNode = null;
			Script.KeyCollectionNode effectNode = null;


			foreach (Script.Node child in head.Children)
			{
				switch (child)
				{
					case Script.KeyCollectionNode kcn:
						String kcn_key = kcn.Key.ToLower();
						switch (kcn_key)
						{
							case "arguments":
								argNode = kcn;
								break;
							case "effect":
								effectNode = kcn;
								break;
						}

						break;
					case Script.CommentNode cn:
						break;
					case Script.KeyValueNode kvn:
						String kvn_key = kvn.Key.ToLower();
						switch (kvn_key)
						{
							case "name":
								if (construct.Name == String.Empty)
								{
									construct.Name = kvn.Value;
								}
								else
								{
									// Log.
								}
								break;
							case "return":
								if (construct.ReturnVariable == String.Empty)
								{
									construct.ReturnVariable = kvn.Value;
								}
								else
								{
									// Log
								}
								break;
							case "arguments":
								// Maybe log this.
								break;
						}
						break;
					case Script.SingleValueNode svn:
						break;

				}
			}

			if (argNode != null)
			{
				foreach (Script.SingleValueNode singleValueNode in argNode.Children.OfType<Script.SingleValueNode>())
				{
					String svn_value_lower = singleValueNode.Value.Trim().ToLower();
					if (foundArguments.Contains(svn_value_lower)) { continue; }
					else { foundArguments.Add(svn_value_lower); construct.Arguments.Add(svn_value_lower); }
				}

				for (int i = 0; i < construct.Arguments.Count; i++)
				{
					construct.ArgumentIndex[construct.Arguments[i]] = i;
				}
			}

			if (effectNode != null)
			{
				construct.EffectsNode = effectNode;
				construct.LocalVariables = FindLocals(effectNode);
				for (int i = 0; i < construct.LocalVariables.Count; i++)
				{
					construct.LocalVariableIndex[construct.LocalVariables[i]] = i;
				}


			}

			return construct;
		}

		private static HashSet<String> LocalVariableKeyWords = new HashSet<string>() { "check_local_variable", "set_local_variable", "add_to_local_variable", "divide_local_variable", "multiply_local_variable" };
		private static HashSet<String> VariableExpressions = new HashSet<string>() { "var", "value", "compare", "min", "max" };

		private static List<String> FindLocals(Script.KeyCollectionNode effectsNode, HashSet<String> foundArgs = null)
		{
			HashSet<String> found_local_variables_set = new HashSet<string>();
			List<String> local_variables = new List<string>();

			Stack<Script.Node> nodeStack = new Stack<Script.Node>();

			nodeStack.Push(effectsNode);

			Script.Node previousNode = null;
			Script.Node currentNode = null;
			Boolean lookingForVariableName = false;

			while (nodeStack.Count > 0)
			{
				previousNode = currentNode;
				currentNode = nodeStack.Pop();

				// If we jump down from an expression and are looking for a variable expression. Stop looking.
				if (previousNode != null && lookingForVariableName && previousNode.Depth > currentNode.Depth)
				{
					lookingForVariableName = false;
				}

				// If looking for a variable name, check if the current node is a Key Value Node.
				if (lookingForVariableName && currentNode.GetType() == typeof(Script.KeyValueNode))
				{
					Script.KeyValueNode KVN = (Script.KeyValueNode)currentNode;
					String lower_key = KVN.Key.ToLower();
					String lower_value = KVN.Value.ToLower();
					if (lower_value.StartsWith("local."))
					{
						lower_value = lower_value.Remove(0, 6);
					}
					else if (lower_value.StartsWith("var:local.")) {
						lower_value = lower_value.Remove(0, 10);
					}


					if (lower_key == "var" && !found_local_variables_set.Contains(lower_value) && (foundArgs == null || !foundArgs.Contains(lower_value)))
					{
						found_local_variables_set.Add(lower_value);
						local_variables.Add(lower_value);
						continue;
					}
					else if (VariableExpressions.Contains(lower_key) || found_local_variables_set.Contains(lower_key)) { continue; }
					else
					{
						if (foundArgs == null || !foundArgs.Contains(lower_value))
						{
							found_local_variables_set.Add(KVN.Key.ToLower());
							local_variables.Add(KVN.Key.ToLower());
						}
						continue;
					}
				}
				else if (currentNode.GetType() == typeof(Script.KeyCollectionNode))
				{
					Script.KeyCollectionNode KCN = (Script.KeyCollectionNode)currentNode;
					if (LocalVariableKeyWords.Contains(KCN.Key.ToLower()))
					{
						lookingForVariableName = true;
					}

					var ReverseChildren = new List<Script.Node>(KCN.Children);
					ReverseChildren.Reverse();
					foreach (Script.Node child in ReverseChildren) { nodeStack.Push(child); }
				}
			}



			return local_variables;
		}



	}
}
