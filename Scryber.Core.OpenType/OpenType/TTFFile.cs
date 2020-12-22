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
using System.Drawing;
using Scryber.OpenType.SubTables;

namespace Scryber.OpenType
{
    public class TTFFile
    {

        public TTFFile(byte[] data, int headOffset)
            : base()
        {
            this._path = string.Empty;
            this.Read(data, headOffset);
        }

        public TTFFile(string path, int headOffset)
            : base()
        {
            this._path = path;
            this.Read(path, headOffset);
        }

        private string _path;

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private TTFHeader _head;
        public TTFHeader Head
        {
            get { return _head; }
        }

        private TTFTableSet _tables;

        public TTFTableSet Tables
        {
            get
            {
                if (null == _tables)
                    _tables = new TTFTableSet(this.Directories);
                return _tables;
            }
        }

        private byte[] _alldata;

        public byte[] FileData 
        { 
            get { return _alldata; }
            private set { _alldata = value; }
        }

        private TTFDirectoryList _dirs;
        public TTFDirectoryList Directories
        {
            get { return _dirs; }
        }

        public bool HasCMap(CMapEncoding encoding)
        {
            CMAPSubTable tbl = this.Tables.CMap.GetOffsetTable(encoding);
            return null != tbl;
        }

        public void Read(string path, int headOffset)
        {
            this.Read(new System.IO.FileInfo(path), headOffset);
        }

        public void Read(System.IO.FileInfo fi, int headOffset)
        {
            if (fi.Exists == false)
                throw new System.IO.FileNotFoundException("The font file at '" + fi.FullName + "' does not exist");

            using (System.IO.FileStream fs = fi.OpenRead())
            {
                this.Read(fs, headOffset);
            }
        }

        public void Read(System.IO.Stream stream, int headOffset)
        {
            System.IO.MemoryStream ms = null;
            
            
            try
            {
                if (stream.CanSeek && stream.Length < (long)int.MaxValue)
                    ms = new System.IO.MemoryStream((int)stream.Length);
                else
                    ms = new System.IO.MemoryStream();

                //Copy the stream to a private data array
                byte[] buffer = new byte[4096];
                int count;
                while ((count = stream.Read(buffer, 0, 4096)) > 0)
                {
                    ms.Write(buffer, 0, count);
                }

                if(headOffset > 0)
                {
                    byte[] data = TTCFile.ExtractTTFfromTTC(ms, headOffset);
                    ms.Dispose();
                    ms = new System.IO.MemoryStream(data);

                }

                ms.Position = 0;
                using (BigEndianReader reader = new BigEndianReader(ms))
                {
                    this.Read(reader);
                }
                this.FileData = ms.ToArray();
            }
            catch (Exception ex)
            {
                throw new System.IO.IOException("Could not load the font file from the stream. " + ex.Message);
            }
            finally
            {
                if (null != ms)
                {
                    ms.Dispose();
                    ms = null;
                }
            }
        }

        public void Read(byte[] data, int position)
        {
            System.IO.MemoryStream ms = null;
            BigEndianReader ber = null;
            try
            {
                ms = new System.IO.MemoryStream(data);
                if(position != 0)
                {
                    data = TTCFile.ExtractTTFfromTTC(ms, position);
                    ms.Dispose();

                    ms = new System.IO.MemoryStream(data);
                }
                ber = new BigEndianReader(ms);
                

                this.Read(ber);
                this.FileData = data;
            }
            catch (Exception ex)
            {
                throw new System.IO.IOException("Could not load the font file from the stream. " + ex.Message);
            }
            finally
            {
                if (null != ber)
                    ber.Dispose();
                if (null != ms)
                    ms.Dispose();
            }
        }
        

