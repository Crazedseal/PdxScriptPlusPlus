using System;

namespace PdxScriptPlusPlus
{
    class Program
    {
        static void Main(string[] args)
        {
            String directory = String.Empty;

            Console.WriteLine("Enter directory to compile or leave blank to target this directory: ");
            directory = Console.ReadLine();

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
