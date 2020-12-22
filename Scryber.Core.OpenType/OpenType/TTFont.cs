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
    public class TTFont
    {
        private TTFFile _file;
        public TTFFile File
        {
            get { return _file; }
        }

        private int _sizeinpts;

        public int SizeInPoints
        {
            get { return _sizeinpts; }
            set { _sizeinpts = value; }
        }

        public TTFont(TTFFile file, int sizeInPoints)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (sizeInPoints < 1)
                throw new ArgumentException("sizeInPoints");

            this._file = file;
            this._sizeinpts = sizeInPoints;
        }

        public string FamilyName
        {
            get
            {
                return File.Head.Version.ToString();
            }
        }
	
    }
}
