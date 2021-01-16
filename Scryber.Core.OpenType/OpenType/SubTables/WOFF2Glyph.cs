using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Scryber.OpenType.SubTables
{
    public class WOFF2Glyph : TTFTable
    {
        Glyph[] _glyphs;
        readonly GlyphLocations _glyphLocations;

        internal readonly Glyph _emptyGlyph = new Glyph(new GlyphPointF[0], new ushort[0], Bounds.Zero, null, 0);

        public Glyph[] Glyphs { get { return _glyphs; } internal set { _glyphs = value; } }

        public WOFF2Glyph(long offset)
            : base(offset)
        {
        }

        #region public readonly struct MathValueRecord

        public readonly struct MathValueRecord
        {
            //MathValueRecord
            //Type      Name            Description
            //int16     Value           The X or Y value in design units
            //Offset16  DeviceTable     Offset to the device table – from the beginning of parent table.May be NULL. Suggested format for device table is 1.
            public readonly short Value;
            public readonly ushort DeviceTable;
            public MathValueRecord(short value, ushort deviceTable)
            {
                this.Value = value;
                this.DeviceTable = deviceTable;
            }
#if DEBUG
            public override string ToString()
            {
                if (DeviceTable == 0)
                {
                    return Value.ToString();
                }
                else
                {
                    return Value + "," + DeviceTable;
                }

            }
#endif
        }

        #endregion

        #region public class MathKern

        public class MathKern
        {
            //reference =>see  MathKernTable
            public ushort HeightCount;
            public MathValueRecord[] CorrectionHeights;
            public MathValueRecord[] KernValues;

            public MathKern(ushort heightCount, MathValueRecord[] correctionHeights, MathValueRecord[] kernValues)
            {
                HeightCount = heightCount;
                CorrectionHeights = correctionHeights;
                KernValues = kernValues;
            }

#if DEBUG
            public override string ToString()
            {
                return HeightCount.ToString();
            }
#endif
        }

        #endregion

        #region public readonly struct MathKernInfoRecord

        public readonly struct MathKernInfoRecord
        {
            //resolved value
            public readonly MathKern TopRight;
            public readonly MathKern TopLeft;
            public readonly MathKern BottomRight;
            public readonly MathKern BottomLeft;
            public MathKernInfoRecord(MathKern topRight,
                 MathKern topLeft,
                 MathKern bottomRight,
                 MathKern bottomLeft)
            {
                TopRight = topLeft;
                TopLeft = topLeft;
                BottomRight = bottomRight;
                BottomLeft = bottomLeft;
            }
        }

        #endregion

        #region public class MathGlyphInfo

        public class MathGlyphInfo
        {
            public readonly ushort GlyphIndex;
            public MathGlyphInfo(ushort glyphIndex)
            {
                this.GlyphIndex = glyphIndex;
            }

            public MathValueRecord? ItalicCorrection { get; internal set; }
            public MathValueRecord? TopAccentAttachment { get; internal set; }
            public bool IsShapeExtensible { get; internal set; }

            //optional 
            public MathKern TopLeftMathKern => _mathKernRec.TopLeft;
            public MathKern TopRightMathKern => _mathKernRec.TopRight;
            public MathKern BottomLeftMathKern => _mathKernRec.BottomLeft;
            public MathKern BottomRightMathKern => _mathKernRec.BottomRight;
            public bool HasSomeMathKern { get; private set; }

            //
            MathKernInfoRecord _mathKernRec;
            internal void SetMathKerns(MathKernInfoRecord mathKernRec)
            {
                _mathKernRec = mathKernRec;
                HasSomeMathKern = true;
            }

            /// <summary>
            /// vertical glyph construction
            /// </summary>
            public MathGlyphConstruction VertGlyphConstruction;
            /// <summary>
            /// horizontal glyph construction
            /// </summary>
            public MathGlyphConstruction HoriGlyphConstruction;

        }

        #endregion

        #region public class MathGlyphConstruction

        public class MathGlyphConstruction
        {
            public MathValueRecord GlyphAsm_ItalicCorrection;
            public GlyphPartRecord[] GlyphAsm_GlyphPartRecords;
            public MathGlyphVariantRecord[] glyphVariantRecords;
        }

        #endregion

        #region public readonly struct GlyphPartRecord

        public readonly struct GlyphPartRecord
        {
            //Thus, a GlyphPartRecord consists of the following fields: 
            //1) Glyph ID for the part.
            //2) Lengths of the connectors on each end of the glyph. 
            //      The connectors are straight parts of the glyph that can be used to link it with the next or previous part.
            //      The connectors of neighboring parts can overlap, which provides flexibility of how these glyphs can be put together.However, the overlap should not be less than the value of MinConnectorOverlap defined in the MathVariants tables, and it should not exceed the length of either of two overlapping connectors.If the part does not have a connector on one of its sides, the corresponding length should be set to zero.

            //3) The full advance of the part. 
            //      It is also used to determine the measurement of the result by using the following formula:

            //  *** Size of Assembly = Offset of the Last Part + Full Advance of the Last Part ***

            //4) PartFlags is the last field.
            //      It identifies a number of parts as extenders – those parts that can be repeated(that is, multiple instances of them can be used in place of one) or skipped altogether.Usually the extenders are vertical or horizontal bars of the appropriate thickness, aligned with the rest of the assembly.

            //To ensure that the width/height is distributed equally and the symmetry of the shape is preserved,
            //following steps can be used by math handling client.

            //1. Assemble all parts by overlapping connectors by maximum amount, and removing all extenders.
            //  This gives the smallest possible result.

            //2. Determine how much extra width/height can be distributed into all connections between neighboring parts.
            //   If that is enough to achieve the size goal, extend each connection equally by changing overlaps of connectors to finish the job.
            //3. If all connections have been extended to minimum overlap and further growth is needed, add one of each extender, 
            //and repeat the process from the first step.

            //Note that for assemblies growing in vertical direction,
            //the distribution of height or the result between ascent and descent is not defined.
            //The math handling client is responsible for positioning the resulting assembly relative to the baseline.


            //GlyphPartRecord Table
            //Type      Name                    Description
            //uint16    Glyph                   Glyph ID for the part.
            //uint16    StartConnectorLength    Advance width/ height of the straight bar connector material, in design units, is at the beginning of the glyph, in the direction of the extension.
            //uint16    EndConnectorLength      Advance width/ height of the straight bar connector material, in design units, is at the end of the glyph, in the direction of the extension.
            //uint16    FullAdvance             Full advance width/height for this part, in the direction of the extension.In design units.
            //uint16    PartFlags               Part qualifiers. PartFlags enumeration currently uses only one bit:
            //                                       0x0001 fExtender If set, the part can be skipped or repeated.
            //                                       0xFFFE Reserved.

            public readonly ushort GlyphId;
            public readonly ushort StartConnectorLength;
            public readonly ushort EndConnectorLength;
            public readonly ushort FullAdvance;
            public readonly ushort PartFlags;
            public bool IsExtender => (PartFlags & 0x0001) != 0;

            public GlyphPartRecord(ushort glyphId, ushort startConnectorLength, ushort endConnectorLength, ushort fullAdvance, ushort partFlags)
            {
                GlyphId = glyphId;
                StartConnectorLength = startConnectorLength;
                EndConnectorLength = endConnectorLength;
                FullAdvance = fullAdvance;
                PartFlags = partFlags;
            }
#if DEBUG
            public override string ToString()
            {
                return "glyph_id:" + GlyphId;
            }
#endif
        }

        #endregion

        #region public readonly struct MathGlyphVariantRecord

        public readonly struct MathGlyphVariantRecord
        {
            //    MathGlyphVariantRecord Table
            //Type      Name                Description
            //uint16    VariantGlyph        Glyph ID for the variant.
            //uint16    AdvanceMeasurement  Advance width/height, in design units, of the variant, in the direction of requested glyph extension.
            public readonly ushort VariantGlyph;
            public readonly ushort AdvanceMeasurement;
            public MathGlyphVariantRecord(ushort variantGlyph, ushort advanceMeasurement)
            {
                this.VariantGlyph = variantGlyph;
                this.AdvanceMeasurement = advanceMeasurement;
            }

#if DEBUG
            public override string ToString()
            {
                return "variant_glyph_id:" + VariantGlyph + ", adv:" + AdvanceMeasurement;
            }
#endif
        }

        #endregion

        #region public readonly struct Type2Instruction

        public readonly struct Type2Instruction
        {
            public readonly int Value;
            public readonly byte Op;
            public Type2Instruction(OperatorName op, int value)
            {
                this.Op = (byte)op;
                this.Value = value;
#if DEBUG
                _dbug_OnlyOp = false;
#endif
            }
            public Type2Instruction(byte op, int value)
            {
                this.Op = op;
                this.Value = value;
#if DEBUG
                _dbug_OnlyOp = false;
#endif
            }
            public Type2Instruction(OperatorName op)
            {
                this.Op = (byte)op;
                this.Value = 0;
#if DEBUG
                _dbug_OnlyOp = true;
#endif
            }


            public float ReadValueAsFixed1616()
            {
                byte b0 = (byte)((0xff) & Value >> 24);
                byte b1 = (byte)((0xff) & Value >> 16);
                byte b2 = (byte)((0xff) & Value >> 8);
                byte b3 = (byte)((0xff) & Value >> 0);


                ///This number is interpreted as a Fixed; that is, a signed number with 16 bits of fraction
                float int_part = (short)((b0 << 8) | b1);
                float fraction_part = (short)((b2 << 8) | b3) / (float)(1 << 16);
                return int_part + fraction_part;
            }

            internal bool IsLoadInt => (OperatorName)Op == OperatorName.LoadInt;

#if DEBUG
            readonly bool _dbug_OnlyOp;

            [System.ThreadStatic]
            static System.Text.StringBuilder s_dbugSb;

            public override string ToString()
            {

                int merge_flags = Op >> 6; //upper most 2 bits we use as our extension
                                           //so operator name is lower 6 bits

                int only_operator = Op & 0b111111;
                OperatorName op_name = (OperatorName)only_operator;

                if (_dbug_OnlyOp)
                {
                    return op_name.ToString();
                }
                else
                {
                    if (s_dbugSb == null)
                    {
                        s_dbugSb = new System.Text.StringBuilder();
                    }
                    s_dbugSb.Length = 0;//reset

                    bool has_ExtenedForm = true;


                    //this is my extension
                    switch (merge_flags)
                    {
#if DEBUG
                        default: throw new NotSupportedException();
#endif
                        case 0:
                            //nothing 
                            has_ExtenedForm = false;
                            break;
                        case 1:
                            //contains merge data for LoadInt
                            s_dbugSb.Append(Value.ToString() + " ");
                            break;
                        case 2:
                            //contains merge data for LoadShort2
                            s_dbugSb.Append((short)(Value >> 16) + " " + (short)(Value >> 0) + " ");
                            break;
                        case 3:
                            //contains merge data for LoadSbyte4
                            s_dbugSb.Append((sbyte)(Value >> 24) + " " + (sbyte)(Value >> 16) + " " + (sbyte)(Value >> 8) + " " + (sbyte)(Value) + " ");
                            break;
                    }

                    switch (op_name)
                    {
                        case OperatorName.LoadInt:
                            s_dbugSb.Append(Value);
                            break;
                        case OperatorName.LoadFloat:
                            s_dbugSb.Append(ReadValueAsFixed1616().ToString());
                            break;
                        //-----------
                        case OperatorName.LoadShort2:
                            s_dbugSb.Append((short)(Value >> 16) + " " + (short)(Value >> 0));
                            break;
                        case OperatorName.LoadSbyte4:
                            s_dbugSb.Append((sbyte)(Value >> 24) + " " + (sbyte)(Value >> 16) + " " + (sbyte)(Value >> 8) + " " + (sbyte)(Value));
                            break;
                        case OperatorName.LoadSbyte3:
                            s_dbugSb.Append((sbyte)(Value >> 24) + " " + (sbyte)(Value >> 16) + " " + (sbyte)(Value >> 8));
                            break;
                        //-----------     
                        case OperatorName.hintmask1:
                        case OperatorName.hintmask2:
                        case OperatorName.hintmask3:
                        case OperatorName.hintmask4:
                        case OperatorName.hintmask_bits:
                            s_dbugSb.Append((op_name).ToString() + " " + Convert.ToString(Value, 2));
                            break;
                        default:
                            if (has_ExtenedForm)
                            {
                                s_dbugSb.Append((op_name).ToString());
                            }
                            else
                            {
                                s_dbugSb.Append((op_name).ToString() + " " + Value.ToString());
                            }

                            break;
                    }

                    return s_dbugSb.ToString();

                }

            }
#endif
        }

        #endregion

        #region public class Cff1GlyphData

        public class Cff1GlyphData
        {
            internal Cff1GlyphData()
            {
            }

            public string Name { get; internal set; }
            public ushort SIDName { get; internal set; }
            internal Type2Instruction[] GlyphInstructions { get; set; }

#if DEBUG
            public ushort dbugGlyphIndex { get; internal set; }
            public override string ToString()
            {
                StringBuilder stbuilder = new StringBuilder();
                stbuilder.Append(dbugGlyphIndex);
                if (Name != null)
                {
                    stbuilder.Append(" ");
                    stbuilder.Append(Name);
                }
                return stbuilder.ToString();
            }
#endif
        }

        #endregion

        #region public readonly struct Bounds

        public readonly struct Bounds
        {

            //TODO: will be changed to => public readonly struct Bounds 

            public static readonly Bounds Zero = new Bounds(0, 0, 0, 0);

            public Bounds(short xmin, short ymin, short xmax, short ymax)
            {
                XMin = xmin;
                YMin = ymin;
                XMax = xmax;
                YMax = ymax;
            }

            public short XMin { get; }
            public short YMin { get; }
            public short XMax { get; }
            public short YMax { get; }
#if DEBUG
            public override string ToString()
            {
                return "(" + XMin + "," + YMin + "," + XMax + "," + YMax + ")";
            }
#endif
        }

        #endregion

        #region public struct GlyphPointF

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct GlyphPointF
        {
            //from https://docs.microsoft.com/en-us/typography/opentype/spec/glyf
            //'point' of the glyph contour.
            //eg. ... In the glyf table, the position of a point ...
            //  ...  the point is on the curve; otherwise, it is off the curve....

            internal Vector2 P;
            internal bool onCurve;

            public GlyphPointF(float x, float y, bool onCurve)
            {
                P = new Vector2(x, y);
                this.onCurve = onCurve;
            }
            public GlyphPointF(Vector2 position, bool onCurve)
            {
                P = position;
                this.onCurve = onCurve;
            }
            public float X => this.P.X;
            public float Y => this.P.Y;

            public static GlyphPointF operator *(GlyphPointF p, float n)
            {
                return new GlyphPointF(p.P * n, p.onCurve);
            }

            //-----------------------------------------

            internal GlyphPointF Offset(short dx, short dy) { return new GlyphPointF(new Vector2(P.X + dx, P.Y + dy), onCurve); }

            internal void ApplyScale(float scale)
            {
                P *= scale;
            }
            internal void ApplyScaleOnlyOnXAxis(float scale)
            {
                P = new Vector2(P.X * scale, P.Y);
            }

            internal void UpdateX(float x)
            {
                this.P.X = x;
            }
            internal void UpdateY(float y)
            {
                this.P.Y = y;
            }
            internal void OffsetY(float dy)
            {
                this.P.Y += dy;
            }
            internal void OffsetX(float dx)
            {
                this.P.X += dx;
            }
#if DEBUG
            internal bool dbugIsEqualsWith(GlyphPointF another)
            {
                return this.P == another.P && this.onCurve == another.onCurve;
            }
            public override string ToString() { return P.ToString() + " " + onCurve.ToString(); }
#endif
        }

        #endregion

        #region enum SimpleGlyphFlag : byte

        [Flags]
        internal enum SimpleGlyphFlag : byte
        {
            OnCurve = 1,
            XByte = 1 << 1,
            YByte = 1 << 2,
            Repeat = 1 << 3,
            XSignOrSame = 1 << 4,
            YSignOrSame = 1 << 5
        }

        #endregion

        #region internal enum CompositeGlyphFlags

        [Flags]
        internal enum CompositeGlyphFlags : ushort
        {
            //These are the constants for the flags field:
            //Bit   Flags 	 	            Description
            //0     ARG_1_AND_2_ARE_WORDS  	If this is set, the arguments are words; otherwise, they are bytes.
            //1     ARGS_ARE_XY_VALUES 	  	If this is set, the arguments are xy values; otherwise, they are points.
            //2     ROUND_XY_TO_GRID 	  	For the xy values if the preceding is true.
            //3     WE_HAVE_A_SCALE 	 	This indicates that there is a simple scale for the component. Otherwise, scale = 1.0.
            //4     RESERVED 	        	This bit is reserved. Set it to 0.
            //5     MORE_COMPONENTS 	    Indicates at least one more glyph after this one.
            //6     WE_HAVE_AN_X_AND_Y_SCALE 	The x direction will use a different scale from the y direction.
            //7     WE_HAVE_A_TWO_BY_TWO 	  	There is a 2 by 2 transformation that will be used to scale the component.
            //8     WE_HAVE_INSTRUCTIONS 	 	Following the last component are instructions for the composite character.
            //9     USE_MY_METRICS 	 	        If set, this forces the aw and lsb (and rsb) for the composite to be equal to those from this original glyph. This works for hinted and unhinted characters.
            //10    OVERLAP_COMPOUND 	 	    If set, the components of the compound glyph overlap. Use of this flag is not required in OpenType — that is, it is valid to have components overlap without having this flag set. It may affect behaviors in some platforms, however. (See Apple’s specification for details regarding behavior in Apple platforms.)
            //11    SCALED_COMPONENT_OFFSET 	The composite is designed to have the component offset scaled.
            //12    UNSCALED_COMPONENT_OFFSET 	The composite is designed not to have the component offset scaled.

            ARG_1_AND_2_ARE_WORDS = 1,
            ARGS_ARE_XY_VALUES = 1 << 1,
            ROUND_XY_TO_GRID = 1 << 2,
            WE_HAVE_A_SCALE = 1 << 3,
            RESERVED = 1 << 4,
            MORE_COMPONENTS = 1 << 5,
            WE_HAVE_AN_X_AND_Y_SCALE = 1 << 6,
            WE_HAVE_A_TWO_BY_TWO = 1 << 7,
            WE_HAVE_INSTRUCTIONS = 1 << 8,
            USE_MY_METRICS = 1 << 9,
            OVERLAP_COMPOUND = 1 << 10,
            SCALED_COMPONENT_OFFSET = 1 << 11,
            UNSCALED_COMPONENT_OFFSET = 1 << 12
        }

        #endregion



        static bool HasFlag(SimpleGlyphFlag target, SimpleGlyphFlag test)
        {
            return (target & test) == test;
        }
        internal static bool HasFlag(CompositeGlyphFlags target, CompositeGlyphFlags test)
        {
            return (target & test) == test;
        }

        public class Glyph
        {

            /// <summary>
            /// glyph info has only essential layout detail (this is our extension)
            /// </summary>
            readonly bool _onlyLayoutEssMode;
            bool _hasOrgAdvWidth;       //FOUND in all mode

            internal Glyph(
                GlyphPointF[] glyphPoints,
                ushort[] contourEndPoints,
                Bounds bounds,
                byte[] glyphInstructions,
                ushort index)
            {
                //create from TTF 

#if DEBUG
                this.dbugId = s_debugTotalId++;
                if (this.dbugId == 444)
                {

                }
#endif
                this.GlyphPoints = glyphPoints;
                EndPoints = contourEndPoints;
                Bounds = bounds;
                GlyphInstructions = glyphInstructions;
                GlyphIndex = index;

            }

            public ushort GlyphIndex { get; }                       //FOUND in all mode
            public Bounds Bounds { get; internal set; }             //FOUND in all mode
            public ushort OriginalAdvanceWidth { get; private set; } //FOUND in all mode
            internal ushort BitmapGlyphAdvanceWidth { get; set; }    //FOUND in all mode

            //TrueTypeFont
            public ushort[] EndPoints { get; private set; }         //NULL in  _onlyLayoutEssMode         
            public GlyphPointF[] GlyphPoints { get; private set; }  //NULL in  _onlyLayoutEssMode         
            internal byte[] GlyphInstructions { get; set; }         //NULL in _onlyLayoutEssMode 
            public bool HasGlyphInstructions => this.GlyphInstructions != null; //FALSE  n _onlyLayoutEssMode 

            //
            public GlyphClassKind GlyphClass { get; internal set; } //FOUND in all mode
            internal ushort MarkClassDef { get; set; }              //FOUND in all mode

            public short MinX => Bounds.XMin;
            public short MaxX => Bounds.XMax;
            public short MinY => Bounds.YMin;
            public short MaxY => Bounds.YMax;


            public static bool HasOriginalAdvancedWidth(Glyph glyph) => glyph._hasOrgAdvWidth;
            public static void SetOriginalAdvancedWidth(Glyph glyph, ushort advW)
            {
                glyph.OriginalAdvanceWidth = advW;
                glyph._hasOrgAdvWidth = true;
            }

            /// <summary>
            /// TrueType outline, offset glyph points
            /// </summary>
            /// <param name="glyph"></param>
            /// <param name="dx"></param>
            /// <param name="dy"></param>
            internal static void TtfOffsetXY(Glyph glyph, short dx, short dy)
            {

                //change data on current glyph
                GlyphPointF[] glyphPoints = glyph.GlyphPoints;
                for (int i = glyphPoints.Length - 1; i >= 0; --i)
                {
                    glyphPoints[i] = glyphPoints[i].Offset(dx, dy);
                }
                //-------------------------
                Bounds orgBounds = glyph.Bounds;
                glyph.Bounds = new Bounds(
                   (short)(orgBounds.XMin + dx),
                   (short)(orgBounds.YMin + dy),
                   (short)(orgBounds.XMax + dx),
                   (short)(orgBounds.YMax + dy));

            }

            /// <summary>
            ///TrueType outline, transform normal
            /// </summary>
            /// <param name="glyph"></param>
            /// <param name="m00"></param>
            /// <param name="m01"></param>
            /// <param name="m10"></param>
            /// <param name="m11"></param>
            internal static void TtfTransformWith2x2Matrix(Glyph glyph, float m00, float m01, float m10, float m11)
            {

                //http://stackoverflow.com/questions/13188156/whats-the-different-between-vector2-transform-and-vector2-transformnormal-i
                //http://www.technologicalutopia.com/sourcecode/xnageometry/vector2.cs.htm

                //change data on current glyph
                float new_xmin = 0;
                float new_ymin = 0;
                float new_xmax = 0;
                float new_ymax = 0;

                GlyphPointF[] glyphPoints = glyph.GlyphPoints;
                for (int i = 0; i < glyphPoints.Length; ++i)
                {
                    GlyphPointF p = glyphPoints[i];
                    float x = p.P.X;
                    float y = p.P.Y;

                    float newX, newY;
                    //please note that this is transform normal***
                    glyphPoints[i] = new GlyphPointF(
                       newX = (float)Math.Round((x * m00) + (y * m10)),
                       newY = (float)Math.Round((x * m01) + (y * m11)),
                       p.onCurve);

                    //short newX = xs[i] = (short)Math.Round((x * m00) + (y * m10));
                    //short newY = ys[i] = (short)Math.Round((x * m01) + (y * m11));
                    //------
                    if (newX < new_xmin)
                    {
                        new_xmin = newX;
                    }
                    if (newX > new_xmax)
                    {
                        new_xmax = newX;
                    }
                    //------
                    if (newY < new_ymin)
                    {
                        new_ymin = newY;
                    }
                    if (newY > new_ymax)
                    {
                        new_ymax = newY;
                    }
                }
                //TODO: review here
                glyph.Bounds = new Bounds(
                   (short)new_xmin, (short)new_ymin,
                   (short)new_xmax, (short)new_ymax);
            }

            /// <summary>
            /// TrueType outline glyph clone
            /// </summary>
            /// <param name="original"></param>
            /// <param name="newGlyphIndex"></param>
            /// <returns></returns>
            internal static Glyph TtfOutlineGlyphClone(Glyph original, ushort newGlyphIndex)
            {
                //for true type instruction glyph***
                return new Glyph(
                    Utils.CloneArray(original.GlyphPoints),
                    Utils.CloneArray(original.EndPoints),
                    original.Bounds,
                    original.GlyphInstructions != null ? Utils.CloneArray(original.GlyphInstructions) : null,
                    newGlyphIndex);
            }

            /// <summary>
            /// append data from src to dest, dest data will changed***
            /// </summary>
            /// <param name="src"></param>
            /// <param name="dest"></param>
            internal static void TtfAppendGlyph(Glyph dest, Glyph src)
            {
                int org_dest_len = dest.EndPoints.Length;
#if DEBUG
                int src_contour_count = src.EndPoints.Length;
#endif
                if (org_dest_len == 0)
                {
                    //org is empty glyph

                    dest.GlyphPoints = Utils.ConcatArray(dest.GlyphPoints, src.GlyphPoints);
                    dest.EndPoints = Utils.ConcatArray(dest.EndPoints, src.EndPoints);

                }
                else
                {
                    ushort org_last_point = (ushort)(dest.EndPoints[org_dest_len - 1] + 1); //since start at 0 

                    dest.GlyphPoints = Utils.ConcatArray(dest.GlyphPoints, src.GlyphPoints);
                    dest.EndPoints = Utils.ConcatArray(dest.EndPoints, src.EndPoints);
                    //offset latest append contour  end points
                    int newlen = dest.EndPoints.Length;
                    for (int i = org_dest_len; i < newlen; ++i)
                    {
                        dest.EndPoints[i] += (ushort)org_last_point;
                    }
                }



                //calculate new bounds
                Bounds destBound = dest.Bounds;
                Bounds srcBound = src.Bounds;
                short newXmin = (short)Math.Min(destBound.XMin, srcBound.XMin);
                short newYMin = (short)Math.Min(destBound.YMin, srcBound.YMin);
                short newXMax = (short)Math.Max(destBound.XMax, srcBound.XMax);
                short newYMax = (short)Math.Max(destBound.YMax, srcBound.YMax);

                dest.Bounds = new Bounds(newXmin, newYMin, newXMax, newYMax);
            }

#if DEBUG
            public readonly int dbugId;
            static int s_debugTotalId;
            public override string ToString()
            {
                var stbuilder = new StringBuilder();
                if (IsCffGlyph)
                {
                    stbuilder.Append("cff");
                    stbuilder.Append(",index=" + GlyphIndex);
                    stbuilder.Append(",name=" + _cff1GlyphData.Name);
                }
                else
                {
                    stbuilder.Append("ttf");
                    stbuilder.Append(",index=" + GlyphIndex);
                    stbuilder.Append(",class=" + GlyphClass.ToString());
                    if (MarkClassDef != 0)
                    {
                        stbuilder.Append(",mark_class=" + MarkClassDef);
                    }
                }
                return stbuilder.ToString();
            }
#endif

            //--------------------

            //cff  

            internal readonly Cff1GlyphData _cff1GlyphData;             //NULL in  _onlyLayoutEssMode 

            internal Glyph(Cff1GlyphData cff1Glyph, ushort glyphIndex)
            {
#if DEBUG
                this.dbugId = s_debugTotalId++;
                cff1Glyph.dbugGlyphIndex = glyphIndex;
#endif
                //create from CFF 
                _cff1GlyphData = cff1Glyph;
                this.GlyphIndex = glyphIndex;
            }

            public bool IsCffGlyph => _cff1GlyphData != null;
            public Cff1GlyphData GetCff1GlyphData() => _cff1GlyphData;

            //TODO: review here again
            public MathGlyphInfo MathGlyphInfo { get; internal set; }  //FOUND in all mode (if font has this data)

            uint _streamLen;            //FOUND in all mode (if font has this data)
            ushort _imgFormat;          //FOUND in all mode (if font has this data)
            internal Glyph(ushort glyphIndex, uint streamOffset, uint streamLen, ushort imgFormat)
            {
                //_bmpGlyphSource = bmpGlyphSource;
                BitmapStreamOffset = streamOffset;
                _streamLen = streamLen;
                _imgFormat = imgFormat;
                this.GlyphIndex = glyphIndex;
            }
            internal uint BitmapStreamOffset { get; private set; }
            internal uint BitmapFormat => _imgFormat;

            private Glyph(ushort glyphIndex)
            {
                //for Clone_NO_BuildingInstructions()
                _onlyLayoutEssMode = true;
                GlyphIndex = glyphIndex;
            }

            internal static void CopyExistingGlyphInfo(Glyph src, Glyph dst)
            {
                dst.Bounds = src.Bounds;
                dst._hasOrgAdvWidth = src._hasOrgAdvWidth;
                dst.OriginalAdvanceWidth = src.OriginalAdvanceWidth;
                dst.BitmapGlyphAdvanceWidth = src.BitmapGlyphAdvanceWidth;
                dst.GlyphClass = src.GlyphClass;
                dst.MarkClassDef = src.MarkClassDef;

                //ttf: NO EndPoints, GlyphPoints, HasGlyphInstructions

                //cff:  NO _cff1GlyphData

                //math-font:
                dst.MathGlyphInfo = src.MathGlyphInfo;
                dst.BitmapStreamOffset = src.BitmapStreamOffset;
                dst._streamLen = src._streamLen;
                dst._imgFormat = src._imgFormat;
            }

            internal static Glyph Clone_NO_BuildingInstructions(Glyph src)
            {
                //a new glyph has only detail about glyph layout
                //NO information about glyph building instructions
                //1. if src if ttf
                //2. if src is cff
                //3. if src is svg
                //4. if src is bitmap

                Glyph newclone = new Glyph(src.GlyphIndex);
                CopyExistingGlyphInfo(src, newclone);
                return newclone;
            }
        }


        //https://docs.microsoft.com/en-us/typography/opentype/spec/gdef
        public enum GlyphClassKind : byte
        {
            //1 	Base glyph (single character, spacing glyph)
            //2 	Ligature glyph (multiple character, spacing glyph)
            //3 	Mark glyph (non-spacing combining glyph)
            //4 	Component glyph (part of single character, spacing glyph)
            //
            // The font developer does not have to classify every glyph in the font, 
            //but any glyph not assigned a class value falls into Class zero (0). 
            //For instance, class values might be useful for the Arabic glyphs in a font, but not for the Latin glyphs. 
            //Then the GlyphClassDef table will list only Arabic glyphs, and-by default-the Latin glyphs will be assigned to Class 0. 
            //Component glyphs can be put together to generate ligatures. 
            //A ligature can be generated by creating a glyph in the font that references the component glyphs, 
            //or outputting the component glyphs in the desired sequence. 
            //Component glyphs are not used in defining any GSUB or GPOS formats.
            //
            Zero = 0,//class0, classZero
            Base,
            Ligature,
            Mark,
            Component
        }


        class TransformedGlyf
        {

            static TripleEncodingTable s_encTable = TripleEncodingTable.GetEncTable();

            public TransformedGlyf(TTFDirectory tableDir)
            {
                
                TableDir = tableDir;
            }
            public TTFDirectory TableDir { get; }

            public T CreateTableEntry<T>(BigEndianReader reader) where T : WOFF2Glyph
            {
                WOFF2Glyph glyfTable = new WOFF2Glyph(this.TableDir.Offset);
                ReconstructGlyfTable(reader, TableDir, glyfTable);

                return (T)glyfTable;
            }


            struct TempGlyph
            {
                public readonly ushort glyphIndex;
                public readonly short numContour;

                public ushort instructionLen;
                public bool compositeHasInstructions;
                public TempGlyph(ushort glyphIndex, short contourCount)
                {
                    this.glyphIndex = glyphIndex;
                    this.numContour = contourCount;

                    instructionLen = 0;
                    compositeHasInstructions = false;
                }
#if DEBUG
                public override string ToString()
                {
                    return glyphIndex + " " + numContour;
                }
#endif
            }


            static void ReconstructGlyfTable(BigEndianReader reader, TTFDirectory woff2TableDir, WOFF2Glyph glyfTable)
            {
                //fill the information to glyfTable 
                //reader.BaseStream.Position += woff2TableDir.transformLength;
                //For greater compression effectiveness,
                //the glyf table is split into several substreams, to group like data together. 

                //The transformed table consists of a number of fields specifying the size of each of the substreams,
                //followed by the substreams in sequence.

                //During the decoding process the reverse transformation takes place,
                //where data from various separate substreams are recombined to create a complete glyph record
                //for each entry of the original glyf table.

                //Transformed glyf Table
                //Data-Type Semantic                Description and value type(if applicable)
                //Fixed     version                 = 0x00000000
                //UInt16    numGlyphs               Number of glyphs
                //UInt16    indexFormatOffset      format for loca table, 
                //                                 should be consistent with indexToLocFormat of 
                //                                 the original head table(see[OFF] specification)

                //UInt32    nContourStreamSize      Size of nContour stream in bytes
                //UInt32    nPointsStreamSize       Size of nPoints stream in bytes
                //UInt32    flagStreamSize          Size of flag stream in bytes
                //UInt32    glyphStreamSize         Size of glyph stream in bytes(a stream of variable-length encoded values, see description below)
                //UInt32    compositeStreamSize     Size of composite stream in bytes(a stream of variable-length encoded values, see description below)
                //UInt32    bboxStreamSize          Size of bbox data in bytes representing combined length of bboxBitmap(a packed bit array) and bboxStream(a stream of Int16 values)
                //UInt32    instructionStreamSize   Size of instruction stream(a stream of UInt8 values)

                //Int16     nContourStream[]        Stream of Int16 values representing number of contours for each glyph record
                //255UInt16 nPointsStream[]         Stream of values representing number of outline points for each contour in glyph records
                //UInt8     flagStream[]            Stream of UInt8 values representing flag values for each outline point.
                //Vary      glyphStream[]           Stream of bytes representing point coordinate values using variable length encoding format(defined in subclause 5.2)
                //Vary      compositeStream[]       Stream of bytes representing component flag values and associated composite glyph data
                //UInt8     bboxBitmap[]            Bitmap(a numGlyphs-long bit array) indicating explicit bounding boxes
                //Int16     bboxStream[]            Stream of Int16 values representing glyph bounding box data
                //UInt8     instructionStream[]	    Stream of UInt8 values representing a set of instructions for each corresponding glyph

                reader.BaseStream.Position = woff2TableDir.Offset;

                long start = reader.BaseStream.Position;

                uint version = reader.ReadUInt32();
                ushort numGlyphs = reader.ReadUInt16();
                ushort indexFormatOffset = reader.ReadUInt16();

                uint nContourStreamSize = reader.ReadUInt32(); //in bytes
                uint nPointsStreamSize = reader.ReadUInt32(); //in bytes
                uint flagStreamSize = reader.ReadUInt32(); //in bytes
                uint glyphStreamSize = reader.ReadUInt32(); //in bytes
                uint compositeStreamSize = reader.ReadUInt32(); //in bytes
                uint bboxStreamSize = reader.ReadUInt32(); //in bytes
                uint instructionStreamSize = reader.ReadUInt32(); //in bytes


                long expected_nCountStartAt = reader.BaseStream.Position;
                long expected_nPointStartAt = expected_nCountStartAt + nContourStreamSize;
                long expected_FlagStreamStartAt = expected_nPointStartAt + nPointsStreamSize;
                long expected_GlyphStreamStartAt = expected_FlagStreamStartAt + flagStreamSize;
                long expected_CompositeStreamStartAt = expected_GlyphStreamStartAt + glyphStreamSize;

                long expected_BboxStreamStartAt = expected_CompositeStreamStartAt + compositeStreamSize;
                long expected_InstructionStreamStartAt = expected_BboxStreamStartAt + bboxStreamSize;
                long expected_EndAt = expected_InstructionStreamStartAt + instructionStreamSize;

                //--------------------------------------------- 
                Glyph[] glyphs = new Glyph[numGlyphs];
                TempGlyph[] allGlyphs = new TempGlyph[numGlyphs];
                List<ushort> compositeGlyphs = new List<ushort>();
                int contourCount = 0;
                for (ushort i = 0; i < numGlyphs; ++i)
                {
                    short numContour = reader.ReadInt16();
                    allGlyphs[i] = new TempGlyph(i, numContour);
                    if (numContour > 0)
                    {
                        contourCount += numContour;
                        //>0 => simple glyph
                        //-1 = compound
                        //0 = empty glyph
                    }
                    else if (numContour < 0)
                    {
                        //composite glyph, resolve later
                        compositeGlyphs.Add(i);
                    }
                    else
                    {

                    }
                }

                //--------------------------------------------------------------------------------------------
                //glyphStream 
                //5.2.Decoding of variable-length X and Y coordinates

                //Simple glyph data structure defines all contours that comprise a glyph outline,
                //which are presented by a sequence of on- and off-curve coordinate points. 

                //These point coordinates are encoded as delta values representing the incremental values 
                //between the previous and current corresponding X and Y coordinates of a point,
                //the first point of each outline is relative to (0, 0) point.

                //To minimize the size of the dataset of point coordinate values, 
                //each point is presented as a (flag, xCoordinate, yCoordinate) triplet.

                //The flag value is stored in a separate data stream 
                //and the coordinate values are stored as part of the glyph data stream using a variable-length encoding format
                //consuming a total of 2 - 5 bytes per point.

                //Decoding of Simple Glyphs:

                //For a simple glyph(when nContour > 0), the process continues as follows:
                //    1) Read numberOfContours 255UInt16 values from the nPoints stream.
                //    Each of these is the number of points of that contour.
                //    Convert this into the endPtsOfContours[] array by computing the cumulative sum, then subtracting one.
                //    For example, if the values in the stream are[2, 4], then the endPtsOfContours array is [1, 5].Also,
                //      the sum of all the values in the array is the total number of points in the glyph, nPoints.
                //      In the example given, the value of nPoints is 6.

                //    2) Read nPoints UInt8 values from the flags stream.Each corresponds to one point in the reconstructed glyph outline.
                //       The interpretation of the flag byte is described in details in subclause 5.2.

                //    3) For each point(i.e.nPoints times), read a number of point coordinate bytes from the glyph stream.
                //       The number of point coordinate bytes is a function of the flag byte read in the previous step: 
                //       for (flag < 0x7f) in the range 0 to 83 inclusive, it is one byte.
                //       In the range 84 to 119 inclusive, it is two bytes. 
                //       In the range 120 to 123 inclusive, it is three bytes, 
                //       and in the range 124 to 127 inclusive, it is four bytes. 
                //       Decode these bytes according to the procedure specified in the subclause 5.2 to reconstruct delta-x and delta-y values of the glyph point coordinates.
                //       Store these delta-x and delta-y values in the reconstructed glyph using the standard TrueType glyph encoding[OFF] subclause 5.3.3.

                //    4) Read one 255UInt16 value from the glyph stream, which is instructionLength, the number of instruction bytes.
                //    5) Read instructionLength bytes from instructionStream, and store these in the reconstituted glyph as instructions.
                //--------
#if DEBUG
                if (reader.BaseStream.Position != expected_nPointStartAt)
                {
                    System.Diagnostics.Debug.WriteLine("ERR!!");
                }
#endif
                //
                //1) nPoints stream,  npoint for each contour

                ushort[] pntPerContours = new ushort[contourCount];
                for (int i = 0; i < contourCount; ++i)
                {
                    // Each of these is the number of points of that contour.
                    pntPerContours[i] = Read255UInt16(reader);
                }
#if DEBUG
                if (reader.BaseStream.Position != expected_FlagStreamStartAt)
                {
                    System.Diagnostics.Debug.WriteLine("ERR!!");
                }
#endif
                //2) flagStream, flags value for each point
                //each byte in flags stream represents one point
                byte[] flagStream = reader.Read((int)flagStreamSize);

#if DEBUG
                if (reader.BaseStream.Position != expected_GlyphStreamStartAt)
                {
                    System.Diagnostics.Debug.WriteLine("ERR!!");
                }
#endif


                //***
                //some composite glyphs have instructions=> so we must check all composite glyphs
                //before read the glyph stream
                //** 
                using (MemoryStream compositeMS = new MemoryStream())
                {
                    reader.BaseStream.Position = expected_CompositeStreamStartAt;
                    compositeMS.Write(reader.Read((int)compositeStreamSize), 0, (int)compositeStreamSize);
                    compositeMS.Position = 0;

                    int j = compositeGlyphs.Count;
                    BigEndianReader compositeReader = new BigEndianReader(compositeMS);
                    for (ushort i = 0; i < j; ++i)
                    {
                        ushort compositeGlyphIndex = compositeGlyphs[i];
                        allGlyphs[compositeGlyphIndex].compositeHasInstructions = CompositeHasInstructions(compositeReader, compositeGlyphIndex);
                    }
                    reader.BaseStream.Position = expected_GlyphStreamStartAt;
                }
                //-------- 
                int curFlagsIndex = 0;
                int pntContourIndex = 0;
                for (int i = 0; i < allGlyphs.Length; ++i)
                {
                    glyphs[i] = BuildSimpleGlyphStructure(reader,
                        ref allGlyphs[i],
                        glyfTable._emptyGlyph,
                        pntPerContours, ref pntContourIndex,
                        flagStream, ref curFlagsIndex);
                }

#if DEBUG
                if (pntContourIndex != pntPerContours.Length)
                {

                }
                if (curFlagsIndex != flagStream.Length)
                {

                }
#endif
                //--------------------------------------------------------------------------------------------
                //compositeStream
                //--------------------------------------------------------------------------------------------
#if DEBUG
                if (expected_CompositeStreamStartAt != reader.BaseStream.Position)
                {
                    //***

                    reader.BaseStream.Position = expected_CompositeStreamStartAt;
                }
#endif
                {
                    //now we read the composite stream again
                    //and create composite glyphs
                    int j = compositeGlyphs.Count;
                    for (ushort i = 0; i < j; ++i)
                    {
                        int compositeGlyphIndex = compositeGlyphs[i];
                        glyphs[compositeGlyphIndex] = ReadCompositeGlyph(glyphs, reader, i, glyfTable._emptyGlyph);
                    }
                }

                //--------------------------------------------------------------------------------------------
                //bbox stream
                //--------------------------------------------------------------------------------------------

                //Finally, for both simple and composite glyphs,
                //if the corresponding bit in the bounding box bit vector is set, 
                //then additionally read 4 Int16 values from the bbox stream, 
                //representing xMin, yMin, xMax, and yMax, respectively, 
                //and record these into the corresponding fields of the reconstructed glyph.
                //For simple glyphs, if the corresponding bit in the bounding box bit vector is not set,
                //then derive the bounding box by computing the minimum and maximum x and y coordinates in the outline, and storing that.

                //A composite glyph MUST have an explicitly supplied bounding box. 
                //The motivation is that computing bounding boxes is more complicated,
                //and would require resolving references to component glyphs taking into account composite glyph instructions and
                //the specified scales of individual components, which would conflict with a purely streaming implementation of font decoding.

                //A decoder MUST check for presence of the bounding box info as part of the composite glyph record 
                //and MUST NOT load a font file with the composite bounding box data missing. 
#if DEBUG
                if (expected_BboxStreamStartAt != reader.BaseStream.Position)
                {

                }
#endif
                int bitmapCount = (numGlyphs + 7) / 8;
                byte[] bboxBitmap = ExpandBitmap(reader.Read(bitmapCount));
                for (ushort i = 0; i < numGlyphs; ++i)
                {
                    TempGlyph tempGlyph = allGlyphs[i];
                    Glyph glyph = glyphs[i];

                    byte hasBbox = bboxBitmap[i];
                    if (hasBbox == 1)
                    {
                        //read bbox from the bboxstream
                        glyph.Bounds = new Bounds(
                            reader.ReadInt16(),
                            reader.ReadInt16(),
                            reader.ReadInt16(),
                            reader.ReadInt16());
                    }
                    else
                    {
                        //no bbox
                        //
                        if (tempGlyph.numContour < 0)
                        {
                            //composite must have bbox
                            //if not=> err
                            throw new System.NotSupportedException();
                        }
                        else if (tempGlyph.numContour > 0)
                        {
                            //simple glyph
                            //use simple calculation
                            //...For simple glyphs, if the corresponding bit in the bounding box bit vector is not set,
                            //then derive the bounding box by computing the minimum and maximum x and y coordinates in the outline, and storing that.
                            glyph.Bounds = FindSimpleGlyphBounds(glyph);
                        }
                    }
                }
                //--------------------------------------------------------------------------------------------
                //instruction stream
#if DEBUG
                if (reader.BaseStream.Position < expected_InstructionStreamStartAt)
                {

                }
                else if (expected_InstructionStreamStartAt == reader.BaseStream.Position)
                {

                }
                else
                {

                }
#endif

                reader.BaseStream.Position = expected_InstructionStreamStartAt;
                //--------------------------------------------------------------------------------------------

                for (ushort i = 0; i < numGlyphs; ++i)
                {
                    TempGlyph tempGlyph = allGlyphs[i];
                    if (tempGlyph.instructionLen > 0)
                    {
                        glyphs[i].GlyphInstructions = reader.Read(tempGlyph.instructionLen);
                    }
                }

#if DEBUG
                if (reader.BaseStream.Position != expected_EndAt)
                {

                }
#endif

                glyfTable.Glyphs = glyphs;
            }

            static Bounds FindSimpleGlyphBounds(Glyph glyph)
            {
                GlyphPointF[] glyphPoints = glyph.GlyphPoints;

                int j = glyphPoints.Length;
                float xmin = float.MaxValue;
                float ymin = float.MaxValue;
                float xmax = float.MinValue;
                float ymax = float.MinValue;

                for (int i = 0; i < j; ++i)
                {
                    GlyphPointF p = glyphPoints[i];
                    if (p.X < xmin) xmin = p.X;
                    if (p.X > xmax) xmax = p.X;
                    if (p.Y < ymin) ymin = p.Y;
                    if (p.Y > ymax) ymax = p.Y;
                }

                return new Bounds(
                   (short)System.Math.Round(xmin),
                   (short)System.Math.Round(ymin),
                   (short)System.Math.Round(xmax),
                   (short)System.Math.Round(ymax));
            }

            const byte ONE_MORE_BYTE_CODE1 = 255;
            const byte ONE_MORE_BYTE_CODE2 = 254;
            const byte WORD_CODE = 253;
            const byte LOWEST_UCODE = 253;

            public static ushort Read255UInt16(BigEndianReader reader)
            {
                //255UInt16 Variable-length encoding of a 16-bit unsigned integer for optimized intermediate font data storage.
                //255UInt16 Data Type
                //255UInt16 is a variable-length encoding of an unsigned integer 
                //in the range 0 to 65535 inclusive.
                //This data type is intended to be used as intermediate representation of various font values,
                //which are typically expressed as UInt16 but represent relatively small values.
                //Depending on the encoded value, the length of the data field may be one to three bytes,
                //where the value of the first byte either represents the small value itself or is treated as a code that defines the format of the additional byte(s).
                //The "C-like" pseudo-code describing how to read the 255UInt16 format is presented below:
                //   Read255UShort(data )
                //    {
                //                UInt8 code;
                //                UInt16 value, value2;

                //                const oneMoreByteCode1    = 255;
                //                const oneMoreByteCode2    = 254;
                //                const wordCode            = 253;
                //                const lowestUCode         = 253;

                //                code = data.getNextUInt8();
                //                if (code == wordCode)
                //                {
                //                    /* Read two more bytes and concatenate them to form UInt16 value*/
                //                    value = data.getNextUInt8();
                //                    value <<= 8;
                //                    value &= 0xff00;
                //                    value2 = data.getNextUInt8();
                //                    value |= value2 & 0x00ff;
                //                }
                //                else if (code == oneMoreByteCode1)
                //                {
                //                    value = data.getNextUInt8();
                //                    value = (value + lowestUCode);
                //                }
                //                else if (code == oneMoreByteCode2)
                //                {
                //                    value = data.getNextUInt8();
                //                    value = (value + lowestUCode * 2);
                //                }
                //                else
                //                {
                //                    value = code;
                //                }
                //                return value;
                //            } 
                //Note that the encoding is not unique.For example, 
                //the value 506 can be encoded as [255, 253], [254, 0], and[253, 1, 250]. 
                //An encoder may produce any of these, and a decoder MUST accept them all.An encoder should choose shorter encodings,
                //and must be consistent in choice of encoding for the same value, as this will tend to compress better.



                byte code = reader.ReadByte();
                if (code == WORD_CODE)
                {
                    /* Read two more bytes and concatenate them to form UInt16 value*/
                    //int value = (reader.ReadByte() << 8) & 0xff00;
                    //int value2 = reader.ReadByte();
                    //return (ushort)(value | (value2 & 0xff));
                    int value = reader.ReadByte();
                    value <<= 8;
                    value &= 0xff00;
                    int value2 = reader.ReadByte();
                    value |= value2 & 0x00ff;

                    return (ushort)value;
                }
                else if (code == ONE_MORE_BYTE_CODE1)
                {
                    return (ushort)(reader.ReadByte() + LOWEST_UCODE);
                }
                else if (code == ONE_MORE_BYTE_CODE2)
                {
                    return (ushort)(reader.ReadByte() + (LOWEST_UCODE * 2));
                }
                else
                {
                    return code;
                }
            }

            static byte[] ExpandBitmap(byte[] orgBBoxBitmap)
            {
                byte[] expandArr = new byte[orgBBoxBitmap.Length * 8];

                int index = 0;
                for (int i = 0; i < orgBBoxBitmap.Length; ++i)
                {
                    byte b = orgBBoxBitmap[i];
                    expandArr[index++] = (byte)((b >> 7) & 0x1);
                    expandArr[index++] = (byte)((b >> 6) & 0x1);
                    expandArr[index++] = (byte)((b >> 5) & 0x1);
                    expandArr[index++] = (byte)((b >> 4) & 0x1);
                    expandArr[index++] = (byte)((b >> 3) & 0x1);
                    expandArr[index++] = (byte)((b >> 2) & 0x1);
                    expandArr[index++] = (byte)((b >> 1) & 0x1);
                    expandArr[index++] = (byte)((b >> 0) & 0x1);
                }
                return expandArr;
            }

            static Glyph BuildSimpleGlyphStructure(BigEndianReader glyphStreamReader,
                ref TempGlyph tmpGlyph,
                Glyph emptyGlyph,
                ushort[] pntPerContours, ref int pntContourIndex,
                byte[] flagStream, ref int flagStreamIndex)
            {

                //reading from glyphstream*** 
                //Building a SimpleGlyph 
                //    1) Read numberOfContours 255UInt16 values from the nPoints stream.
                //    Each of these is the number of points of that contour.
                //    Convert this into the endPtsOfContours[] array by computing the cumulative sum, then subtracting one.
                //    For example, if the values in the stream are[2, 4], then the endPtsOfContours array is [1, 5].Also,
                //      the sum of all the values in the array is the total number of points in the glyph, nPoints.
                //      In the example given, the value of nPoints is 6.

                //    2) Read nPoints UInt8 values from the flags stream.Each corresponds to one point in the reconstructed glyph outline.
                //       The interpretation of the flag byte is described in details in subclause 5.2.

                //    3) For each point(i.e.nPoints times), read a number of point coordinate bytes from the glyph stream.
                //       The number of point coordinate bytes is a function of the flag byte read in the previous step: 
                //       for (flag < 0x7f)
                //       in the range 0 to 83 inclusive, it is one byte.
                //       In the range 84 to 119 inclusive, it is two bytes. 
                //       In the range 120 to 123 inclusive, it is three bytes, 
                //       and in the range 124 to 127 inclusive, it is four bytes. 
                //       Decode these bytes according to the procedure specified in the subclause 5.2 to reconstruct delta-x and delta-y values of the glyph point coordinates.
                //       Store these delta-x and delta-y values in the reconstructed glyph using the standard TrueType glyph encoding[OFF] subclause 5.3.3.

                //    4) Read one 255UInt16 value from the glyph stream, which is instructionLength, the number of instruction bytes.
                //    5) Read instructionLength bytes from instructionStream, and store these in the reconstituted glyph as instructions. 


                if (tmpGlyph.numContour == 0) return emptyGlyph;
                if (tmpGlyph.numContour < 0)
                {
                    //composite glyph,
                    //check if this has instruction or not
                    if (tmpGlyph.compositeHasInstructions)
                    {
                        tmpGlyph.instructionLen = Read255UInt16(glyphStreamReader);
                    }
                    return null;//skip composite glyph (resolve later)     
                }

                //-----
                int curX = 0;
                int curY = 0;

                int numContour = tmpGlyph.numContour;

                var _endContours = new ushort[numContour];
                ushort pointCount = 0;

                //create contours
                for (ushort i = 0; i < numContour; ++i)
                {
                    ushort numPoint = pntPerContours[pntContourIndex++];//increament pntContourIndex AFTER
                    pointCount += numPoint;
                    _endContours[i] = (ushort)(pointCount - 1);
                }

                //collect point for our contours
                var _glyphPoints = new GlyphPointF[pointCount];
                int n = 0;
                for (int i = 0; i < numContour; ++i)
                {
                    //read point detail
                    //step 3) 

                    //foreach contour
                    //read 1 byte flags for each contour

                    //1) The most significant bit of a flag indicates whether the point is on- or off-curve point,
                    //2) the remaining seven bits of the flag determine the format of X and Y coordinate values and 
                    //specify 128 possible combinations of indices that have been assigned taking into consideration 
                    //typical statistical distribution of data found in TrueType fonts. 

                    //When X and Y coordinate values are recorded using nibbles(either 4 bits per coordinate or 12 bits per coordinate)
                    //the bits are packed in the byte stream with most significant bit of X coordinate first, 
                    //followed by the value for Y coordinate (most significant bit first). 
                    //As a result, the size of the glyph dataset is significantly reduced, 
                    //and the grouping of the similar values(flags, coordinates) in separate and contiguous data streams allows 
                    //more efficient application of the entropy coding applied as the second stage of encoding process. 

                    int endContour = _endContours[i];
                    for (; n <= endContour; ++n)
                    {

                        byte f = flagStream[flagStreamIndex++]; //increment the flagStreamIndex AFTER read

                        //int f1 = (f >> 7); // most significant 1 bit -> on/off curve

                        int xyFormat = f & 0x7F; // remainging 7 bits x,y format  

                        TripleEncodingRecord enc = s_encTable[xyFormat]; //0-128 

                        byte[] packedXY = glyphStreamReader.Read(enc.ByteCount - 1); //byte count include 1 byte flags, so actual read=> byteCount-1
                                                                                          //read x and y 

                        int x = 0;
                        int y = 0;

                        switch (enc.XBits)
                        {
                            default:
                                throw new System.NotSupportedException();//???
                            case 0: //0,8, 
                                x = 0;
                                y = enc.Ty(packedXY[0]);
                                break;
                            case 4: //4,4
                                x = enc.Tx(packedXY[0] >> 4);
                                y = enc.Ty(packedXY[0] & 0xF);
                                break;
                            case 8: //8,0 or 8,8
                                x = enc.Tx(packedXY[0]);
                                y = (enc.YBits == 8) ?
                                        enc.Ty(packedXY[1]) :
                                        0;
                                break;
                            case 12: //12,12
                                     //x = enc.Tx((packedXY[0] << 8) | (packedXY[1] >> 4));
                                     //y = enc.Ty(((packedXY[1] & 0xF)) | (packedXY[2] >> 4));
                                x = enc.Tx((packedXY[0] << 4) | (packedXY[1] >> 4));
                                y = enc.Ty(((packedXY[1] & 0xF) << 8) | (packedXY[2]));
                                break;
                            case 16: //16,16
                                x = enc.Tx((packedXY[0] << 8) | packedXY[1]);
                                y = enc.Ty((packedXY[2] << 8) | packedXY[3]);
                                break;
                        }

                        //incremental point format***
                        _glyphPoints[n] = new GlyphPointF(curX += x, curY += y, (f >> 7) == 0); // most significant 1 bit -> on/off curve 
                    }
                }

                //----
                //step 4) Read one 255UInt16 value from the glyph stream, which is instructionLength, the number of instruction bytes.
                tmpGlyph.instructionLen = Read255UInt16(glyphStreamReader);
                //step 5) resolve it later

                return new Glyph(_glyphPoints,
                   _endContours,
                   new Bounds(), //calculate later
                   null,  //load instruction later
                   tmpGlyph.glyphIndex);
            }

            static bool CompositeHasInstructions(BigEndianReader reader, ushort compositeGlyphIndex)
            {

                //To find if a composite has instruction or not.

                //This method is similar to  ReadCompositeGlyph() (below)
                //but this dose not create actual composite glyph.

                WOFF2Glyph.CompositeGlyphFlags flags;
                do
                {
                    flags = (CompositeGlyphFlags)reader.ReadUInt16();
                    ushort glyphIndex = reader.ReadUInt16();
                    short arg1 = 0;
                    short arg2 = 0;
                    ushort arg1and2 = 0;

                    if (HasFlag(flags, CompositeGlyphFlags.ARG_1_AND_2_ARE_WORDS))
                    {
                        arg1 = reader.ReadInt16();
                        arg2 = reader.ReadInt16();
                    }
                    else
                    {
                        arg1and2 = reader.ReadUInt16();
                    }
                    //-----------------------------------------
                    float xscale = 1;
                    float scale01 = 0;
                    float scale10 = 0;
                    float yscale = 1;

                    bool useMatrix = false;
                    //-----------------------------------------
                    bool hasScale = false;
                    if (HasFlag(flags, CompositeGlyphFlags.WE_HAVE_A_SCALE))
                    {
                        //If the bit WE_HAVE_A_SCALE is set,
                        //the scale value is read in 2.14 format-the value can be between -2 to almost +2.
                        //The glyph will be scaled by this value before grid-fitting. 
                        xscale = yscale = reader.ReadF2Dot14(); /* Format 2.14 */
                        hasScale = true;
                    }
                    else if (HasFlag(flags, CompositeGlyphFlags.WE_HAVE_AN_X_AND_Y_SCALE))
                    {
                        xscale = reader.ReadF2Dot14(); /* Format 2.14 */
                        yscale = reader.ReadF2Dot14(); /* Format 2.14 */
                        hasScale = true;
                    }
                    else if (HasFlag(flags, CompositeGlyphFlags.WE_HAVE_A_TWO_BY_TWO))
                    {

                        //The bit WE_HAVE_A_TWO_BY_TWO allows for linear transformation of the X and Y coordinates by specifying a 2 × 2 matrix.
                        //This could be used for scaling and 90-degree*** rotations of the glyph components, for example.

                        //2x2 matrix

                        //The purpose of USE_MY_METRICS is to force the lsb and rsb to take on a desired value.
                        //For example, an i-circumflex (U+00EF) is often composed of the circumflex and a dotless-i. 
                        //In order to force the composite to have the same metrics as the dotless-i,
                        //set USE_MY_METRICS for the dotless-i component of the composite. 
                        //Without this bit, the rsb and lsb would be calculated from the hmtx entry for the composite 
                        //(or would need to be explicitly set with TrueType instructions).

                        //Note that the behavior of the USE_MY_METRICS operation is undefined for rotated composite components. 
                        useMatrix = true;
                        hasScale = true;
                        xscale = reader.ReadF2Dot14(); /* Format 2.14 */
                        scale01 = reader.ReadF2Dot14(); /* Format 2.14 */
                        scale10 = reader.ReadF2Dot14();/* Format 2.14 */
                        yscale = reader.ReadF2Dot14(); /* Format 2.14 */

                    }

                } while (HasFlag(flags, CompositeGlyphFlags.MORE_COMPONENTS));

                //
                return HasFlag(flags, CompositeGlyphFlags.WE_HAVE_INSTRUCTIONS);
            }

            

            static Glyph ReadCompositeGlyph(Glyph[] createdGlyphs, BigEndianReader reader, ushort compositeGlyphIndex, Glyph emptyGlyph)
            {

                //Decoding of Composite Glyphs
                //For a composite glyph(nContour == -1), the following steps take the place of (Building Simple Glyph, steps 1 - 5 above):

                //1a.Read a UInt16 from compositeStream.
                //  This is interpreted as a component flag word as in the TrueType spec.
                //  Based on the flag values, there are between 4 and 14 additional argument bytes,
                //  interpreted as glyph index, arg1, arg2, and optional scale or affine matrix.

                //2a.Read the number of argument bytes as determined in step 2a from the composite stream, 
                //and store these in the reconstructed glyph.
                //If the flag word read in step 2a has the FLAG_MORE_COMPONENTS bit(bit 5) set, go back to step 2a.

                //3a.If any of the flag words had the FLAG_WE_HAVE_INSTRUCTIONS bit(bit 8) set,
                //then read the instructions from the glyph and store them in the reconstructed glyph, 
                //using the same process as described in steps 4 and 5 above (see Building Simple Glyph).



                Glyph finalGlyph = null;
                CompositeGlyphFlags flags;
                do
                {
                    flags = (CompositeGlyphFlags)reader.ReadUInt16();
                    ushort glyphIndex = reader.ReadUInt16();
                    if (createdGlyphs[glyphIndex] == null)
                    {
                        // This glyph is not read yet, resolve it first!
                        long storedOffset = reader.BaseStream.Position;
                        Glyph missingGlyph = ReadCompositeGlyph(createdGlyphs, reader, glyphIndex, emptyGlyph);
                        createdGlyphs[glyphIndex] = missingGlyph;
                        reader.BaseStream.Position = storedOffset;
                    }

                    Glyph newGlyph = Glyph.TtfOutlineGlyphClone(createdGlyphs[glyphIndex], compositeGlyphIndex);

                    short arg1 = 0;
                    short arg2 = 0;
                    ushort arg1and2 = 0;

                    if (WOFF2Glyph.HasFlag(flags, CompositeGlyphFlags.ARG_1_AND_2_ARE_WORDS))
                    {
                        arg1 = reader.ReadInt16();
                        arg2 = reader.ReadInt16();
                    }
                    else
                    {
                        arg1and2 = reader.ReadUInt16();
                    }
                    //-----------------------------------------
                    float xscale = 1;
                    float scale01 = 0;
                    float scale10 = 0;
                    float yscale = 1;

                    bool useMatrix = false;
                    //-----------------------------------------
                    bool hasScale = false;
                    if (WOFF2Glyph.HasFlag(flags, CompositeGlyphFlags.WE_HAVE_A_SCALE))
                    {
                        //If the bit WE_HAVE_A_SCALE is set,
                        //the scale value is read in 2.14 format-the value can be between -2 to almost +2.
                        //The glyph will be scaled by this value before grid-fitting. 
                        xscale = yscale = reader.ReadF2Dot14(); /* Format 2.14 */
                        hasScale = true;
                    }
                    else if (WOFF2Glyph.HasFlag(flags, CompositeGlyphFlags.WE_HAVE_AN_X_AND_Y_SCALE))
                    {
                        xscale = reader.ReadF2Dot14(); /* Format 2.14 */
                        yscale = reader.ReadF2Dot14(); /* Format 2.14 */
                        hasScale = true;
                    }
                    else if (HasFlag(flags, CompositeGlyphFlags.WE_HAVE_A_TWO_BY_TWO))
                    {

                        //The bit WE_HAVE_A_TWO_BY_TWO allows for linear transformation of the X and Y coordinates by specifying a 2 × 2 matrix.
                        //This could be used for scaling and 90-degree*** rotations of the glyph components, for example.

                        //2x2 matrix

                        //The purpose of USE_MY_METRICS is to force the lsb and rsb to take on a desired value.
                        //For example, an i-circumflex (U+00EF) is often composed of the circumflex and a dotless-i. 
                        //In order to force the composite to have the same metrics as the dotless-i,
                        //set USE_MY_METRICS for the dotless-i component of the composite. 
                        //Without this bit, the rsb and lsb would be calculated from the hmtx entry for the composite 
                        //(or would need to be explicitly set with TrueType instructions).

                        //Note that the behavior of the USE_MY_METRICS operation is undefined for rotated composite components. 
                        useMatrix = true;
                        hasScale = true;
                        xscale = reader.ReadF2Dot14(); /* Format 2.14 */
                        scale01 = reader.ReadF2Dot14(); /* Format 2.14 */
                        scale10 = reader.ReadF2Dot14(); /* Format 2.14 */
                        yscale = reader.ReadF2Dot14(); /* Format 2.14 */

                        if (HasFlag(flags, CompositeGlyphFlags.UNSCALED_COMPONENT_OFFSET))
                        {


                        }
                        else
                        {


                        }
                        if (HasFlag(flags, CompositeGlyphFlags.USE_MY_METRICS))
                        {

                        }
                    }

                    //--------------------------------------------------------------------
                    if (HasFlag(flags, CompositeGlyphFlags.ARGS_ARE_XY_VALUES))
                    {
                        //Argument1 and argument2 can be either x and y offsets to be added to the glyph or two point numbers.  
                        //x and y offsets to be added to the glyph
                        //When arguments 1 and 2 are an x and a y offset instead of points and the bit ROUND_XY_TO_GRID is set to 1,
                        //the values are rounded to those of the closest grid lines before they are added to the glyph.
                        //X and Y offsets are described in FUnits. 

                        if (useMatrix)
                        {
                            //use this matrix  
                            Glyph.TtfTransformWith2x2Matrix(newGlyph, xscale, scale01, scale10, yscale);
                            Glyph.TtfOffsetXY(newGlyph, arg1, arg2);
                        }
                        else
                        {
                            if (hasScale)
                            {
                                if (xscale == 1.0 && yscale == 1.0)
                                {

                                }
                                else
                                {
                                    Glyph.TtfTransformWith2x2Matrix(newGlyph, xscale, 0, 0, yscale);
                                }
                                Glyph.TtfOffsetXY(newGlyph, arg1, arg2);
                            }
                            else
                            {
                                if (HasFlag(flags, CompositeGlyphFlags.ROUND_XY_TO_GRID))
                                {
                                    //TODO: implement round xy to grid***
                                    //----------------------------
                                }
                                //just offset***
                                Glyph.TtfOffsetXY(newGlyph, arg1, arg2);
                            }
                        }


                    }
                    else
                    {
                        //two point numbers. 
                        //the first point number indicates the point that is to be matched to the new glyph. 
                        //The second number indicates the new glyph's “matched” point. 
                        //Once a glyph is added,its point numbers begin directly after the last glyphs (endpoint of first glyph + 1)

                    }

                    //
                    if (finalGlyph == null)
                    {
                        finalGlyph = newGlyph;
                    }
                    else
                    {
                        //merge 
                        Glyph.TtfAppendGlyph(finalGlyph, newGlyph);
                    }

                } while (HasFlag(flags, CompositeGlyphFlags.MORE_COMPONENTS));

                //
                if (HasFlag(flags, CompositeGlyphFlags.WE_HAVE_INSTRUCTIONS))
                {
                    //read this later
                    //ushort numInstr = reader.ReadUInt16();
                    //byte[] insts = reader.ReadBytes(numInstr);
                    //finalGlyph.GlyphInstructions = insts;
                }


                return finalGlyph ?? emptyGlyph;
            }

            readonly struct TripleEncodingRecord
            {
                public readonly byte ByteCount;
                public readonly byte XBits;
                public readonly byte YBits;
                public readonly ushort DeltaX;
                public readonly ushort DeltaY;
                public readonly sbyte Xsign;
                public readonly sbyte Ysign;

                public TripleEncodingRecord(
                    byte byteCount,
                    byte xbits, byte ybits,
                    ushort deltaX, ushort deltaY,
                    sbyte xsign, sbyte ysign)
                {
                    ByteCount = byteCount;
                    XBits = xbits;
                    YBits = ybits;
                    DeltaX = deltaX;
                    DeltaY = deltaY;
                    Xsign = xsign;
                    Ysign = ysign;
                    //#if DEBUG
                    //                debugIndex = -1;
                    //#endif
                }
#if DEBUG
                //public int debugIndex;
                public override string ToString()
                {
                    return ByteCount + " " + XBits + " " + YBits + " " + DeltaX + " " + DeltaY + " " + Xsign + " " + Ysign;
                }
#endif
                /// <summary>
                /// translate X
                /// </summary>
                /// <param name="orgX"></param>
                /// <returns></returns>
                public int Tx(int orgX) => (orgX + DeltaX) * Xsign;

                /// <summary>
                /// translate Y
                /// </summary>
                /// <param name="orgY"></param>
                /// <returns></returns>
                public int Ty(int orgY) => (orgY + DeltaY) * Ysign;

            }

            class TripleEncodingTable
            {

                static TripleEncodingTable s_encTable;

                List<TripleEncodingRecord> _records = new List<TripleEncodingRecord>();
                public static TripleEncodingTable GetEncTable()
                {
                    if (s_encTable == null)
                    {
                        s_encTable = new TripleEncodingTable();
                    }
                    return s_encTable;
                }

                private TripleEncodingTable()
                {

                    BuildTable();

#if DEBUG
                    if (_records.Count != 128)
                    {
                        throw new System.Exception();
                    }
                    dbugValidateTable();
#endif
                }
#if DEBUG
                void dbugValidateTable()
                {
#if DEBUG
                    for (int xyFormat = 0; xyFormat < 128; ++xyFormat)
                    {
                        TripleEncodingRecord tripleRec = _records[xyFormat];
                        if (xyFormat < 84)
                        {
                            //0-83 inclusive
                            if ((tripleRec.ByteCount - 1) != 1)
                            {
                                throw new System.NotSupportedException();
                            }
                        }
                        else if (xyFormat < 120)
                        {
                            //84-119 inclusive
                            if ((tripleRec.ByteCount - 1) != 2)
                            {
                                throw new System.NotSupportedException();
                            }
                        }
                        else if (xyFormat < 124)
                        {
                            //120-123 inclusive
                            if ((tripleRec.ByteCount - 1) != 3)
                            {
                                throw new System.NotSupportedException();
                            }
                        }
                        else if (xyFormat < 128)
                        {
                            //124-127 inclusive
                            if ((tripleRec.ByteCount - 1) != 4)
                            {
                                throw new System.NotSupportedException();
                            }
                        }
                    }

#endif
                }
#endif
                public TripleEncodingRecord this[int index] => _records[index];

                void BuildTable()
                {
                    // Each of the 128 index values define the following properties and specified in details in the table below:

                    // Byte count(total number of bytes used for this set of coordinate values including one byte for 'flag' value).
                    // Number of bits used to represent X coordinate value(X bits).
                    // Number of bits used to represent Y coordinate value(Y bits).
                    // An additional incremental amount to be added to X bits value(delta X).
                    // An additional incremental amount to be added to Y bits value(delta Y).
                    // The sign of X coordinate value(X sign).
                    // The sign of Y coordinate value(Y sign).

                    //Please note that “Byte Count” field reflects total size of the triplet(flag, xCoordinate, yCoordinate), 
                    //including ‘flag’ value that is encoded in a separate stream.


                    //Triplet Encoding
                    //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign

                    //(set 1.1)
                    //0     2            0       8       N/A       0     N/A     -   
                    //1                                            0             +
                    //2                                           256            -
                    //3                                           256            +
                    //4                                           512            -
                    //5                                           512            +
                    //6                                           768            -
                    //7                                           768            +
                    //8                                           1024           -
                    //9                                           1024           +
                    BuildRecords(2, 0, 8, null, new ushort[] { 0, 256, 512, 768, 1024 }); //2*5

                    //---------------------------------------------------------------------
                    //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                    //(set 1.2)
                    //10    2            8       0        0       N/A     -     N/A
                    //11                                  0               +
                    //12                                256               -
                    //13                                256               +
                    //14                                512               -
                    //15                                512               +
                    //16                                768               -
                    //17                                768               +
                    //18                                1024              -
                    //19                                1024              +
                    BuildRecords(2, 8, 0, new ushort[] { 0, 256, 512, 768, 1024 }, null); //2*5

                    //---------------------------------------------------------------------
                    //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                    //(set 2.1)
                    //20    2           4       4        1        1       -      -
                    //21                                          1       +      -
                    //22                                          1       -      +
                    //23                                          1       +      +
                    //24                                          17      -      -
                    //25                                          17      +      -
                    //26                                          17      -      +
                    //27                                          17      +      +
                    //28                                          33      -      - 
                    //29                                          33      +      -
                    //30                                          33      -      +
                    //31                                          33      +      +
                    //32                                          49      -      -
                    //33                                          49      +      -
                    //34                                          49      -      +
                    //35                                          49      +      +  
                    BuildRecords(2, 4, 4, new ushort[] { 1 }, new ushort[] { 1, 17, 33, 49 });// 4*4 => 16 records

                    //---------------------------------------------------------------------            
                    //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                    //(set 2.2)
                    //36    2           4       4       17        1       -      -
                    //37                                          1       +      -
                    //38                                          1       -      +
                    //39                                          1       +      +
                    //40                                          17      -      -
                    //41                                          17      +      -
                    //42                                          17      -      + 
                    //43                                          17      +      +
                    //44                                          33      -      - 
                    //45                                          33      +      -
                    //46                                          33      -      +
                    //47                                          33      +      +
                    //48                                          49      -      -
                    //49                                          49      +      -
                    //50                                          49      -      +
                    //51                                          49      +      +
                    BuildRecords(2, 4, 4, new ushort[] { 17 }, new ushort[] { 1, 17, 33, 49 });// 4*4 => 16 records

                    //---------------------------------------------------------------------            
                    //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                    //(set 2.3)
                    //52    2           4          4     33        1      -      -
                    //53                                           1      +      -
                    //54                                           1      -      +
                    //55                                           1      +      +
                    //56                                          17      -      -
                    //57                                          17      +      -
                    //58                                          17      -      +
                    //59                                          17      +      +
                    //60                                          33      -      -
                    //61                                          33      +      -
                    //62                                          33      -      +
                    //63                                          33      +      +
                    //64                                          49      -      -
                    //65                                          49      +      -
                    //66                                          49      -      +
                    //67                                          49      +      +
                    BuildRecords(2, 4, 4, new ushort[] { 33 }, new ushort[] { 1, 17, 33, 49 });// 4*4 => 16 records

                    //---------------------------------------------------------------------            
                    //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                    //(set 2.4)
                    //68    2           4         4     49         1      -      -
                    //69                                           1      +      -
                    //70                                           1      -      +
                    //71                                           1      +      +
                    //72                                          17      -      -
                    //73                                          17      +      -
                    //74                                          17      -     +
                    //75                                          17      +     +
                    //76                                          33      -     -
                    //77                                          33      +     -
                    //78                                          33      -     +
                    //79                                          33      +     +
                    //80                                          49      -     -
                    //81                                          49      +     -
                    //82                                          49      -     +
                    //83                                          49      +     +
                    BuildRecords(2, 4, 4, new ushort[] { 49 }, new ushort[] { 1, 17, 33, 49 });// 4*4 => 16 records

                    //---------------------------------------------------------------------            
                    //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                    //(set 3.1)
                    //84    3             8       8         1      1      -     -
                    //85                                           1      +     -
                    //86                                           1      -     +
                    //87                                           1      +     +
                    //88                                         257      -     -
                    //89                                         257      +     -
                    //90                                         257      -     +
                    //91                                         257      +     +
                    //92                                         513      -     -
                    //93                                         513      +     -
                    //94                                         513      -     +
                    //95                                         513      +     +
                    BuildRecords(3, 8, 8, new ushort[] { 1 }, new ushort[] { 1, 257, 513 });// 4*3 => 12 records

                    //---------------------------------------------------------------------            
                    //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                    //(set 3.2)
                    //96    3               8       8      257      1     -      -
                    //97                                            1     +      -
                    //98                                            1     -      +
                    //99                                            1     +      +
                    //100                                         257     -      -
                    //101                                         257     +      -
                    //102                                         257     -      +
                    //103                                         257     +      +
                    //104                                         513     -      -
                    //105                                         513     +      -
                    //106                                         513     -      +
                    //107                                         513     +      +
                    BuildRecords(3, 8, 8, new ushort[] { 257 }, new ushort[] { 1, 257, 513 });// 4*3 => 12 records

                    //---------------------------------------------------------------------            
                    //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                    //(set 3.3)
                    //108   3              8        8       513     1     -      -
                    //109                                           1     +      -
                    //110                                           1     -      +
                    //111                                           1     +      +
                    //112                                         257     -      -
                    //113                                         257     +      -
                    //114                                         257     -      +
                    //115                                         257     +      +
                    //116                                         513     -      -
                    //117                                         513     +      -
                    //118                                         513     -      +
                    //119                                         513     +      +
                    BuildRecords(3, 8, 8, new ushort[] { 513 }, new ushort[] { 1, 257, 513 });// 4*3 => 12 records

                    //---------------------------------------------------------------------            
                    //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                    //(set 4)
                    //120   4               12     12         0      0    -      -
                    //121                                                 +      -
                    //122                                                 -      +
                    //123                                                 +      +
                    BuildRecords(4, 12, 12, new ushort[] { 0 }, new ushort[] { 0 }); // 4*1 => 4 records

                    //---------------------------------------------------------------------            
                    //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                    //(set 5)
                    //124   5               16      16      0       0     -      -
                    //125                                                 +      -
                    //126                                                 -      +
                    //127                                                 +      + 
                    BuildRecords(5, 16, 16, new ushort[] { 0 }, new ushort[] { 0 });// 4*1 => 4 records

                }

                void AddRecord(byte byteCount, byte xbits, byte ybits, ushort deltaX, ushort deltaY, sbyte xsign, sbyte ysign)
                {
                    var rec = new TripleEncodingRecord(byteCount, xbits, ybits, deltaX, deltaY, xsign, ysign);
#if DEBUG
                    //rec.debugIndex = _records.Count;
#endif
                    _records.Add(rec);
                }

                void BuildRecords(byte byteCount, byte xbits, byte ybits, ushort[] deltaXs, ushort[] deltaYs)
                {
                    if (deltaXs == null)
                    {
                        //(set 1.1)
                        for (int y = 0; y < deltaYs.Length; ++y)
                        {
                            AddRecord(byteCount, xbits, ybits, 0, deltaYs[y], 0, -1);
                            AddRecord(byteCount, xbits, ybits, 0, deltaYs[y], 0, 1);
                        }
                    }
                    else if (deltaYs == null)
                    {
                        //(set 1.2)
                        for (int x = 0; x < deltaXs.Length; ++x)
                        {
                            AddRecord(byteCount, xbits, ybits, deltaXs[x], 0, -1, 0);
                            AddRecord(byteCount, xbits, ybits, deltaXs[x], 0, 1, 0);
                        }
                    }
                    else
                    {
                        //set 2.1, - set5
                        for (int x = 0; x < deltaXs.Length; ++x)
                        {
                            ushort deltaX = deltaXs[x];

                            for (int y = 0; y < deltaYs.Length; ++y)
                            {
                                ushort deltaY = deltaYs[y];

                                AddRecord(byteCount, xbits, ybits, deltaX, deltaY, -1, -1);
                                AddRecord(byteCount, xbits, ybits, deltaX, deltaY, 1, -1);
                                AddRecord(byteCount, xbits, ybits, deltaX, deltaY, -1, 1);
                                AddRecord(byteCount, xbits, ybits, deltaX, deltaY, 1, 1);
                            }
                        }
                    }
                }
            }
        }
    }
}
