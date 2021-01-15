using System;
using System.Collections.Generic;
using Scryber.OpenType;

namespace Scryber.Core.OpenType.Tests
{
    public static class WOFFFile
    {

        public static byte[] ExtractTTFfromWOFF(System.IO.Stream woff, int offset)
        {
            return null;
        }

        public static TTFRef[] GetRefsFromWOFF(string fullPath)
        {

            using(var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                return GetRefsFromWOFF(stream, fullPath);
            }
        }

        private static TTFRef[] GetRefsFromWOFF(System.IO.FileStream stream, string fullPath)
        {
            using(var reader = new BigEndianReader(stream))
            {
                return GetRefsFromWOFF(reader, fullPath);
            }
        }

        private static TTFRef[] GetRefsFromWOFF(BigEndianReader reader, string fullPath)
        {
            var woff = reader.ReadString(4);

            switch (woff)
            {
                case ("wOF2"):
                    return GetRefsFromWOFF2(reader, fullPath, woff);
                case ("wOFF"):
                    return GetRefsFromWOFF1(reader, fullPath, woff);
                default:
                    throw new TTFReadException("Could not understand the WOFF file format - expected 'wOFF' or 'wOF2' header. Instead found '" + woff + "'");
            }
        }

        private static TTFRef[] GetRefsFromWOFF1(BigEndianReader reader, string fullPath, string format)
        {
            throw new NotImplementedException();
        }

        private static TTFRef[] GetRefsFromWOFF2(BigEndianReader reader, string fullPath, string format)
        {
            WOFF2Header head;
            if(!WOFF2Header.TryReadHeader(reader, out head))
                return new TTFRef[] { };

            var uncompressed = ReadCompressedContent(reader, head);
            uncompressed.Position = 0;

            reader.BaseStream.Dispose();
            reader.Dispose();

            reader = new BigEndianReader(uncompressed);

            var directory = GetTableDirectory(head, reader);
            var factory = head.Version.GetTableFactory();

            var one = TTFRef.LoadARef(reader, fullPath, directory, factory);
            

            List<TTFRef> all = new List<TTFRef>();
            all.Add(one);

            return all.ToArray();


        }

        private static TTFDirectoryList GetTableDirectory(WOFF2Header head, BigEndianReader reader)
        {
            uint offset = 0;
            List<TTFDirectory> all = new List<TTFDirectory>();

            foreach (var item in head.Directory.AllEntries)
            {
                TTFDirectory dir = GetTable(item, offset, reader);

                if (null != dir)
                    all.Add(dir);
                offset += item.TableLength;
            }
            return new TTFDirectoryList(all);
        }

        

        private static TTFDirectory GetTable(WOFF2TableEntry item, uint currOffset, BigEndianReader reader)
        {
            TTFDirectory dir = new TTFDirectory(item.TypeName, 0, currOffset, item.TableLength);
            return dir;
        }

        private static System.IO.Stream ReadCompressedContent(BigEndianReader reader, WOFF2Header header)
        {
            var data = new byte[header.TablesSize];
            int total = reader.BaseStream.Read(data, 0, (int)header.TablesSize);

            if (total != header.TablesSize)
                throw new TTFReadException("Font header compressed size is not the same as the read content");

            var outputStream = new System.IO.MemoryStream();
            
            using (var ms = new System.IO.MemoryStream(data))
            {
                using (BrotliSharpLib.BrotliStream stream = new BrotliSharpLib.BrotliStream(ms, System.IO.Compression.CompressionMode.Decompress, true))
                {
                    int len = 2048;
                    byte[] output = new byte[len];

                    while (len >= 2048)
                    {
                        len = stream.Read(output, 0, len);
                        outputStream.Write(output, 0, len);
                    }

                    outputStream.Flush();
                }
            }

            return outputStream;

        }
    }
}
