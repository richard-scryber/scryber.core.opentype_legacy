using System;
namespace Scryber.OpenType.Utility
{
    public sealed class UnknownTypeface : ITypefaceInfo
    {
        public string Path { get; private set; }

        public int TypefaceCount { get { return 0; } }

        public ITypefaceReference[] References { get { return null; } }

        public DataFormat SourceFormat { get { return DataFormat.Other; } }

        public string ErrorMessage { get; private set; }

        public UnknownTypeface(string sourcePath, string error)
        {
            this.Path = sourcePath;
            this.ErrorMessage = error;
        }
    }
}
