using System;
namespace Scryber.OpenType
{
    public class WOFF2Header
    {
        public TTFVersion Version { get; set; }

        public long TotalSize { get; set; }

        public long TablesOffset { get; set; }

        public long TablesSize
        {
            get { return TotalSize - TablesOffset; }
        }

        public int TableNumber { get; set; }

        public long UncompressedSize { get; set; }

        public Version WoffVersion { get; set; }

        public long MetaOffset { get; set; }

        public long MetaLength { get; set; }

        public long PrivateOffset { get; set; }

        public long PrivateLength { get; set; }

        public WOFF2TableDirectory Directory { get; set; }

        public bool IsCollection
        {
            get { return Version.IsCollection; }
        }

        public WOFF2Header()
        {
        }


        public static bool TryReadHeader(BigEndianReader reader, out WOFF2Header header)
        {
            header = null;
            TTFVersion vers;

            if (TTFVersion.TryGetVersion(reader, out vers) == false)
                return false;

            var sz = reader.ReadUInt32();
            var tnum = reader.ReadUInt16();
            var res = reader.ReadUInt16();

            var uncompsz = reader.ReadUInt32();
            var compsz = reader.ReadUInt32();
            var majorV = reader.ReadUInt16();
            var minorV = reader.ReadUInt16();

            var metaOff = reader.ReadUInt32();
            var metaLen = reader.ReadUInt32();
            var metaOrigLen = reader.ReadUInt32();
            var privOff = reader.ReadUInt32();
            var privLen = reader.ReadUInt32();

            WOFF2TableDirectory dir;

            if (vers.IsCollection)
            {
                WOFF2CollectionDirectory col;
                if (!WOFF2CollectionDirectory.TryReadCollectionDirectory(reader, (int)tnum, out col))
                    return false;
                dir = col;
            }
            else if (!WOFF2TableDirectory.TryReadDirectory(reader, (int)tnum, out dir))
                return false;

            header = new WOFF2Header()
            {
                Directory = dir,
                Version = vers,
                TablesOffset = reader.Position,
                TotalSize = sz,
                TableNumber = tnum,
                UncompressedSize = uncompsz,
                WoffVersion = new Version((int)majorV, (int)minorV),
                MetaOffset = metaOff,
                MetaLength = metaLen,
                PrivateOffset = privOff,
                PrivateLength = privLen
            };

            return true;
        }
    }
}
