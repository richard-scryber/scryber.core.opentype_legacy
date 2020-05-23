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
    public class TTFTableSet
    {
        private TTFDirectoryList _directories;

        public TTFTableSet(TTFDirectoryList dirs)
        {
            _directories = dirs;
        }

        public SubTables.NamingTable Names
        {
            get { return (SubTables.NamingTable)GetTable(TTFTables.NamingTable); }
        }

        public SubTables.CMAPTable CMap
        {
            get { return (SubTables.CMAPTable)GetTable(TTFTables.CharacterMapping); }
        }

        public SubTables.FontHeader FontHeader
        {
            get { return (SubTables.FontHeader)GetTable(TTFTables.FontHeader); }
        }

        public SubTables.HorizontalHeader HorizontalHeader
        {
            get { return (SubTables.HorizontalHeader)GetTable(TTFTables.HorizontalHeader); }
        }

        public SubTables.HorizontalMetrics HorizontalMetrics
        {
            get { return (SubTables.HorizontalMetrics)GetTable(TTFTables.HorizontalMetrics); }
        }

        public SubTables.MaxProfile MaximumProfile
        {
            get { return (SubTables.MaxProfile)GetTable(TTFTables.MaximumProfile); }
        }

        public SubTables.OS2Table WindowsMetrics
        {
            get { return (SubTables.OS2Table)GetTable(TTFTables.WindowsMetrics); }
        }

        public SubTables.PostscriptTable PostscriptInformation
        {
            get { return (SubTables.PostscriptTable)GetTable(TTFTables.PostscriptInformation); }
        }


        public bool TryGetTable(string name, out TTFTable table)
        {
            if (this._directories.Contains(name))
                table = _directories[name].Table;
            else
                table = null;
            return null != table;
        }



        public TTFTable GetTable(string name)
        {
            return _directories[name].Table;
        }
    }
}
