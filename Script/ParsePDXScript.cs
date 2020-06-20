using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PdxScriptPlusPlus.Script
{
    static class ParsePDXScript
    {
		private static HashSet<Char> ValidOperators;

		static ParsePDXScript()
		{
			ValidOperators = new HashSet<char>() { '=', '<', '>' }; 
		}

		public static ParsePDXScriptResult ParseFile(String file)
		{
			ParsePDXScriptResult result = new ParsePDXScriptResult();
			// Open the file.
			StreamReader fileRead;
			try
			{
				fileRead = File.OpenText(file);
			}
			catch (Exception except)
			{
				result.Reason = except.Message;
				result.Success = false;
				result.Context = null;
				return result;
			}

			// Read the contents, close and dispose.
			String fileContents = fileRead.ReadToEnd();

			fileRead.Close();
			fileRead.Dispose();

			ParseContext parseContext;
			KeyCollectionNode root = new KeyCollectionNode(file, "root");
			parseContext = ParseContext.ConstructContext(root);
			result.Context = parseContext;

			Boolean UnixNewLine = (Environment.NewLine == "\n");

			HashSet<int> EscapedCharacters = new HashSet<int>();

			for (int currentPosition = 0; currentPosition < fileContents.Length; currentPosition++) {
				Char CurrentCharacter = fileContents[currentPosition];

				switch (parseContext.State)
				{
					case ParseContext.ContextState.Unknown:
						if (CurrentCharacter == '#')
						{
							parseContext.State = ParseContext.ContextState.CommentBuilding;
							parseContext.Comment += CurrentCharacter;
						}
						else if (CurrentCharacter == '}')
						{
							parseContext.MoveUpParent();
						}
						else if (Char.IsWhiteSpace(CurrentCharacter) == false)
						{
							parseContext.State = ParseContext.ContextState.KeyBuilding;
							parseContext.CurrentKey += CurrentCharacter;
						}
						break;
					case ParseContext.ContextState.KeyBuilding:
						if (CurrentCharacter == '#')
						{
							parseContext.CreateSingleNode(); // Move this current value into a single value line.
							parseContext.State = ParseContext.ContextState.CommentBuilding;
						}
						else if (ValidOperators.Contains(CurrentCharacter))
						{
							parseContext.State = ParseContext.ContextState.AfterOperator;
							parseContext.CurrentOperator = CurrentCharacter;

						}
						else if (CurrentCharacter == '}')
						{
							parseContext.CreateSingleNode();
							parseContext.MoveUpParent();
							parseContext.State = ParseContext.ContextState.Unknown;
						}
						else if (!Char.IsWhiteSpace(CurrentCharacter))
						{
							parseContext.CurrentKey += CurrentCharacter;
						}
						else
						{
							parseContext.State = ParseContext.ContextState.StateFinding;
						}
						break;
					case ParseContext.ContextState.StringBuilding:
						if (CurrentCharacter == '\\' && !EscapedCharacters.Contains(currentPosition))
						{
							if (currentPosition + 1 < fileContents.Length)
								EscapedCharacters.Add(currentPosition + 1);

							parseContext.CurrentValue += CurrentCharacter;
						}
						else if (CurrentCharacter == '\\' && EscapedCharacters.Contains(currentPosition))
						{
							parseContext.CurrentValue += CurrentCharacter;
						}
						else if (CurrentCharacter == '"' && !EscapedCharacters.Contains(currentPosition))
						{
							parseContext.CurrentValue += CurrentCharacter; // Finish the end of the String.
							parseContext.CreateKeyValueNode();
							parseContext.State = ParseContext.ContextState.Unknown;
							parseContext.BuildingNodeState = ParseContext.NodeState.Unknown;

						}
						else if (CurrentCharacter == '\n' || CurrentCharacter == '\r')
						{
							parseContext.CurrentValue += '"';
							parseContext.CreateKeyValueNode();
							parseContext.State = ParseContext.ContextState.Unknown;
							parseContext.BuildingNodeState = ParseContext.NodeState.Unknown;
						}
						else
						{
							parseContext.CurrentValue += CurrentCharacter;
						}
						break;
					case ParseContext.ContextState.ValueBuilding:
						if (CurrentCharacter == '#')
						{
							parseContext.CreateKeyValueNode();
							parseContext.State = ParseContext.ContextState.CommentBuilding;
							parseContext.BuildingNodeState = ParseContext.NodeState.Comment;
						}
						else if (Char.IsWhiteSpace(CurrentCharacter))
						{
							parseContext.CreateKeyValueNode();
							parseContext.State = ParseContext.ContextState.Unknown;

						}
						else if (CurrentCharacter == '}')
						{
							parseContext.CreateKeyValueNode();
							parseContext.State = ParseContext.ContextState.Unknown;
							parseContext.MoveUpParent();
						}
						else
						{
							parseContext.CurrentValue += CurrentCharacter;
						}
						break;
					case ParseContext.ContextState.StateFinding:
						if (CurrentCharacter == '#')
						{
							parseContext.CreateSingleNode();
							parseContext.State = ParseContext.ContextState.CommentBuilding;
							parseContext.BuildingNodeState = ParseContext.NodeState.Comment;
						}
						else if (CurrentCharacter == '}')
						{
							parseContext.CreateSingleNode();
							parseContext.MoveUpParent();
							parseContext.State = ParseContext.ContextState.Unknown;
						}
						else if (ValidOperators.Contains(CurrentCharacter))
						{
							parseContext.CurrentOperator = CurrentCharacter;
							parseContext.State = ParseContext.ContextState.AfterOperator;
						}
						else if (Char.IsWhiteSpace(CurrentCharacter) == false)
						{
							parseContext.CreateSingleNode();
							parseContext.State = ParseContext.ContextState.Unknown;
							parseContext.CurrentKey += CurrentCharacter;
						}
						break;
					case ParseContext.ContextState.AfterOperator:
						if (CurrentCharacter == '{')
						{
							parseContext.CreateKeyCollectionNode();
							parseContext.State = ParseContext.ContextState.Unknown;
						}
						else if (CurrentCharacter == '"')
						{
							parseContext.CurrentValue += CurrentCharacter;
							parseContext.State = ParseContext.ContextState.StringBuilding;
						}
						else if (CurrentCharacter == '#')
						{
							// Discard current node.
							parseContext.ForceDiscardNode();
							parseContext.State = ParseContext.ContextState.CommentBuilding;
						}
						else if (Char.IsWhiteSpace(CurrentCharacter) == false)
						{
							parseContext.CurrentValue += CurrentCharacter;
							parseContext.State = ParseContext.ContextState.ValueBuilding;
						}
						break;
					case ParseContext.ContextState.CommentBuilding:
						if (UnixNewLine)
						{
							if (CurrentCharacter == '\n')
							{
								parseContext.CreateCommentNode();
								parseContext.State = ParseContext.ContextState.Unknown;
							}
							else
							{
								parseContext.Comment += CurrentCharacter;
							}
						}
						else {
							int NextCharacterPosition = currentPosition + 1;

							if (NextCharacterPosition < fileContents.Length 
								&& CurrentCharacter == '\r'
								&& fileContents[NextCharacterPosition] == '\n')
							{
								parseContext.CreateCommentNode();
								parseContext.State = ParseContext.ContextState.Unknown;
								currentPosition = NextCharacterPosition;
							}
							else
							{
								parseContext.Comment += CurrentCharacter;
							}
						}
						break;
				}


			}

			// Ensuring we do not drop the last node.
			switch (parseContext.State)
			{
				case ParseContext.ContextState.AfterOperator:
					parseContext.CreateKeyValueNode();
					break;
				case ParseContext.ContextState.Unknown: // Literally do nothing.
					break;
				case ParseContext.ContextState.CommentBuilding:
					parseContext.CreateCommentNode();
					break;
				case ParseContext.ContextState.StateFinding:
					parseContext.CreateSingleNode();
					break;
				case ParseContext.ContextState.StringBuilding:
					parseContext.CurrentValue += '"'; // Close the String.
					parseContext.CreateKeyValueNode();
					break;
				case ParseContext.ContextState.ValueBuilding:
					parseContext.CreateKeyValueNode();
					break;
				case ParseContext.ContextState.KeyBuilding:
					parseContext.CreateSingleNode();
					break;
			}



			return result;
		}
	}

	struct ParsePDXScriptResult
	{
		public Boolean Success { get; set; }
		public ParseContext Context { get; set; }
		public String Reason { get; set; }
	}

}
