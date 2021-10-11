using System;
using System.Runtime.CompilerServices;

namespace Scryber.Core.OpenType.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var tests = new[]
            {
                new { Name = "Hachi", Path = "https://fonts.gstatic.com/s/hachimarupop/v2/HI_TiYoRLqpLrEiMAuO9Ysfz7rW1.ttf"},
                new { Name = "Roboto", Path = "https://fonts.gstatic.com/s/roboto/v20/KFOmCnqEu92Fr1Me5Q.ttf"},
                new { Name = "Helvetica", Path = "https://raw.githubusercontent.com/richard-scryber/scryber.core/svgParsing/Scryber.Drawing/Text/_FontResources/Helvetica/Helvetica.ttf"},
                new { Name = "Noto TC (not working)", Path = "https://fonts.gstatic.com/s/notosanstc/v20/-nF7OG829Oofr2wohFbTp9iFOQ.otf"},
            };

            foreach (var item in tests)
            {
                Console.WriteLine("Measuring the strings for " + item.Name);

                try
                {
                    var path = item.Path;
                    MeasureStringFor(path);
                }
                catch (Exception ex)
                {
                    ExitClean("Could not measure : " + item.Name + ":" + ex.Message);
                }
            }

        }

        private static void MeasureStringFor(string path)
        {
            byte[] data = null;

            if (System.Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                using (var wc = new System.Net.WebClient())
                    data = wc.DownloadData(path);
            }
            else if (System.IO.File.Exists(path))
            {
                data = System.IO.File.ReadAllBytes(path);
            }
            else
            {
                ExitClean("Font could not be found at path " + path);
                return;
            }

            using (var ms = new System.IO.MemoryStream(data))
            {
                using (var reader = new Scryber.OpenType.BigEndianReader(ms))
                {
                    var fontRef = Scryber.OpenType.TTFRef.LoadRef(reader, path);
                    if (null == fontRef || fontRef.IsValid == false)
                    {
                        ExitClean("Could not load the font reference information for the file " + path);
                        return;
                    }

                    Console.WriteLine("Loaded font reference " + fontRef.FamilyName);
                }
            }


            var ttf = new Scryber.OpenType.TTFFile(data, 0);

            Console.WriteLine("Loaded font file " + ttf.ToString());

            var encoding = Scryber.OpenType.SubTables.CMapEncoding.WindowsUnicode;
            string text = "This is the text to measure";
            int fitted;
            var size = ttf.MeasureString(encoding, text, 0, 12, 10000, true, out fitted);

            Console.WriteLine("String Measured to " + size.ToString() + " and fitted " + fitted + " characters out of " + text.Length);



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
