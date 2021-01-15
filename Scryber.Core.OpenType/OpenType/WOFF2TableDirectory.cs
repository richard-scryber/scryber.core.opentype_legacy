using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Scryber.OpenType.SubTables;

namespace Scryber.OpenType
{
    public class WOFF2TableDirectory
    {
        public List<WOFF2TableEntry> AllEntries { get; private set; }

        public WOFF2TableDirectory()
        {
        }

        
        public static bool TryReadDirectory(BigEndianReader reader, int numTables, out WOFF2TableDirectory dir)
        {
            dir = null;
            List<WOFF2TableEntry> entries = new List<WOFF2TableEntry>(numTables);

            for (var i = 0; i < numTables; i++)
            {
                var flags = reader.ReadByte();
                string flagStr = Convert.ToString(flags, 2);


                //clear bits 6 and 7
                var type = (int)flags & 63;
                string typeStr = Convert.ToString(type, 2);

                //all but bits 6 & 7
                var transform = flags & 192;
                
                string transforStr = Convert.ToString(transform, 2);
                transform = transform << 5;
                transforStr = Convert.ToString(transform, 2);

                var table = (SubTables.WOFF2TableTypes)type;
                string other = null;

                if (table == SubTables.WOFF2TableTypes._Arbitary)
                {
                    //We have a 4 character
                    other = reader.ReadString(4);
                }
                else
                {
                    other = WOFF2TableEntry.WOFF2TablesToNames[table];
                }

                uint origLen;

                if (!ReadUIntBase128(reader, out origLen))
                    return false;

                var transformLen = origLen;
                var isTransofrmed = HasTransform(table, transform);

                if (isTransofrmed)
                {
                    if (!ReadUIntBase128(reader, out transformLen))
                        return false;
                }

                WOFF2TableEntry entry = new WOFF2TableEntry()
                {
                    IsTransformed = isTransofrmed,
                    Type = table,
                    TypeName = other,
                    TableLength = origLen,
                    TransformedLength = transformLen
                };
                entries.Add(entry);
            }

            dir = new WOFF2TableDirectory();
            dir.AllEntries = entries;

            return true;
        }

        private static bool HasTransform(SubTables.WOFF2TableTypes table, int transform)
        {
            if (table == SubTables.WOFF2TableTypes.glyf || table == SubTables.WOFF2TableTypes.loca)
                return transform == 0;

            else if (transform > 0)
                return true;

            else
                return false;
        }

        private static bool ReadUIntBase128(BigEndianReader reader, out UInt32 result)
        {
            uint accum = 0;
            result = 0;
            for (int i = 0; i < 5; ++i)
            {
                byte data_byte = reader.ReadByte();
                // No leading 0's
                if (i == 0 && data_byte == 0x80) return false;

                // If any of top 7 bits are set then << 7 would overflow
                if ((accum & 0xFE000000) != 0) return false;
                //
                accum = (uint)(accum << 7) | (uint)(data_byte & 0x7F);
                // Spin until most significant bit of data byte is false
                if ((data_byte & 0x80) == 0)
                {
                    result = accum;
                    return true;
                }
                //
            }
            // UIntBase128 sequence exceeds 5 bytes
            return false;
        }
    }

    public class WOFF2TableEntry
    {
        public SubTables.WOFF2TableTypes Type { get; set; }
        public string TypeName { get; set; }
        public UInt32 TableLength { get; set; }
        public UInt32 TransformedLength { get; set; }
        public bool IsTransformed { get; set; }

        public override string ToString()
        {
            if (this.IsTransformed)
            {
                if (!string.IsNullOrEmpty(TypeName))
                    return TypeName + " (transformed)";
                else
                    return Type.ToString() + " (transformed)";
            }
            else
            {
                if (!string.IsNullOrEmpty(TypeName))
                    return TypeName;
                else
                    return Type.ToString();
            }
        }



