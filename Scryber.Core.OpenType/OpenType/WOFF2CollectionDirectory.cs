using System;
namespace Scryber.OpenType
{
    public class WOFF2CollectionDirectory : WOFF2TableDirectory
    {
        public WOFF2CollectionDirectory()
        {
        }


        public static bool TryReadCollectionDirectory(BigEndianReader reader, int numTables, out WOFF2CollectionDirectory col)
        {
            throw new NotImplementedException("WOFF2 Collections are not supported at the moment");
        }

    }
}
