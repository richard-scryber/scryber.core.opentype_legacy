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
using System.Net.Http.Headers;
using System.Text;

namespace Scryber.OpenType.SubTables
{
    public enum CharacterPlatforms : ushort
    {
        Unicode = 0,
        Macintosh= 1,
        Other = 2,
        Windows = 3
    }

    public enum WindowsCharacterEncodings : ushort
    {
        Symbol = 0,
        Unicode = 1,
        ShiftJIS = 2,
        PRC = 3,
        Big5 = 4,
        Wansung = 5,
        Johab = 6,
        Reserved1 = 7,
        Reserved2 = 8,
        Reserverd3 = 9,
        UCS4 = 10
    }

    public enum UnicodeCharacterEncodings : ushort
    {
        Default = 0,
        Version_11 = 1,
        ISO10646 = 2,
        Unicode_20 = 3
    }

    /// <summary>
    /// The character encodings based on the 'Mac' character platform
    /// </summary>
    public enum MacCharacterEncodings : ushort
    {
        Roman = 0,
        Japanese = 1,
        Traditional_Chinese = 2,
        Korean = 3,
        Arabic = 4,
        Hebrew = 5,
        Greek = 6,
        Russian = 7,
        RSymbol = 8,
        Devanagari = 9,
        Gurmukhi = 10,
        Gujarati = 11,
        Orija = 12,
        Bengali = 13,
        Tamil = 14,
        Teluga = 15,
        Kannada = 16,
        Malayalam = 17,
        Sinhalese = 18,
        Burmese = 19,
        Khmer = 20,
        Thai = 21,
        Laotian = 22,
        Georgian = 23,
        Armenian = 24,
        SimplifiedChinese = 25,
        Tibetan = 26,
        Mongolian = 27,
        Geez = 28,
        Slavic = 29,
        Vietnamese = 30,
        Sindhi = 31,
        Uninterpreted = 32
    }

    [Flags()]
    public enum FontHeaderFlags : ushort
    {
        BaseLineAtZero = 1,
        LeftPointAtZero = 2,
        InstructionsBasedOnPoint = 4,
        ForcePPemToInteger = 8,
        InstructionsAlterWidth = 16,
        VerticalLayout = 32,
        Reserved1 = 64,
        ArabicFont = 128,
        IsGXWithMetamorphosis = 256,
        ContainsStrongR2LGlyphs = 512,
        ContainsIndicStyle = 1024,
        FontDataIsLossless = 2048,
        FontIsConverted = 4096,
        FontIsOptimizedForClearType = 8192,
        Reserved2 = 16384,
        Reserved3 = 32768
    }

    [Flags()]
    public enum FontStyleFlags : ushort
    {
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Outline = 8,
        Shadow = 16,
        Condensed = 32,
        Extended = 64
    }

    public enum FontDirectionFlags : short
    {
        StrongR2LAndNeutrals = -2,
        StrongR2L = -1,
        Mixed = 0,
        StrongL2R = 1,
        StrongL2RAndNeutrals = 2
    }

    public enum FontIndexLocationFormat : ushort
    {
        ShortOffsets = 0,
        LongOffsets = 1
    }

    public enum GlyphDataFormat : ushort
    {
        Current = 0
    }

    public enum OS2TableVersion : ushort
    {
        TrueType15 = 0,
        TrueType166 = 1,
        OpenType12 = 2
    }

    public enum WeightClass : ushort
    {
        Thin = 100,
        ExtraLight = 200,
        Light = 300,
        Normal = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        ExtraBold = 800,
        Black = 900
    }

    public enum WidthClass : ushort
    {
        UltraCondensed = 1,
        ExtraCondensed = 2,
        Condensed = 3,
        SemiCondensed = 4,
        Medium = 5,
        SemiExpanded = 6,
        Expanded = 7,
        ExtraExpanded = 8,
        UltraExpanded = 9
    }

