using System;

namespace Scryber.Core.OpenType.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0 || args[0].Length == 0)
            {
                ExitClean("Please provide the file path as an argument to execution");
                return;
            }

            var path = args[0];

            if (!System.IO.File.Exists(path))
            {
                ExitClean("File path '" + path + "' could not be found");

                return;
            }

            var fontRef = Scryber.OpenType.TTFRef.LoadRef(path);
            if(null == fontRef || fontRef.IsValid == false)
            {
                ExitClean("Could not load the font reference information for the file " + path);
                return;
            }

            Console.WriteLine("Loaded font reference " + fontRef.FamilyName);

            

        }

        static void ExitClean(string message, bool error = true)
        {
            if (error)
                Console.WriteLine("AN ERROR OCCURRED");
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine(message);

            Console.WriteLine();
            Console.Write("Press any key to exit...");
            Console.Read();
        }
    }
}
