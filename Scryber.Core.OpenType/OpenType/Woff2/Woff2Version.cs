using System;
namespace Scryber.OpenType.Woff2
{
    public class Woff2Version : TypefaceVersion
    {
        public Woff2Version(string id, byte[] header)
            : base(header, DataFormat.Woff2)
        {
        }

        public override TTFTableFactory GetTableFactory()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Woff 2 Format";
        }
    }
}
