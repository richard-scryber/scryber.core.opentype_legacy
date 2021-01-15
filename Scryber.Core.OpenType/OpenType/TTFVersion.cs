/*  Copyright 2012 PerceiveIT Limited
 *  This file is part of the Scryber library.
 *
 *  You can redistribute Scryber and/or modify 
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 * 
 *  Scryber is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 * 
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with Scryber source code in the COPYING.txt file.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Scryber.OpenType
{
    public abstract class TTFVersion
    {
        public byte[] HeaderData { get; private set; }

        public abstract override string ToString();

        public abstract bool IsCollection { get; }

        public abstract TTFTableFactory GetTableFactory();

        public TTFVersion(byte[] header)
        {
            this.HeaderData = header;
        }


        #region public static TTFVersion GetVersion(BigEndianReader reader)

        public static bool TryGetVersion(BigEndianReader reader, out TTFVersion vers)
        {
            vers = null;
            byte[] data = reader.Read(4);
            char[] chars = ConvertToChars(data,4);

            if (chars[0] == 'O' && chars[1] == 'T' && chars[2] == 'T' && chars[3] == 'O')
            {
                //CCF Format is not supported
            }
            else if (chars[0] == 't' && chars[1] == 'r' && chars[2] == 'u' && chars[3] == 'e')
                vers = new TTFTrueTypeVersion(new string(chars), data);

            else if (chars[0] == 't' && chars[1] == 'y' && chars[2] == 'p' && chars[3] == '1')
                vers = new TTFTrueTypeVersion(new string(chars), data);
            else if (chars[0] == 't' && chars[1] == 't' && chars[2] == 'c' && chars[3] == 'f')
                vers = new TTFCollectionVersion(new string(chars), data);
            else
            {
                BigEnd16 wrd1 = new BigEnd16(data, 0);
                BigEnd16 wrd2 = new BigEnd16(data, 2);

                if (((int)wrd1.UnsignedValue) == 1 && ((int)wrd2.UnsignedValue) == 0)
                    vers = new TTFOpenType1Version(wrd1.UnsignedValue, wrd2.UnsignedValue, data);


            }
            
            return vers != null;
        }
        public static TTFVersion GetVersion(BigEndianReader reader)
        {
            TTFVersion version;

            if (TryGetVersion(reader, out version) == false)
                throw new TTFReadException("The version could not be identified");

            return version;
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

        #endregion

    }

    public class TTFOpenType1Version : TTFVersion
    {
        private Version _innervers;
        protected Version InnerVersion
        {
            get { return _innervers; }
        }

        public override bool IsCollection {  get { return false; } }

        public TTFOpenType1Version(UInt16 major, UInt16 minor, byte[] data) : base(data)
        {
            this._innervers = new Version((int)major, (int)minor);
            
            if (this._innervers != new Version("1.0"))
                throw new TTFReadException("The open type version can only be version 1.0");
        }

        public override string ToString()
        {
            return "Open Type " + InnerVersion.ToString();
        }

        public override TTFTableFactory GetTableFactory()
        {
            return new TTFOpenTypeTableFactory(false);
        }
    }

    public class TTFCollectionVersion : TTFVersion
    {

        public string VersionIdentifier
        {
            get;
            private set;
        }

        public override bool IsCollection { get { return true; } }

        public TTFCollectionVersion(string type, byte[] data) : base(data)
        {
            if (string.IsNullOrEmpty(type) || (type.Equals("ttcf", StringComparison.OrdinalIgnoreCase) == false))
                throw new TTFReadException("The True Type collection version must be ttcf");
            this.VersionIdentifier = type;
        }

        public override string ToString()
        {
            return "TT Collection : " + this.VersionIdentifier;
        }

        public override TTFTableFactory GetTableFactory()
        {
            return new TTFOpenTypeTableFactory(false);
        }
    }

    public class TTFTrueTypeVersion : TTFVersion
    {
        private string _versid;
        public string VersionIdentifier
        {
            get { return _versid; }
        }

        public override bool IsCollection { get { return false; } }


        public TTFTrueTypeVersion(string id, byte[] data) : base(data)
        {
            if (string.IsNullOrEmpty(id) || (id.Equals("TRUE", StringComparison.CurrentCultureIgnoreCase) || id.Equals("typ1", StringComparison.CurrentCultureIgnoreCase)) == false)
                throw new TTFReadException("The true type version must be either 'true' or 'typ1'");

            this._versid = id;
        }

        public override string ToString()
        {
            return "True Type : " + this.VersionIdentifier;
        }

        public override TTFTableFactory GetTableFactory()
        {
            return null;
        }
    }
}
