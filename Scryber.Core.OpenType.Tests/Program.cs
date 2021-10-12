#define UseOpenFont

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Scryber.OpenType;
using Scryber.OpenType.SubTables;
using Typography.OpenFont.Extensions;
using Typography.OpenFont.WebFont;

namespace Scryber.Core.OpenType.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var fonts = new[]
            {
                new { Name = "Hachi", Include = false, Path = "https://fonts.gstatic.com/s/hachimarupop/v2/HI_TiYoRLqpLrEiMAuO9Ysfz7rW1.ttf"},
                new { Name = "Roboto", Include = false, Path = "https://fonts.gstatic.com/s/roboto/v20/KFOmCnqEu92Fr1Me5Q.ttf"},
                new { Name = "Helvetica", Include = false, Path = "https://raw.githubusercontent.com/richard-scryber/scryber.core/svgParsing/Scryber.Drawing/Text/_FontResources/Helvetica/Helvetica.ttf"},
                new { Name = "Noto TC", Include = false, Path = "https://fonts.gstatic.com/s/notosanstc/v20/-nF7OG829Oofr2wohFbTp9iFOQ.otf"},
                new { Name = "Festive", Include = true, Path = "https://fonts.gstatic.com/s/festive/v1/cY9Ffj6KX1xcoDWhJt_qyvPQgah_Lw.woff2"}
            };

            FontDownload loader = new FontDownload();

            foreach (var item in fonts)
            {

                try
                {
                    var path = item.Path;

                    if (item.Include)
                    {
                        Console.WriteLine("Loading the font for " + item.Name + " from " + path);
                        MeasureStringFor(loader, path).Wait();
                    }
                    else
                        Console.WriteLine("Skipped " + item.Name);
                }
                catch (Exception ex)
                {
                    ExitClean("Could not measure : " + item.Name + ":" + ex.Message);
                }
            }

            loader.Dispose();

        }

        private async static Task MeasureStringFor(FontDownload loader, string path)
        {
            byte[] data = null;

            data = await loader.DownloadFrom(path);
            

#if UseOpenFont

            ZlibDecompressStreamFunc zipfunc = (byte[] dataIn, byte[] output) =>
            {
                using (var streamIn = new MemoryStream(dataIn))
                {
                    System.IO.Compression.DeflateStream deflate = new System.IO.Compression.DeflateStream(streamIn, System.IO.Compression.CompressionMode.Decompress);

                    using (var streamOut = new MemoryStream(output))
                        deflate.CopyTo(streamOut);

                    return true;
                }

            };
            WoffDefaultZlibDecompressFunc.DecompressHandler = zipfunc;

            BrotliDecompressStreamFunc brotlifunc = (byte[] dataIn, Stream dataOut) =>
            {
                using (var streamIn = new MemoryStream(dataIn))
                {
                    System.IO.Compression.BrotliStream deflate = new System.IO.Compression.BrotliStream(streamIn, System.IO.Compression.CompressionMode.Decompress);

                    deflate.CopyTo(dataOut);

                    return true;
                }
            };
            Woff2DefaultBrotliDecompressFunc.DecompressHandler = brotlifunc;


            using (var ms = new System.IO.MemoryStream(data))
            {
                var reader = new Typography.OpenFont.OpenFontReader();
                var preview = reader.ReadPreview(ms);

                Console.WriteLine("Loaded font reference " + preview.Name);
            }

            using (var ms = new System.IO.MemoryStream(data))
            {
                var reader = new Typography.OpenFont.OpenFontReader();
                var full = reader.Read(ms);
                string text = "This is the text to measure";
                var encoding = Scryber.OpenType.SubTables.CMapEncoding.WindowsUnicode;
                int fitted;
                var size = full.MeasureString(text, 0, 12, 10000, true, out fitted);


                Console.WriteLine("String Measured to " + size.ToString() + " and fitted " + fitted + " characters out of " + text.Length);

            }

#else



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


            var stopWatch = Stopwatch.StartNew();

            MeasureStringMeasurer(ttf);

            stopWatch.Stop();

            Console.WriteLine("To measure 4 different strings " + maxRepeat + " times took " + stopWatch.Elapsed.TotalMilliseconds + "ms");
#endif

        }

        const int maxRepeat = 100000;

        static void MeasureStringsUsual(TTFFile file)
        {
            


            for(var repeat = 0; repeat < maxRepeat; repeat++)
            {
                for (var index = 0; index < AllToMeasure.Length; index++)
                {
                    var str = AllToMeasure[index];
                    int fitted;
                    var encoding = CMapEncoding.WindowsUnicode;
                    var size = file.MeasureString(encoding, str, 0, 12.0, 5061, false, out fitted);


                    if (fitted != AllFitted[index])
                        throw new InvalidOperationException("The measured number of characters " + fitted + " was not the same as the expected result " + AllFitted[index] + " for string " + index);
                }

            }

        }

        static void MeasureStringMeasurer(TTFFile file)
        {
            var measurer = TTFStringMeasurer.Create(file, CMapEncoding.WindowsUnicode, 0, 0);

            for (var repeat = 0; repeat < maxRepeat; repeat++)
            {
                for (var index = 0; index < AllToMeasure.Length; index++)
                {
                    var str = AllToMeasure[index];
                    int fitted;
                    var size = measurer.MeasureChars(str, 0, 12.0, 5061, false, out fitted); 

                    if (fitted != AllFitted[index])
                        throw new InvalidOperationException("The measured number of characters " + fitted + " was not the same as the expected result " + AllFitted[index] + " for string " + index);
                }

            }



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

        static int[] AllFitted = new int[]
        {
            12,
            56,
            167,
            273,
            584
        };

        static string[] AllToMeasure = new string[]
        {
            "First String",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas scelerisque porttitor urna. Duis pellentesque sem tempus magna faucibus, quis lobortis magna aliquam.",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas scelerisque porttitor urna. Duis pellentesque sem tempus magna faucibus, quis lobortis magna aliquam. Nullam eu risus facilisis sapien fermentum condimentum. Pellentesque ut placerat diam, sed suscipit nibh.",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas scelerisque porttitor urna. Duis pellentesque sem tempus magna faucibus, quis lobortis magna aliquam. Nullam eu risus facilisis sapien fermentum condimentum. Pellentesque ut placerat diam, sed suscipit nibh. Integer dictum dolor vel finibus imperdiet. Orci varius natoque penatibus et magnis disparturient montes, nascetur ridiculus mus. Integer congue turpis at varius porttitor. nec faucibus ipsum bibendum sed. Nunc tristique risus eu quam porttitor blandit. In erat mauris, imperdiet a venenatis eu, tempus a nunc."
        };
    }
}
