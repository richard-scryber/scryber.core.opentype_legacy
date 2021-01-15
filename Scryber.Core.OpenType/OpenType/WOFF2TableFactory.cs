using System;
using System.Collections.Generic;

namespace Scryber.OpenType
{
    public class WOFF2TableFactory : TTFOpenTypeTableFactory
    {
        private WOF2OpenTypeVersion _wofVersion;

        public WOFF2TableFactory(WOF2OpenTypeVersion vers, BigEndianReader reader, bool notfound)
            : base(vers, reader, notfound)
        {
            this._wofVersion = vers;
        }


        protected override TTFHeader ReadHeader(TTFVersion vers)
        {
            WOFF2Header head;
            if (WOFF2Header.TryReadHeader(this._wofVersion, this.Reader, out head))
                return head;
            else
                throw new TTFReadException("Could not read the WOFF2 header");

        }

        protected override TTFDirectoryList ReadDirectories()
        {
            WOFF2TableDirectory dir;
            if (!WOFF2TableDirectory.TryReadDirectory(this.Reader, this.Header.NumberOfTables, out dir))
                throw new TTFReadException("Cannot decompress the the tables");

            this.Reader = DecompressReader(this.Reader);

            var ttf = ConvertDirectories(dir, this.Header as WOFF2Header);

            return ttf;
        }

        private TTFDirectoryList ConvertDirectories(WOFF2TableDirectory dir, WOFF2Header head)
        {
            List<TTFDirectory> all = new List<TTFDirectory>();

            uint offset = 0;
            foreach (var d in dir.AllEntries)
            {
                TTFDirectory ttf;
                if (d.IsTransformed)
                    ttf = new TTFDirectory(d.TypeName, 0, offset, d.TransformedLength);
                else
                    ttf = new TTFDirectory(d.TypeName, 0, offset, d.TableLength);

                all.Add(ttf);

                offset += ttf.Length;

            }


            return new TTFDirectoryList(all);
        }

        private BigEndianReader DecompressReader(BigEndianReader reader)
        {
            var outputStream = new System.IO.MemoryStream();

            using (var decompress = new System.IO.Compression.BrotliStream(reader.BaseStream, System.IO.Compression.CompressionMode.Decompress, true))
            {
                int len = 2048;
                byte[] output = new byte[len];

                while (len >= 2048)
                {
                    len = decompress.Read(output, 0, len);
                    outputStream.Write(output, 0, len);
                }

                outputStream.Flush();
            }
            
            reader.BaseStream.Dispose();
            reader.Dispose();

            reader = new BigEndianReader(outputStream);
            return reader;
        }
    }
}
