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
    public class TTFDirectory
    {
        private string _tag;
        public string Tag
        {
            get { return _tag; }
        }

        private uint _checksum;
        public uint CheckSum
        {
            get { return _checksum; }
        }

        private uint _offset;
        public uint Offset
        {
            get { return _offset; }
        }

        private uint _len;
        public uint Length
        {
            get { return _len; }
        }

        private TTFTable _tbl;
        public TTFTable Table
        {
            get { return _tbl; }
        }

        public TTFDirectory()
        {

        }

        public TTFDirectory(string tag, uint checksum, uint offset, uint len)
        {
            _tag = tag;
            _checksum = checksum;
            _offset = offset;
            _len = len;
        }

        public void Read(BigEndianReader reader)
        {
            this._tag = reader.ReadString(4);
            //this._tag = new string(tag);
            this._checksum = reader.ReadUInt32();
            this._offset = reader.ReadUInt32();
            this._len = reader.ReadUInt32();
        }

        

        public override string ToString()
        {
            return "Directory : " + this.Tag + " (from '" + this.Offset.ToString() + "' to '" + (this.Length + this.Offset).ToString() + "'";
        }

        internal void SetTable(TTFTable tbl)
        {
            this._tbl = tbl;
        }
    }

    public class TTFDirectoryList : System.Collections.ObjectModel.KeyedCollection<string, TTFDirectory>
    {
        public TTFDirectoryList()
            : base()
        {
        }

        public TTFDirectoryList(IEnumerable<TTFDirectory> items)
            : this()
        {
            if (items != null)
            {
                foreach (TTFDirectory item in items)
                {
                    this.Add(item);
                }
            }
        }

        protected override string GetKeyForItem(TTFDirectory item)
        {
            return item.Tag;
        }
    }
}
