using System;
namespace Scryber.OpenType
{
    public class TTFOpenTypeHeader : TTFHeader
    {
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
    }
}