    [Flags()]
    public enum FontRestrictions : ushort
    {
        InstallableEmbedding = 0,
        Reserved0 = 1,
        NoEmbedding = 2,
        PreviewPrintEmbedding = 4,
        EditableEmbedding = 8,
        Reserved4 = 16,
        Reserved5 = 32,
        Reserved6 = 64,
        Reserved7 = 128,
        NoSubsetting = 256,
        BitmapEmbedding = 512,
        Reserved10 = 1024,
        Reserved11 = 2048,
        Reserved12 = 4096,
        Reserved13 = 8192,
        Reserved14 = 16384,
        Reserved15 = 32768,
    }

    public enum UnicodeRangeBit : int
    {
        BasicLatin = 0,
        Latin1Supplement = 1,
        LatinExtendedA = 2,
        LatinExtendedB = 3,
        IPAExtensions = 4,
        SpacingModifierLetters = 5,
        CombinigDiacriticalMarks = 6,
        GreekAndCoptic = 7,
        Cyrillic = 9,
        Armenian = 10,
        Hebrew = 11,
        Arabic = 13,
        Devanagari = 15,
        Bengali = 16,
        Gurmukhi = 17,
        Gujarati = 18,
        Oriya = 19,
        Tamil = 20,
        Telugu = 21,
        Kannada = 22,
        Malayalam = 23,
        Thai = 24,
        Lao = 25,
        Georgian = 26,
        HangulJamo = 28,
        LatinExtendedAdditional = 29,
        GreekExtended = 30,
        GeneralPunctuation = 31,
        SuperAndSubScripts = 32,
        CurrencySymbols = 33,
        CombiningDiacriticalMarkForSymbols = 34,
        LetterlikeSymbols = 35,
        NumberForms = 36,
        Arrows = 37,
        MathematicalOperators = 38,
        MiscTechnical = 39,
        ControlPictures = 40,
        OpticalCharacterRecognition = 41,
        EnclosedAlphaNumerics = 42,
        BoxDrawing = 43,
        BlockComponents = 44,
        GeometricShapes = 45,
        MiscSymbols = 46,
        Dingbats = 47,
        CJKSymbolsAndPunctuation = 48,
        Hiragana = 49,
        Katakana = 50,
        Bopomofo = 51,
        HangulCompatibility = 52,
        EnclosedCJKLettersAndMonths = 54,
        CJKCompatibility = 55,
        HangulSyllables = 56,
        NonPlane = 57,
        CJKUnifiedIdeographs = 59,
        PrivateUseArea = 60,
        CJKCompatibilityIdeographs = 61,
        AlphabeticPresentationForms = 62,
        ArabicPresentationFormsA = 63,
        CombiningHalfMarks = 64,
        CJKCompatibilityForms = 65,
        SmallFormVariants = 66,
        ArabicPresentationFormsB = 67,
        HalfWidthAndFullWidthForms = 68,
        Specials = 69,
        Tibetan = 70,
        Syriac = 71,
        Thaana = 72,
        Sinhala = 73,
        Myanmar = 74,
        Ethiopic = 75,
        Cherokee = 76,
        UnifiedCanadianAboriginalSyllabics = 77,
        Ogham = 78,
        Runic = 79,
        Khmer = 80,
        Mongolian = 81,
        BrailePatterns = 82,
        YiSyllables = 83,
        Tagalog = 84,
        OldItalic = 85,
        Gothic = 86,
        Deseret = 87,
        MusicalSymbols = 88,
        MathematicalAlphanumericSymbols = 89,
        PrivateUsePlane15 = 90,
        VariationSelectors = 91,
        Tags = 92
    }

    [Flags()]
    public enum FontSelection : ushort
    {
        Italic = 1,
        Underscore = 2,
        Negative = 4,
        Outlined = 8,
        Strikeout = 16,
        Bold = 32,
        Regular = 64
    }

