using System;
namespace Scryber.OpenType.Woff
{
    public class WoffVersion : TypefaceVersion
    {
        
        public WoffVersion(string id, byte[] header)
            : base(header, DataFormat.Woff)
        {
        }

        public override TTFTableFactory GetTableFactory()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Woff Format";
        }
    }
}
