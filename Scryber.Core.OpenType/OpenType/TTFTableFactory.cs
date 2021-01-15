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
using Scryber.OpenType.SubTables;

namespace Scryber.OpenType
{
    public abstract class TTFTableFactory
    {
        private bool _throwOnNotFound;

        public bool ThrowOnNotFound
        {
            get { return _throwOnNotFound; }
        }

        private List<string> _required;

        protected List<string> RequiredTables
        {
            get { return this._required; }
        }

        private BigEndianReader _reader;

        public BigEndianReader Reader
        {
            get { return _reader; }
            protected set { _reader = value; }
        }

        private TTFDirectoryList _directories;

        public TTFDirectoryList Directories
        {
            get { return this._directories; }
            protected set { this._directories = value; }
        }

        private TTFHeader _header;

        public TTFHeader Header
        {
            get { return _header; }
            protected set { this._header = value; }
        }

        public TTFTableFactory(TTFVersion version, BigEndianReader reader, bool throwonnotfound, IEnumerable<string> required)
        {
            this._reader = reader;
            this._throwOnNotFound = throwonnotfound;
            this._required = new List<string>(required);
            this._header = this.ReadHeader(version);
            this._directories = this.ReadDirectories();
        }

        protected virtual TTFDirectoryList ReadDirectories()
        {
            return null;
        }

        protected virtual TTFHeader ReadHeader(TTFVersion vers)
        {
            var header = new TTFOpenTypeHeader();

            var reader = this.Reader;
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

            header = new TTFOpenTypeHeader();
            header.Version = vers;
            header.NumberOfTables = numtables;
            header.SearchRange = search;
            header.EntrySelector = entry;
            header.RangeShift = range;

            return header;
        }

        public TTFTable ReadDirectory(string tname)
        {
            TTFDirectory dir = this.Directories[tname];

            this.Reader.Position = dir.Offset;
            return this.ReadTable(dir.Tag, dir.Length, this.Directories, this.Reader);
        }

        public TTFTable ReadDirectory(TTFDirectory directory)
        {
            if (null == directory)
                throw new ArgumentNullException("directory");

            if (this.Directories.IndexOf(directory) < 0)
                throw new InvalidOperationException("This is not a directory in the fonts tables");

            this.Reader.Position = directory.Offset;
            return this.ReadTable(directory.Tag, directory.Length, this.Directories, this.Reader);
        }

        protected abstract TTFTable ReadTable(string key, uint length, TTFDirectoryList dirlist, BigEndianReader reader);


        public void ValidateReadTables()
        {
            if (this._required.Count > 0)
            {
                StringBuilder sb = new StringBuilder("The required tables '");
                for (int i = 0; i < this._required.Count; i++)
                {
                    if (i > 0)
                        sb.Append("', '");
                    sb.Append(this._required[i]);
                }
                sb.Append("' were not found in the font definition");

                throw new TTFReadException(sb.ToString());
            }
        }
    }



}