    public enum CodePageBit : int
    {
        Latin1 = 0,
        Latin2EasternEurope = 1,
        Cyrillic = 2,
        Greek = 3,
        Turkish = 4,
        Hebrew = 5,
        Arabic = 6,
        WindowsBaltic = 7,
        Vietnamese = 8,
        Thai = 16,
        Japan = 17,
        ChineseSimplified = 18,
        KoreanWansung = 19,
        ChineseTraditional = 20,
        KoreanJohab = 21,
        MacintoshUSRoman = 29,
        OEMCharacterset = 30,
        SymbolCharacterSet = 31,
        IBMGreek = 48,
        MSDOSRussian = 49,
        MSDOSNordic = 50,
        Arabic2 = 51,
        MSDOSCanadianFrench = 52,
        Hebrew2 = 53,
        MSDOSIcelandic = 54,
        MSDOSPortuguese = 55,
        IBMTurkish = 56,
        IBMCyrillic = 57,
        Latin2 = 58,
        MSDOSBaltic = 59,
        GreekFormer437G = 60,
        ArabicFormerASMO708 = 61,
        WELatin1 = 62,
        US = 63
    }

    public enum KerningCoverage : byte
    {
        Horizontal = 1,
        Minimum = 2,
        CrossStream = 4,
        Override = 8,
        Reserved4 = 16,
        Reserved5 = 32,
        Reserved6 = 64,
        Reserved7 = 128,
        
    }

    public enum KerningFormat : byte
    {
        Format0 = 1,
        Format3 = 4
    }

    public enum NameTypes : ushort
    {
        CopyrightNotice = 0,
        FontFamily = 1,
        FontSubFamily = 2,
        UniqueFontIdentifier = 3,
        FullFontName = 4,
        Version = 5,
        PostscriptName = 6,
        Trademark = 7,
        ManufacturerName = 8,
        Designer = 9,
        Description = 10,
        VendorURL = 11,
        DesignerURL = 12,
        LicenseDescription = 13,
        LicenseURL = 14,
        Reserved0 = 15,
        PreferredFamily = 16,
        PreferredSubFamily = 17,
        CompatibleFull_MacOnly = 18,
        SampleText = 19,
        PostscriptCID = 20

    }

    public enum WOFF2TableTypes
    {
        cmap = 0,
        head = 1,
        hhea = 2,
        hmtx = 3,
        maxp = 4,
        name = 5,
        OS_2 = 6,
        post = 7,
        cvt_ = 8,
        fpgm = 9,
        glyf = 10,
        loca = 11,
        prep = 12,
        CFF_ = 13,
        VORG = 14,
        EBDT = 15,
        EBLC = 16,
        gasp = 17,
        hdmx = 18,
        kern = 19,
        LTSH = 20,
        PCLT = 21,
        VDMX = 22,
        vhea = 23,
        vmtx = 24,
        BASE = 25,
        GDEF = 26,
        GPOS = 27,
        GSUB = 28,
        EBSC = 29,
        JSTF = 30,
        MATH = 31,
        CBDT = 32,
        CBLC = 33,
        COLR = 34,
        CPAL = 35,
        SVG_ = 36,
        sbix = 37,
        acnt = 38,
        avar = 39,
        bdat = 40,
        bloc = 41,
        bsln = 42,
        cvar = 43,
        fdsc = 44,
        feat = 45,
        fmtx = 46,
        fvar = 47,
        gvar = 48,
        hsty = 49,
        just = 50,
        lcar = 51,
        mort = 52,
        morx = 53,
        opbd = 54,
        prop = 55,
        trak = 56,
        Zapf = 57,
        Silf = 58,
        Glat = 59,
        Gloc = 60,
        Feat = 61,
        Sill = 62,
        _Arbitary = 63

    }




