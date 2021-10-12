using System;
using System.IO;

namespace Scryber.OpenType
{
    public class TypefaceReader
    {
        public TypefaceReader()
        {
        }

        public ITypefaceInfo GetInfo(Stream stream, string source)
        {
            using(var reader = new BigEndianReader(stream))
            {
                TypefaceVersion version;
                if (!TryGetVersion(reader, out version))
                    return new Utility.UnknownTypeface(source, "Could not identify the version of the font source");

                else
                {
                    var factory = version.GetTableFactory();
                    return factory.ReadTypefaceInfoAfterVersion(reader, source);
                }
            }
        }

        public ITypeface GetTypeface(Stream stream, ITypefaceReference theReference)
        {
            return null;
        }


        public bool TryGetVersion(BigEndianReader reader, out TypefaceVersion vers)
        {
            vers = null;
            byte[] data = reader.Read(4);
            char[] chars = ConvertToChars(data, 4);

            if (chars[0] == 'O' && chars[1] == 'T' && chars[2] == 'T' && chars[3] == 'O')
            {
                vers = new CCFOpenTypeVersion(new string(chars), data);
            }
            else if (chars[0] == 't' && chars[1] == 'r' && chars[2] == 'u' && chars[3] == 'e')
                vers = new TTFTrueTypeVersion(new string(chars), data);

            else if (chars[0] == 't' && chars[1] == 'y' && chars[2] == 'p' && chars[3] == '1')
                vers = new TTFTrueTypeVersion(new string(chars), data);
            else if (chars[0] == 't' && chars[1] == 't' && chars[2] == 'c' && chars[3] == 'f')
                vers = new TTFCollectionVersion(new string(chars), data);
            else if (chars[0] == 'w' && chars[1] == 'O' && chars[2] == 'F' && chars[3] == 'F')
                vers = new Woff.WoffVersion(new string(chars), data);
            else if(chars[0] == 'w' && chars[1] == 'O' && chars[2] == 'F' && chars[3] =='2' )
                vers = new Woff2.Woff2Version(new string(chars), data);

            else
            {
                BigEnd16 wrd1 = new BigEnd16(data, 0);
                BigEnd16 wrd2 = new BigEnd16(data, 2);

                if (((int)wrd1.UnsignedValue) == 1 && ((int)wrd2.UnsignedValue) == 0)
                    vers = new TTFOpenType1Version(wrd1.UnsignedValue, wrd2.UnsignedValue, data);


            }

            return vers != null;
        }

        private static char[] ConvertToChars(byte[] data, int count)
        {
            char[] chars = new char[count];

            for (int i = 0; i < count; i++)
            {
                chars[i] = (char)data[i];
            }
            return chars;
        }
    }

}
