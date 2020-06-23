using System;
using System.Linq;

namespace PdxScriptPlusPlus
{
    class Program
    {
        static void Main(string[] args)
        {
            String directory = String.Empty;

            Console.WriteLine("Enter directory to compile or leave blank to target this directory: ");
            directory = Console.ReadLine();

			var context = Script.ParsePDXScript.ParseFile(directory).Context;
			Console.WriteLine(context.Root.PrintString());

			foreach (var kcn in context.Root.Children.OfType<Script.KeyCollectionNode>())
			{
				if (kcn.Key.ToLower() == "function")
				{
					Function.PDXFunction function = Function.ParseFunction.ParseNode(kcn);
					function.DumpInfo(); Console.WriteLine("");
					Console.WriteLine(function.GenerateFunctionEffect());
				}
			}


            if (directory == String.Empty)
            {
                // This Directory
                Console.WriteLine("Directory: " + Environment.CurrentDirectory);
            }
            else
            {
                Console.WriteLine("Directory: " + directory);


            }

            Console.ReadLine();
        }
    }
}