    class OriginalType2OperatorAttribute : Attribute
    {
        public OriginalType2OperatorAttribute(Type2Operator1 type2Operator1)
        {
        }
        public OriginalType2OperatorAttribute(Type2Operator2 type2Operator1)
        {
        }
    }

    enum Type2Operator1 : byte
    {
        //Appendix A Type 2 Charstring Command Codes       
        _Reserved0_ = 0,
        hstem, //1
        _Reserved2_,//2
        vstem, //3
        vmoveto,//4
        rlineto, //5
        hlineto, //6
        vlineto,//7,
        rrcurveto,//8
        _Reserved9_, //9
        callsubr, //10
                  //---------------------
        _return, //11
        escape,//12
        _Reserved13_,
        endchar,//14
        _Reserved15_,
        _Reserved16_,
        _Reserved17_,
        hstemhm,//18
        hintmask,//19
        cntrmask,//20
                 //---------------------
        rmoveto,//21
        hmoveto,//22
        vstemhm,//23
        rcurveline, //24
        rlinecurve,//25
        vvcurveto,//26
        hhcurveto, //27
        shortint, //28
        callgsubr, //29
        vhcurveto, //30
                   //-----------------------
        hvcurveto, //31
    }
    enum Type2Operator2 : byte
    {
        //Two-byte Type 2 Operators
        _Reserved0_ = 0,
        _Reserved1_,
        _Reserved2_,
        and, //3
        or, //4
        not, //5
        _Reserved6_,
        _Reserved7_,
        _Reserved8_,
        //
        abs,//9        
        add,//10
            //------------------
        sub,//11
        div,//12
        _Reserved13_,
        neg,//14
        eq, //15
        _Reserved16_,
        _Reserved17_,
        drop,//18
        _Reserved19_,
        put,//20
            //------------------ 
        get, //21
        ifelse,//22
        random,//23
        mul, //24,
        _Reserved25_,
        sqrt,//26
        dup,//27
        exch,//28 , exchanges the top two elements on the argument stack
        index,//29
        roll,//30
             //--------------
        _Reserved31_,
        _Reserved32_,
        _Reserved33_,
        //--------------
        hflex,//34
        flex, //35
        hflex1,//36
        flex1//37
    }

    /// <summary>
    /// Merged ccf operators,(op1 and op2, note on attribute of each field)
    /// </summary>
    public enum OperatorName : byte
    {
        Unknown,
        //
        LoadInt,
        LoadFloat,
        GlyphWidth,

        LoadSbyte4, //my extension, 4 sbyte in an int32
        LoadSbyte3, //my extension, 3 sbytes in an int32
        LoadShort2, //my extension, 2 short in an int32

        //---------------------
        //type2Operator1
        //---------------------
        [OriginalType2Operator(Type2Operator1.hstem)] hstem,
        [OriginalType2Operator(Type2Operator1.vstem)] vstem,
        [OriginalType2Operator(Type2Operator1.vmoveto)] vmoveto,
        [OriginalType2Operator(Type2Operator1.rlineto)] rlineto,
        [OriginalType2Operator(Type2Operator1.hlineto)] hlineto,
        [OriginalType2Operator(Type2Operator1.vlineto)] vlineto,
        [OriginalType2Operator(Type2Operator1.rrcurveto)] rrcurveto,
        [OriginalType2Operator(Type2Operator1.callsubr)] callsubr,
        //---------------------
        [OriginalType2Operator(Type2Operator1._return)] _return,
        //[OriginalType2Operator(Type2Operator1.escape)] escape, //not used!
        [OriginalType2Operator(Type2Operator1.endchar)] endchar,
        [OriginalType2Operator(Type2Operator1.hstemhm)] hstemhm,

