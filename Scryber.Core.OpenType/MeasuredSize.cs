using System;
namespace Scryber
{
    /// <summary>
    /// Defines the size of the characters measured and the space needed to fit them.
    /// </summary>
    public struct MeasuredSize
    {
        public double RequiredWidth { get; set; }

        public double RequiredHeight { get; set; }

        public MeasuredSize(double width, double height)
        {
            this.RequiredWidth = width;
            this.RequiredHeight = height;
        }
    }
}
