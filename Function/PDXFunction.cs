using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PdxScriptPlusPlus.Function
{
	class PDXFunction
	{
		/// <summary>
		/// Name of the function. From KeyValueNode [name] = [value]
		/// </summary>
		public String Name { get; set; }


		public List<String> Arguments { internal get; set; }
		public Dictionary<String, int> ArgumentIndex = new Dictionary<string, int>();
		public IReadOnlyList<String> GetArguments() { return this.Arguments.AsReadOnly(); }
		public Boolean HasArguments { get { return Arguments.Count > 0; } }
		public int LocalCount { get { return LocalVariables.Count; } }
		public List<String> LocalVariables { internal get; set; }
		public Dictionary<String, int> LocalVariableIndex = new Dictionary<string, int>();
		public IReadOnlyList<String> GetLocalVariables() { return this.LocalVariables.AsReadOnly(); }
		public String ReturnVariable { get; set; }
		public String RawText { get; set; }
		public Script.KeyCollectionNode EffectsNode { get; set; }
		public Script.KeyCollectionNode ParsedEffectsNode { get; set; }
		public Script.KeyCollectionNode HeadNode { get; set; }

		public PDXFunction() {
			this.Name = String.Empty;
			this.ReturnVariable = String.Empty;
			this.Arguments = new List<string>();
			this.LocalVariables = new List<string>();

		}

		public void DumpInfo()
		{
			Console.WriteLine("Name: {0}", this.Name);
			Console.Write("Arguments: ");
			foreach (var arg in Arguments)
			{
				Console.Write(arg + " ");
			}
			Console.WriteLine("");
			Console.Write("Locals: ");
			foreach (var loc in LocalVariables)
			{
				Console.Write(loc + " ");
			}
			

		}

		public String GenerateFunctionVariables()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("global.func_{0}_loc_count = {1}\n", this.Name, this.LocalCount);
			stringBuilder.AppendFormat("global.func_{0}_arg_count {1}\n", this.Name, this.Arguments.Count);
			return stringBuilder.ToString();
		}

		public String GenerateFunctionStack()
		{
			StringBuilder stringBuilder = new StringBuilder(1100 + (this.Name.Length*16));
			// PUSH
			stringBuilder.AppendFormat("global.func_{0}_push_stack = {\n", this.Name);
			stringBuilder.AppendFormat("\tadd_to_variable = { global.func_{0}_stack_frame = 1 }\n", this.Name);
			stringBuilder.AppendFormat("\tfunc_{0}_resize_arg = yes\n}\n", this.Name);

			// POP
			stringBuilder.AppendFormat("global.func_{0}_pop_stack = {\n", this.Name);
			stringBuilder.AppendFormat("\tadd_to_variable = { global.func_{0}_stack_frame = -1 }\n", this.Name);
			stringBuilder.AppendFormat("\tfunc_{0}_resize_arg = yes\n}\n", this.Name);

			// RESIZE
			stringBuilder.AppendFormat("global.func_{0}_resize_arg = {\n", this.Name);
			stringBuilder.AppendFormat("\tset_variable = { global.func_{0}_arg_resize = global.func_{0}_stack_frame }\n", this.Name);
			stringBuilder.AppendFormat("\tset_variable = { global.func_{0}_loc_resize = global.func_{0}_stack_frame }\n", this.Name);
			stringBuilder.AppendFormat("\tmultiply_variable = { global.func_{0}_loc_resize = global.func_{0}_loc_count }\n", this.Name);
			stringBuilder.AppendFormat("\tmultiply_variable = { global.func_{0}_arg_resize = global.func_{0}_arg_count }\n", this.Name);
			stringBuilder.AppendFormat("\tresize_array = { global.func_{0}_args = global.func_{0}_arg_resize }\n", this.Name);
			stringBuilder.AppendFormat("\tresize_array = { global.func_{0}_locals = global.func_{0}_loc_resize }\n", this.Name);
			stringBuilder.AppendFormat("\tclear_variable = global.func_{0}_arg_resize\n", this.Name);
			stringBuilder.AppendFormat("\tclear_variable = global.func_{0}_loc_resize\n}", this.Name);

			return stringBuilder.ToString();
		}


		public String GenerateFunctionEffect()
		{
			// Convert local variable declarations to stack local array, and arg variables to stack arg array.
			StringBuilder stringBuilder = new StringBuilder();

			Stack<Script.Node> nodeStack = new Stack<Script.Node>();
			Script.KeyCollectionNode parseNodeEffects = (Script.KeyCollectionNode)EffectsNode.Clone();
			nodeStack.Push(parseNodeEffects);

			Script.Node currentNode = null;

			Dictionary<string, string> match_replace = new Dictionary<string, string>();
			for (int i = 0; i != this.LocalVariables.Count; i++)
			{
				match_replace[this.LocalVariables[i]] = String.Format("global.func_{0}_locals^{1}", this.Name, i);
			}


			while (nodeStack.Count > 0)
			{
				currentNode = nodeStack.Pop();

				switch (currentNode)
				{
					case Script.KeyCollectionNode kcn:
						String kcn_key_lower = kcn.Key.ToLower();

						if (kcn.Key == "set_local_variable") { kcn.Key = "set_variable"; }

						if (kcn_key_lower.StartsWith("local."))
						{
							kcn_key_lower = kcn_key_lower.Remove(0, 6);
							int local_index = this.LocalVariableIndex[kcn_key_lower];
							kcn.Key = String.Format("global.func_{0}_locals^{1}", this.Name, local_index);
						}
						else if (kcn_key_lower.StartsWith("var:local."))
						{
							kcn_key_lower = kcn_key_lower.Remove(0, 10);
							int local_index = this.LocalVariableIndex[kcn_key_lower];
							kcn.Key = String.Format("global.func_{0}_locals^{1}", this.Name, local_index);
						}



						foreach (var child in kcn.Children) { nodeStack.Push(child); }


						break;
					case Script.KeyValueNode kvn:
						String kvn_key_lower = kvn.Key.ToLower();
						String kvn_value_lower = kvn.Value.ToLower();

						

						if (kvn.HasStringValue())
						{
							kvn_value_lower = Regex.Replace(kvn_value_lower, "(?<=\\[\\?local\\.)\\S*(?=\\])", m => match_replace[m.Value]);
							kvn_value_lower = Regex.Replace(kvn_value_lower, "(?<=\\[\\?)\\local(?=\\])", "");
							kvn.Value = kvn_value_lower;
						}
						else
						{
							if (kvn.Value.StartsWith("local."))
							{
								kvn_value_lower = kvn_value_lower.Remove(0, 6);
								int local_index = this.LocalVariableIndex[kvn_value_lower];
								kvn.Value = String.Format("global.func_{0}_locals^{1}", this.Name, local_index);
							}
							else if (kvn.Value.StartsWith("var:local."))
							{
								kvn_value_lower = kvn_value_lower.Remove(0, 10);
								int local_index = this.LocalVariableIndex[kvn_value_lower];
								kvn.Value = String.Format("var:global.func_{0}_locals^{1}", this.Name, local_index);
							}
							else if (this.LocalVariableIndex.ContainsKey(kvn_value_lower)) {
								int local_index = this.LocalVariableIndex[kvn_value_lower];
								kvn.Value = String.Format("global.func_{0}_locals^{1}", this.Name, local_index);
							}

							if (kvn.Key.StartsWith("local."))
							{
								kvn_key_lower = kvn_key_lower.Remove(0, 6);
								int local_index = this.LocalVariableIndex[kvn_key_lower];
								kvn.Key = String.Format("global.func_{0}_locals^{1}", this.Name, local_index);
							}
							else if (kvn.Key.StartsWith("var:local."))
							{
								kvn_key_lower = kvn_key_lower.Remove(0, 10);
								int local_index = this.LocalVariableIndex[kvn_key_lower];
								kvn.Key = String.Format("var:global.func_{0}_locals^{1}", this.Name, local_index);
							}
							else if (this.LocalVariableIndex.ContainsKey(kvn_key_lower)) {
								int local_index = this.LocalVariableIndex[kvn_key_lower];
								kvn.Key = String.Format("global.func_{0}_locals^{1}", this.Name, local_index);
							}

						}
						break;
				}



			}


			return parseNodeEffects.PrintString();
		}

	}
}