        //---------
        [OriginalType2Operator(Type2Operator1.hintmask)] hintmask1, //my hint-mask extension, contains 1 byte hint
        [OriginalType2Operator(Type2Operator1.hintmask)] hintmask2, //my hint-mask extension, contains 2 bytes hint
        [OriginalType2Operator(Type2Operator1.hintmask)] hintmask3, //my hint-mask extension, contains 3 bytes hint
        [OriginalType2Operator(Type2Operator1.hintmask)] hintmask4, //my hint-mask extension, contains 4 bytes hint 
        [OriginalType2Operator(Type2Operator1.hintmask)] hintmask_bits,//my hint-mask extension, contains n bits of hint

        //---------

        [OriginalType2Operator(Type2Operator1.cntrmask)] cntrmask1, //my counter-mask extension, contains 1 byte hint
        [OriginalType2Operator(Type2Operator1.cntrmask)] cntrmask2, //my counter-mask extension, contains 2 bytes hint
        [OriginalType2Operator(Type2Operator1.cntrmask)] cntrmask3, //my counter-mask extension, contains 3 bytes hint
        [OriginalType2Operator(Type2Operator1.cntrmask)] cntrmask4, //my counter-mask extension, contains 4 bytes hint
        [OriginalType2Operator(Type2Operator1.cntrmask)] cntrmask_bits, //my counter-mask extension, contains n bits of hint

        //---------------------
        [OriginalType2Operator(Type2Operator1.rmoveto)] rmoveto,
        [OriginalType2Operator(Type2Operator1.hmoveto)] hmoveto,
        [OriginalType2Operator(Type2Operator1.vstemhm)] vstemhm,
        [OriginalType2Operator(Type2Operator1.rcurveline)] rcurveline,
        [OriginalType2Operator(Type2Operator1.rlinecurve)] rlinecurve,
        [OriginalType2Operator(Type2Operator1.vvcurveto)] vvcurveto,
        [OriginalType2Operator(Type2Operator1.hhcurveto)] hhcurveto,
        [OriginalType2Operator(Type2Operator1.shortint)] shortint,
        [OriginalType2Operator(Type2Operator1.callgsubr)] callgsubr,
        [OriginalType2Operator(Type2Operator1.vhcurveto)] vhcurveto,
        //-----------------------
        [OriginalType2Operator(Type2Operator1.hvcurveto)] hvcurveto,
        //--------------------- 
        //Two-byte Type 2 Operators 
        [OriginalType2Operator(Type2Operator2.and)] and,
        [OriginalType2Operator(Type2Operator2.or)] or,
        [OriginalType2Operator(Type2Operator2.not)] not,
        [OriginalType2Operator(Type2Operator2.abs)] abs,
        [OriginalType2Operator(Type2Operator2.add)] add,
        //------------------
        [OriginalType2Operator(Type2Operator2.sub)] sub,
        [OriginalType2Operator(Type2Operator2.div)] div,
        [OriginalType2Operator(Type2Operator2.neg)] neg,
        [OriginalType2Operator(Type2Operator2.eq)] eq,
        [OriginalType2Operator(Type2Operator2.drop)] drop,
        [OriginalType2Operator(Type2Operator2.put)] put,
        //------------------ 
        [OriginalType2Operator(Type2Operator2.get)] get,
        [OriginalType2Operator(Type2Operator2.ifelse)] ifelse,
        [OriginalType2Operator(Type2Operator2.random)] random,
        [OriginalType2Operator(Type2Operator2.mul)] mul,
        [OriginalType2Operator(Type2Operator2.sqrt)] sqrt,
        [OriginalType2Operator(Type2Operator2.dup)] dup,
        [OriginalType2Operator(Type2Operator2.exch)] exch,
        [OriginalType2Operator(Type2Operator2.index)] index,
        [OriginalType2Operator(Type2Operator2.roll)] roll,
        [OriginalType2Operator(Type2Operator2.hflex)] hflex,
        [OriginalType2Operator(Type2Operator2.flex)] flex,
        [OriginalType2Operator(Type2Operator2.hflex1)] hflex1,
        [OriginalType2Operator(Type2Operator2.flex1)] flex1
    }



}