        private void Read(BigEndianReader reader)
        {

            TTFHeader header;
            if (TTFHeader.TryReadHeader(reader, out header) == false)
                throw new NotSupportedException("The current stream is not a supported OpenType or TrueType font file");

            List<TTFDirectory> dirs;
            try
            {
                dirs = new List<TTFDirectory>();

                for (int i = 0; i < header.NumberOfTables; i++)
                {
                    TTFDirectory dir = new TTFDirectory();
                    dir.Read(reader);
                    dirs.Add(dir);
                }

                dirs.Sort(delegate(TTFDirectory one, TTFDirectory two) { return one.Offset.CompareTo(two.Offset); });
                this._dirs = new TTFDirectoryList(dirs);
                this._head = header;

                TTFTableFactory factory = this.GetFactory(header);
                foreach (TTFDirectory dir in dirs)
                {
                    TTFTable tbl = factory.ReadDirectory(dir, this, reader);
                    if(tbl != null)
                        dir.SetTable(tbl);
                }

                
            }
            catch (OutOfMemoryException) { throw; }
            catch (System.Threading.ThreadAbortException) { throw; }
            catch (StackOverflowException) { throw; }
            catch (TTFReadException) { throw; }
            catch (Exception ex) { throw new TTFReadException("Could not read the TTF File", ex); }

            
            
        }

        protected virtual TTFTableFactory GetFactory(TTFHeader header)
        {
            return header.Version.GetTableFactory();
        }

        public const double NoWordSpace = 0.0;
        public const double NoCharacterSpace = 0.0;
        public const double NoHorizontalScale = 1.0;

        /// <summary>
        /// Measures the size of the provided string at the specified font size (starting at a specific offset), 
        /// stopping when the available space is full and returning the number of characters fitted.
        /// </summary>
        /// <param name="encoding">The encoding to use to map the characters</param>
        /// <param name="s">The string to measure the size of</param>
        /// <param name="startOffset">The starting (zero based) offset in that string to start measuring from</param>
        /// <param name="emsize">The M size in font units</param>
        /// <param name="availablePts">The max width allowed for this string</param>
        /// <param name="wordspace">The spacing between words in font units. Default 0</param>
        /// <param name="charspace">The spacing between characters in font units. Default 0</param>
        /// <param name="hscale">The horizontal scaling of all characters. Default 100</param>
        /// <param name="vertical">If true then this is vertical writing</param>
        /// <param name="wordboundary">If True the measuring will stop at a boundary to a word rather than character.</param>
        /// <param name="charsfitted">Set to the number of characters that can be renered at this size within the width.</param>
        /// <returns></returns>
        public SizeF MeasureString(CMapEncoding encoding, string s, int startOffset, double emsize, double availablePts, double? wordspacePts, double charspacePts, double hscale, bool vertical, bool wordboundary, out int charsfitted)
        {
            HorizontalMetrics table = this.Directories["hmtx"].Table as HorizontalMetrics;
            CMAPTable cmap = this.Directories["cmap"].Table as CMAPTable;
            OS2Table os2 = this.Directories["OS/2"].Table as OS2Table;
            
            CMAPSubTable mac = cmap.GetOffsetTable(encoding);
            if (mac == null)
                mac = cmap.GetOffsetTable(CMapEncoding.MacRoman);

            HorizontalHeader hhead = this.Directories["hhea"].Table as HorizontalHeader;
            FontHeader head = this.Directories["head"].Table as FontHeader;

            double availableFU = availablePts * ((double)head.UnitsPerEm / emsize);
            double charspaceFU = NoCharacterSpace;

            if (charspacePts != NoCharacterSpace)
                charspaceFU = charspacePts * ((double)head.UnitsPerEm / emsize);

            double wordspaceFU = NoWordSpace;

            if (wordspacePts.HasValue)
                wordspaceFU = (wordspacePts.Value * ((double)head.UnitsPerEm / emsize));
            else if (charspacePts != NoCharacterSpace)
                //If we dont have explicit wordspacing then we use the character spacing
                wordspaceFU = charspaceFU;


            double len = 0.0;
            double lastwordlen = 0.0;
            int lastwordcount = 0;
            charsfitted = 0;


            for (int i = startOffset; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c))
                {
                    lastwordlen = len;
                    lastwordcount = charsfitted;
                }

                int moffset = (int)mac.GetCharacterGlyphOffset(c);
                
                if (moffset >= table.HMetrics.Count)
                    moffset = table.HMetrics.Count - 1;

                Scryber.OpenType.SubTables.HMetric metric;
                metric = table.HMetrics[moffset];
                double w = metric.AdvanceWidth;

