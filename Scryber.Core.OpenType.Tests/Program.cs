using System;
using System.IO;
using System.IO.Compression;
using Scryber.OpenType;

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
            var wd = System.AppContext.BaseDirectory;

            if (!System.IO.Path.IsPathRooted(path))
                path = System.IO.Path.Combine(wd, path);

            if (!System.IO.File.Exists(path))
            {
                ExitClean("File path '" + path + "' could not be found");

                return;
            }

            var typeface = ReadWOFFFromTypography(path);



            var fontRef = Scryber.OpenType.TTFRef.LoadRef(path);
            if(null == fontRef)
            {
                ExitClean("Could not load the font reference information for the file " + path);
                return;
            }

            var font = fontRef;

            /* /Users/richardhewitson/Projects/Scryber.Core/Scryber.Core.OpenType/Scryber.Core.OpenType.Tests/Samples */
            Console.WriteLine("Loaded font reference " + fontRef.FamilyName);

            

        }

        static Typography.OpenFont.Typeface ReadWOFFFromTypography(string path)
        {
            Typography.OpenFont.WebFont.Woff2DefaultBrotliDecompressFunc.DecompressHandler = new Typography.OpenFont.WebFont.BrotliDecompressStreamFunc(DecompressWithBrotli);

            using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var reader = new Typography.OpenFont.OpenFontReader();
                var typeface = reader.Read(stream);

                return typeface;
            }
        }

        static bool DecompressWithBrotli(byte[] input, Stream outputStream)
        {
            using (var ms = new MemoryStream(input))
            {
                BrotliSharpLib.BrotliStream stream = new BrotliSharpLib.BrotliStream(ms, CompressionMode.Decompress, true);
                int len = 2048;
                byte[] output = new byte[len];

                while (len >= 2048)
                {
                    len = stream.Read(output, 0, len);
                    outputStream.Write(output, 0, len);
                }

                outputStream.Flush();
            }

            return true;
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