        public static Dictionary<WOFF2TableTypes, string> WOFF2TablesToNames = new Dictionary<WOFF2TableTypes, string>()
    {
        { WOFF2TableTypes.cmap, "cmap" },
        { WOFF2TableTypes.head, "head" },
        { WOFF2TableTypes.hhea, "hhea" },
        { WOFF2TableTypes.hmtx, "hmtx" },
        { WOFF2TableTypes.maxp, "maxp" },
        { WOFF2TableTypes.name , "name" },
        { WOFF2TableTypes.OS_2 , "OS/2" },
        { WOFF2TableTypes.post , "post" },
        { WOFF2TableTypes.cvt_ , "cvt " },
        { WOFF2TableTypes.fpgm , "fpgm" },
        { WOFF2TableTypes.glyf , "glyf" },
        { WOFF2TableTypes.loca , "loca" },
        { WOFF2TableTypes.prep , "prep" },
        { WOFF2TableTypes.CFF_ , "CFF " },
        { WOFF2TableTypes.VORG , "VORG" },
        { WOFF2TableTypes.EBDT , "EBDT" },
        { WOFF2TableTypes.EBLC , "EBLC" },
        { WOFF2TableTypes.gasp , "gasp" },
        { WOFF2TableTypes.hdmx , "hdmx" },
        { WOFF2TableTypes.kern , "kern" },
        { WOFF2TableTypes.LTSH , "LTSH" },
        { WOFF2TableTypes.PCLT , "PCLT" },
        { WOFF2TableTypes.VDMX , "VDMX" },
        { WOFF2TableTypes.vhea , "vhea" },
        { WOFF2TableTypes.vmtx, "vmtx" },
        { WOFF2TableTypes.BASE, "BASE" },
        { WOFF2TableTypes.GDEF, "GDEF" },
        { WOFF2TableTypes.GPOS, "GPOS" },
        { WOFF2TableTypes.GSUB , "GSUB" },
        { WOFF2TableTypes.EBSC , "EBSC" },
        { WOFF2TableTypes.JSTF , "JSTF" },
        { WOFF2TableTypes.MATH , "MATH" },
        { WOFF2TableTypes.CBDT , "CBDT" },
        { WOFF2TableTypes.CBLC , "CBLC" },
        { WOFF2TableTypes.COLR , "COLR" },
        { WOFF2TableTypes.CPAL , "CPAL" },
        { WOFF2TableTypes.SVG_ , "SVG " },
        { WOFF2TableTypes.sbix , "sbix" },
        { WOFF2TableTypes.acnt , "acnt" },
        { WOFF2TableTypes.avar , "avar" },
        { WOFF2TableTypes.bdat, "bdat" },
        { WOFF2TableTypes.bloc, "bloc" },
        { WOFF2TableTypes.bsln , "bsln" },
        { WOFF2TableTypes.cvar , "cvar" },
        { WOFF2TableTypes.fdsc , "fdsc" },
        { WOFF2TableTypes.feat, "feat" },
        { WOFF2TableTypes.fmtx , "fmtx" },
        { WOFF2TableTypes.fvar , "fvar" },
        { WOFF2TableTypes.gvar , "gvar" },
        { WOFF2TableTypes.hsty, "hsty" },
        { WOFF2TableTypes.just , "just" },
        { WOFF2TableTypes.lcar , "lcar" },
        { WOFF2TableTypes.mort , "mort" },
        { WOFF2TableTypes.morx , "morx" },
        { WOFF2TableTypes.opbd , "opbd" },
        { WOFF2TableTypes.prop , "prop" },
        { WOFF2TableTypes.trak , "trak" },
        { WOFF2TableTypes.Zapf , "Zapf" },
        { WOFF2TableTypes.Silf , "Silf" },
        { WOFF2TableTypes.Glat , "Glat" },
        { WOFF2TableTypes.Gloc , "Gloc" },
        { WOFF2TableTypes.Feat , "Feat" },
        { WOFF2TableTypes.Sill, "Sill" }

        };
    }
}
