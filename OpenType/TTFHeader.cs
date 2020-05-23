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
    public class TTFHeader
    {

        private TTFVersion _vers;
        public TTFVersion Version
        {
            get { return _vers; }
            set { this._vers = value; }
        }

        private int _numtables;

        public int NumberOfTables
        {
            get { return _numtables; }
            set { _numtables = value; }
        }

        private int _searchrange;

        public int SearchRange
        {
            get { return _searchrange; }
            set { _searchrange = value; }
        }

        private int _entrySel;

        public int EntrySelector
        {
            get { return _entrySel; }
            set { _entrySel = value; }
        }

        private int _rangeShift;

        public int RangeShift
        {
            get { return _rangeShift; }
            set { _rangeShift = value; }
        }

        internal static bool TryReadHeader(BigEndianReader reader, out TTFHeader header)
        {
            header = null;
            TTFVersion vers;
            if (TTFVersion.TryGetVersion(reader, out vers) == false)
                return false;

            ushort numtables = reader.ReadUInt16();
            ushort search = reader.ReadUInt16();
            ushort entry = reader.ReadUInt16();
            ushort range = reader.ReadUInt16();

            //Validate values returned.

            //searchRange is the (Maximum power of 2 <= numTables) * 16
            ushort max2 = 2;
            while (max2 * 2 <= numtables)
                max2 *= 2;

#if VALIDATEHEADER
            if (search != max2 * 16)
                return false;

            //entrySelector is Log2(max2)
            if (Math.Log(max2, 2) != entry)
                return false;

            //rangeShift = numTables * 16-searchRange
            if (range != ((numtables * 16) - search))
                return false;

#endif

            header = new TTFHeader();
            header.Version = vers;
            header.NumberOfTables = numtables;
            header.SearchRange = search;
            header.EntrySelector = entry;
            header.RangeShift = range;

            return true;
        }
    }
}