                if (i == 0)
                {
                    w -= metric.LeftSideBearing;
                }

                if (c == ' ')
                {
                    if (wordspaceFU != NoWordSpace)
                        w += wordspaceFU;
                }
                else if (charspaceFU != NoCharacterSpace)
                    w += charspaceFU;




                if (hscale != NoHorizontalScale)
                    w *= hscale;

                len += w;

                //check if we can fit more
                if (len > availableFU)
                {
                    len -= w;
                    break;
                }
                charsfitted++;
            }

            if ((charsfitted + startOffset < s.Length) && wordboundary && lastwordlen > 0)
            {
                len = lastwordlen;
                charsfitted = lastwordcount;
            }

            len = len * emsize;
            len = len / (double)head.UnitsPerEm;
            double h = ((double)(os2.TypoAscender - os2.TypoDescender + os2.TypoLineGap) / (double)head.UnitsPerEm) * emsize;
            return new SizeF((float)len, (float)h);
        }


        public SizeF MeasureString(CMapEncoding encoding, string s, int startOffset, double emsize, double available, bool wordboundary, out int charsfitted)
        {
            HorizontalMetrics table = this.Directories["hmtx"].Table as HorizontalMetrics;
            CMAPTable cmap = this.Directories["cmap"].Table as CMAPTable;
            OS2Table os2 = this.Directories["OS/2"].Table as OS2Table;

            
            CMAPSubTable mac = cmap.GetOffsetTable(encoding);
            if (mac == null)
                mac = cmap.GetOffsetTable(CMapEncoding.MacRoman);

            HorizontalHeader hhead = this.Directories["hhea"].Table as HorizontalHeader;
            FontHeader head = this.Directories["head"].Table as FontHeader;
            available = (available * head.UnitsPerEm) / emsize;

            double len = 0.0;
            double lastwordlen = 0.0;
            int lastwordcount = 0;
            charsfitted = 0;

            
            for (int i = startOffset; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c))
                {
                    lastwordlen = len;
                    lastwordcount = charsfitted;
                }

                int moffset = (int)mac.GetCharacterGlyphOffset(c);
                //System.Diagnostics.Debug.WriteLine("Character '" + chars[i].ToString() + "' (" + ((byte)chars[i]).ToString() + ") has offset '" + moffset.ToString() + "' in mac encoding and '" + woffset + "' in windows encoding");

                if (moffset >= table.HMetrics.Count)
                    moffset = table.HMetrics.Count - 1;
                Scryber.OpenType.SubTables.HMetric metric;
                metric = table.HMetrics[moffset];
                if (i == 0)
                    len = -metric.LeftSideBearing;
                len += metric.AdvanceWidth;
                
                //check if we can fit more
                if (len > available)
                {
                    len -= metric.AdvanceWidth;
                    break;
                }
                charsfitted++;
            }

            if ((charsfitted + startOffset < s.Length) && wordboundary && lastwordlen > 0)
            {
                len = lastwordlen;
                charsfitted = lastwordcount;
            }
            
            len = len / (double)head.UnitsPerEm;
            len = len * emsize;
            double h = ((double)(os2.TypoAscender - os2.TypoDescender + os2.TypoLineGap) / (double)head.UnitsPerEm) * emsize;
            return new SizeF((float)len, (float)h);
        }

        public static bool CanRead(string path)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(path);

            return CanRead(fi);
        }
        public static bool CanRead(System.IO.FileInfo fi)
        {
            if (fi.Exists == false)
                return false;
            else
            {
                using (System.IO.FileStream fs = fi.OpenRead())
                {
                    return CanRead(fs);
                }
            }
        }

        public static bool CanRead(System.IO.Stream stream)
        {
            BigEndianReader reader = new BigEndianReader(stream);
            return CanRead(reader);
            
        }

        public static bool CanRead(BigEndianReader reader)
        {
            long oldpos = reader.Position;
            reader.Position = 0;
            TTFHeader header;

            bool b = TTFHeader.TryReadHeader(reader, out header);

            reader.Position = oldpos;

            return b;
        }
    }
}
