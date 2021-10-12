using System;
using System.Collections.Generic;
using Scryber.OpenType;
using Scryber.OpenType.SubTables;

namespace Scryber
{

    public delegate double Measurer(string chars, int startOffset, double emsize, double available, bool wordboundary, out int fitted);

    public class TTFStringMeasurer
    {
        private CMAPSubTable _offsets;
        private List<HMetric> _metrics;
        private int unitsPerEm;
        private Dictionary<char, HMetric> _lookup;

        private TTFStringMeasurer(int unitsPerEm, CMAPSubTable offsets, List<HMetric> metrics)
        {
            this.unitsPerEm = unitsPerEm;
            this._offsets = offsets;
            this._metrics = metrics;
            this._lookup = new Dictionary<char, HMetric>();
        }

        public double MeasureChars(string chars, int startOffset, double emsize, double available, bool wordboundary, out int fitted)
        {
            int totalUnits = (int)((available / emsize) * this.unitsPerEm);

            int measured = 0;
            int count = 0;

            int lastWordLen = 0;
            int lastWordCount = 0;

            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];

                if(char.IsWhiteSpace(c))
                {
                    lastWordLen = measured;
                    lastWordCount = count;
                }
                HMetric metric;

                if (_lookup.TryGetValue(c, out metric) == false)
                {
                    int moffset = _offsets.GetCharacterGlyphOffset(c);

                    if (moffset >= _metrics.Count)
                        moffset = _metrics.Count - 1;

                    metric = _metrics[moffset];
                    _lookup.Add(c, metric);
                }

                if (i == 0)
                    measured -= metric.LeftSideBearing;

                measured += metric.AdvanceWidth;

                if (measured > totalUnits)
                {
                    lastWordLen -= metric.AdvanceWidth;
                    break;
                }
                else
                    count++;
            }

            if(count + startOffset < chars.Length)
            {
                if (wordboundary && lastWordLen > 0)
                {
                    //Not everything fitted so go bask to the last full word
                    //(if there was one).
                    measured = lastWordLen;
                    count = lastWordCount;
                }
            }

            double w = (measured * emsize) / (double)unitsPerEm;
            fitted = count;
            return w;

        }


        public static TTFStringMeasurer Create(TTFFile forfont, CMapEncoding encoding, double charSpace, double wordspace)
        {
            HorizontalMetrics table = forfont.Directories["hmtx"].Table as HorizontalMetrics;
            CMAPTable cmap = forfont.Directories["cmap"].Table as CMAPTable;
            OS2Table os2 = forfont.Directories["OS/2"].Table as OS2Table;
            FontHeader head = forfont.Directories["head"].Table as FontHeader;
            HorizontalHeader hhead = forfont.Directories["hhea"].Table as HorizontalHeader;
            CMAPSubTable mac = cmap.GetOffsetTable(encoding);
            if (mac == null)
                mac = cmap.GetOffsetTable(CMapEncoding.MacRoman);

            return new TTFStringMeasurer(head.UnitsPerEm, mac, table.HMetrics);
        }
    }

    
}
