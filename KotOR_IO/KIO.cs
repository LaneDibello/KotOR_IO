using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

/*TODO: 
 * 
 * Create detailed constructors, with lots of back-end to make creating these files very user friendly
 * Consider separate class for GFF derivatives
 * Continue populating XML documentation.
 * ADD: GFF add field, add struct, add label, etc. These will be used in "set" accessors in Blueprints. GFF constructors will accept different blueprints, and call their own GFF seeding method.
 * 
 * 
 */

namespace KotOR_IO 
{
    /// <summary>
    /// Class containing extension methods for streams and stream readers.
    /// </summary>
    public static class StreamReaderExtensions
    {
        /// <summary>
        /// Reads and returns an array of all the bytes in a binary reader.
        /// </summary>
        public static byte[] ReadAllBytes(this Stream reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Reads and returns an array of all the bytes in a binary reader.
        /// </summary>
        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }
    }

    /// <summary>
    /// Class containing enumeration extension methods
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Gets an attribute on an enum field value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value.</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? attributes[0] as T : null;
        }

        /// <summary>
        /// Gets the DescriptionAttribute on an enum field value.
        /// </summary>
        /// <param name="value">The enum value</param>
        /// <returns>Returns the description string if available. Otherwise, it returns "null".</returns>
        /// <example><![CDATA[string desc = myEnumVariable.ToDescription();]]></example>
        public static string ToDescription(this Enum value)
        {
            var attribute = value.GetAttributeOfType<DescriptionAttribute>();
            return attribute == null ? "null" : attribute.Description;
        }

        /// <summary>
        /// Gets the WalkableAttribute value on an enum field.
        /// </summary>
        public static bool IsWalkable(this SurfaceMaterial value)
        {
            var attribute = value.GetAttributeOfType<WalkableAttribute>();
            return attribute?.IsWalkable ?? false;
        }

        //public static string ToName(this Enum value)
        //{
        //    var attribute = value.GetAttributeOfType<DisplayNameAttribute>();
        //    return attribute == null ? value.ToString() : attribute.DisplayName;
        //}
    }

    /// <summary>
    /// Resource Type IDs
    /// </summary>
    public enum ResourceType : int
    {
        /// <summary>Null</summary>
        [Description("NULL")]
        NULL = 0,
        /// <summary>Bitmap</summary>
        [Description("BMP ")]
        BMP = 1,
        /// <summary>Truevision Raster Graphic</summary>
        [Description("TGA ")]
        TGA = 3,
        /// <summary>Waveform Audio</summary>
        [Description("WAV ")]
        WAV = 4,
        /// <summary>Vector-Based Plotter</summary>
        [Description("PLT ")]
        PLT = 6,
        /// <summary>Initialization</summary>
        [Description("INI ")]
        INI = 7,
        /// <summary>Text</summary>
        [Description("TXT ")]
        TXT = 10,
        /// <summary>Simulation Model</summary>
        [Description("MDL ")]
        MDL = 2002,
        /// <summary>Sound Set</summary>
        [Description("NSS ")]
        NSS = 2009,
        /// <summary>Model</summary>
        [Description("MOD ")]
        MOD = 2011,
        /// <summary></summary>
        [Description("NCS ")]
        NCS = 2010,
        /// <summary></summary>
        [Description("ARE ")]
        ARE = 2012,
        /// <summary></summary>
        [Description("SET ")]
        SET = 2013,
        /// <summary></summary>
        [Description("IFO ")]
        IFO = 2014,
        /// <summary></summary>
        [Description("BIC ")]
        BIC = 2015,
        /// <summary></summary>
        [Description("WOK ")]
        WOK = 2016,
        /// <summary>Two-Dimensional Array</summary>
        [Description("2DA ")]
        TwoDA = 2017,
        /// <summary>Talk</summary>
        [Description("TLK ")]
        TLK = 2018,
        /// <summary></summary>
        [Description("TXI ")]
        TXI = 2022,
        /// <summary></summary>
        [Description("GIT ")]
        GIT = 2023,
        /// <summary></summary>
        [Description("BTI ")]
        BTI = 2024,
        /// <summary></summary>
        [Description("UTI ")]
        UTI = 2025,
        /// <summary></summary>
        [Description("BTC ")]
        BTC = 2026,
        /// <summary></summary>
        [Description("UTC ")]
        UTC = 2027,
        /// <summary></summary>
        [Description("DLG ")]
        DLG = 2029,
        /// <summary></summary>
        [Description("ITP ")]
        ITP = 2030,
        /// <summary></summary>
        [Description("UTT ")]
        UTT = 2032,
        /// <summary></summary>
        [Description("DDS ")]
        DDS = 2033,
        /// <summary></summary>
        [Description("UTS ")]
        UTS = 2035,
        /// <summary></summary>
        [Description("LTR ")]
        LTR = 2036,
        /// <summary></summary>
        [Description("GFF ")]
        GFF = 2037,
        /// <summary></summary>
        [Description("FAC ")]
        FAC = 2038,
        /// <summary></summary>
        [Description("UTE ")]
        UTE = 2040,
        /// <summary></summary>
        [Description("UTD ")]
        UTD = 2042,
        /// <summary></summary>
        [Description("UTP ")]
        UTP = 2044,
        /// <summary></summary>
        [Description("DFT ")]
        DFT = 2045,
        /// <summary></summary>
        [Description("GIC ")]
        GIC = 2046,
        /// <summary></summary>
        [Description("GUI ")]
        GUI = 2047,
        /// <summary></summary>
        [Description("UTM ")]
        UTM = 2051,
        /// <summary></summary>
        [Description("DWK ")]
        DWK = 2052,
        /// <summary></summary>
        [Description("PWK ")]
        PWK = 2053,
        /// <summary></summary>
        [Description("JRL ")]
        JRL = 2056,
        /// <summary>Save</summary>
        [Description("SAV ")]
        SAV = 2057,
        /// <summary></summary>
        [Description("UTW ")]
        UTW = 2058,
        /// <summary></summary>
        [Description("SSF ")]
        SSF = 2060,
        /// <summary></summary>
        [Description("HAK ")]
        HAK = 2061,
        /// <summary></summary>
        [Description("NDB ")]
        NDB = 2064,
        /// <summary></summary>
        [Description("PTM ")]
        PTM = 2065,
        /// <summary></summary>
        [Description("PTT ")]
        PTT = 2066,
        /// <summary></summary>
        [Description("LYT ")]
        LYT = 3000,
        /// <summary></summary>
        [Description("VIS ")]
        VIS = 3001,
        /// <summary></summary>
        [Description("RIM ")]
        RIM = 3002,
        /// <summary></summary>
        [Description("TPC ")]
        TPC = 3007,
        /// <summary></summary>
        [Description("MDX ")]
        MDX = 3008,
        /// <summary></summary>
        [Description("KEY ")]
        KEY = 9999,
        /// <summary></summary>
        [Description("BIF ")]
        BIF = 9998,
        /// <summary></summary>
        [Description("ERF ")]
        ERF = 9997,
    }

    /// <summary>
    /// IDs for Supported Languages
    /// </summary>
    public enum LanguageID : int
    {
        /// <summary>English</summary>
        English = 0,
        /// <summary>French</summary>
        French = 1,
        /// <summary>German</summary>
        German = 2,
        /// <summary>Italian</summary>
        Italian = 3,
        /// <summary>Spanish</summary>
        Spanish = 4,
        /// <summary>Polish</summary>
        Polish = 5,
        /// <summary>Korean</summary>
        Korean = 128,
        /// <summary>Chinese (Traditional)</summary>
        ChineseTraditional = 129,
        /// <summary>Chinese (Simplified)</summary>
        ChineseSimplified = 130,
        /// <summary>Japanese</summary>
        Japanese = 131,
    }

    /// <summary>
    /// Lookup for GFF Field Type ID based on Field Data Type Text
    /// </summary>
    public enum GffFieldType : int
    {
        /// <summary>Byte</summary>
        BYTE = 0,
        /// <summary>Character</summary>
        CHAR = 1,
        /// <summary>Word</summary>
        WORD = 2,
        /// <summary>Short</summary>
        SHORT = 3,
        /// <summary>Double Word</summary>
        DWORD = 4,
        /// <summary>Integer</summary>
        INT = 5,
        /// <summary>64 Bit Double Word</summary>
        DWORD64 = 6,
        /// <summary>64 Bit Integer</summary>
        INT64 = 7,
        /// <summary>Floating Point</summary>
        FLOAT = 8,
        /// <summary>Double Precision</summary>
        DOUBLE = 9,
        /// <summary>String</summary>
        CExoString = 10,
        /// <summary>Resource Reference</summary>
        ResRef = 11,
        /// <summary>Local String</summary>
        CExoLocString = 12,
        /// <summary>Void</summary>
        VOID = 13,
        /// <summary>Structure</summary>
        Struct = 14,
        /// <summary>List</summary>
        List = 15,
        /// <summary>Orientation</summary>
        Orientation = 16,
        /// <summary>Vector</summary>
        Vector = 17,
        /// <summary>String Reference</summary>
        StrRef = 18,
    }

    /// <summary>
    /// Attribute to describe if a surface material is walkable.
    /// </summary>
    public class WalkableAttribute : Attribute
    {
        /// <summary> Is this item walkable? </summary>
        public bool IsWalkable { get; }

        /// <summary> Attribute to describe if a surface material is walkable. </summary>
        public WalkableAttribute(bool isWalkable) { IsWalkable = isWalkable; }
    }

    /// <summary>
    /// Material used for walkmesh surfaces, as defined in surfacemat.2da.
    /// </summary>
    public enum SurfaceMaterial : int
    {
        /// <summary>Undefined (unwalkable)</summary>
        [Walkable(false)] NotDefined = 0,
        /// <summary>Dirt</summary>
        [Walkable(true)]  Dirt = 1,
        /// <summary>Obscuring (unwalkable)</summary>
        [Walkable(false)] Obscuring = 2,
        /// <summary>Grass</summary>
        [Walkable(true)]  Grass = 3,
        /// <summary>Stone</summary>
        [Walkable(true)]  Stone = 4,
        /// <summary>Wood</summary>
        [Walkable(true)]  Wood = 5,
        /// <summary>Water</summary>
        [Walkable(true)]  Water = 6,
        /// <summary>Non-walkable (unwalkable)</summary>
        [Walkable(false)] NonWalk = 7,
        /// <summary>Transparent (unwalkable)</summary>
        [Walkable(false)] Transparent = 8,
        /// <summary>Carpet</summary>
        [Walkable(true)]  Carpet = 9,
        /// <summary>Metal</summary>
        [Walkable(true)]  Metal = 10,
        /// <summary>Puddles</summary>
        [Walkable(true)]  Puddles = 11,
        /// <summary>Swamp</summary>
        [Walkable(true)]  Swamp = 12,
        /// <summary>Mud</summary>
        [Walkable(true)]  Mud = 13,
        /// <summary>Leaves</summary>
        [Walkable(true)]  Leaves = 14,
        /// <summary>Lava (unwalkable)</summary>
        [Walkable(false)] Lava = 15,
        /// <summary>Bottomless Pit (unwalkable)</summary>
        [Walkable(true)]  BottomlessPit = 16,   // walkable in kotor 2, unused in kotor 1
        /// <summary>Deep Water (unwalkable)</summary>
        [Walkable(false)] DeepWater = 17,
        /// <summary>Door</summary>
        [Walkable(true)]  Door = 18,
        /// <summary>Non-walkable grass (unwalkable)</summary>
        [Walkable(false)] NonWalkGrass = 19,
        /// <summary>Unused surface material (unwalkable)</summary>
        [Walkable(false)] Unused0 = 20,
        /// <summary>Unused surface material (unwalkable)</summary>
        [Walkable(false)] Unused1 = 21,
        /// <summary>Unused surface material (unwalkable)</summary>
        [Walkable(false)] Unused2 = 22,
        /// <summary>Unused surface material (unwalkable)</summary>
        [Walkable(false)] Unused3 = 23,
        /// <summary>Unused surface material (unwalkable)</summary>
        [Walkable(false)] Unused4 = 24,
        /// <summary>Unused surface material (unwalkable)</summary>
        [Walkable(false)] Unused5 = 25,
        /// <summary>Unused surface material (unwalkable)</summary>
        [Walkable(false)] Unused6 = 26,
        /// <summary>Unused surface material (unwalkable)</summary>
        [Walkable(false)] Unused7 = 27,
        /// <summary>Unused surface material (unwalkable)</summary>
        [Walkable(false)] Unused8 = 28,
        /// <summary>Unused surface material (unwalkable)</summary>
        [Walkable(false)] Unused9 = 29,
        /// <summary>Trigger</summary>
        [Walkable(true)]  Trigger = 30,
    }

    /// <summary>
    /// A set of reference data used by the readers and constructors to generate user friendly data. 
    /// </summary>
    public static class Reference_Tables
    {
        /// <summary> Dictionary for conversion of Resource Type IDs to resource file extension. </summary>
        public static Dictionary<int, string> Res_Types = new Dictionary<int, string>() { { 0, "null" }, { 1, "bmp" }, { 3, "tga" }, { 4, "wav" }, { 6, "plt" }, { 7, "ini" }, { 10, "txt" }, { 2002, "mdl" }, { 2009, "nss" }, { 2011, "mod" }, { 2010, "ncs" }, { 2012, "are" }, { 2013, "set" }, { 2014, "ifo" }, { 2015, "bic" }, { 2016, "wok" }, { 2017, "2da" }, { 2018, "tlk" }, { 2022, "txi" }, { 2023, "git" }, { 2024, "bti" }, { 2025, "uti" }, { 2026, "btc" }, { 2027, "utc" }, { 2029, "dlg" }, { 2030, "itp" }, { 2032, "utt" }, { 2033, "dds" }, { 2035, "uts" }, { 2036, "ltr" }, { 2037, "gff" }, { 2038, "fac" }, { 2040, "ute" }, { 2042, "utd" }, { 2044, "utp" }, { 2045, "dft" }, { 2046, "gic" }, { 2047, "gui" }, { 2051, "utm" }, { 2052, "dwk" }, { 2053, "pwk" }, { 2056, "jrl" }, { 2057, "mod" }, { 2058, "utw" }, { 2060, "ssf" }, { 2061, "hak" }, { 2064, "ndb" }, { 2065, "ptm" }, { 2066, "ptt" }, { 3000, "lyt" }, { 3001, "vis" }, { 3002, "rim" }, { 3007, "tpc" }, { 3008, "mdx" }, { 9999, "key" }, { 9998, "bif" }, { 9997, "erf" } };

        /// <summary> Dictionary for conversion of Language IDs to Language name. </summary>
        public static Dictionary<int, string> Language_IDs = new Dictionary<int, string>() { { 0, "English" }, { 1, "French" }, { 2, "German" }, { 3, "Italian" }, { 4, "Spanish" }, { 5, "Polish" }, { 128, "Korean" }, { 129, "Chinese Traditional" }, { 130, "Chinese Simplified" }, { 131, "Japanese" } };

        /// <summary> Dictionary for conversion of GFF Field Type IDs to GFF Field data type text. </summary>
        public static Dictionary<int, string> Field_Types = new Dictionary<int, string>() { { 0, "BYTE" }, { 1, "CHAR" }, { 2, "WORD" }, { 3, "SHORT" }, { 4, "DWORD" }, { 5, "INT" }, { 6, "DWORD64" }, { 7, "INT64" }, { 8, "FLOAT" }, { 9, "DOUBLE" }, { 10, "CExoString" }, { 11, "ResRef" }, { 12, "CExoLocString" }, { 13, "VOID" }, { 14, "Struct" }, { 15, "List" }, { 16, "Orientation" }, { 17, "Vector" }, { 18, "StrRef" } };

        /// <summary> List of GFF Field Types that are marked by the interpreter as "complex". </summary>
        public static List<int> Complex_Field_Types = new List<int>() { 6, 7, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };

        /// <summary> List of GFF Field Types that are marked by the interpreter as "complex". </summary>
        public static List<GffFieldType> ComplexFieldTypes = new List<GffFieldType>()
        {
            GffFieldType.CExoString,
            GffFieldType.DOUBLE,
            GffFieldType.DWORD64,
            GffFieldType.INT64,
            GffFieldType.List,
            GffFieldType.Orientation,
            GffFieldType.StrRef,
            GffFieldType.Struct,
            GffFieldType.Vector,
            GffFieldType.VOID,
        };

        /// <summary> List of Default Fields in Sound set files. </summary>
        public static List<string> SSFields = new List<string>() { "Battlecry 1", "Battlecry 2", "Battlecry 3", "Battlecry 4", "Battlecry 5", "Battlecry 6", "Select 1", "Select 2", "Select 3", "Attack Grunt 1", "Attack Grunt 2", "Attack Grunt 3", "Pain Grunt 1", "Pain Grunt 2", "Low Health", "Dead", "Critical Hit", "Target Immune to Assault", "Lay Mine", "Disarm Mine", "Begin Stealth", "Begin Search", "Begin Unlock", "Unlock Failed", "Unlock Success", "Separate from Party", "Rejoin Party", "Poisoned" };

        /// <summary> Dictionary for conversion of 4 char FileTypes into resource IDs. </summary>
        public static Dictionary<string, int> TypeCodes = new Dictionary<string, int>() { { "NULL", 0 }, { "BMP ", 1 }, { "TGA ", 3 }, { "WAV ", 4 }, { "PLT ", 6 }, { "INI ", 7 }, { "TXT ", 10 }, { "MDL ", 2002 }, { "NSS ", 2009 }, { "MOD ", 2011 }, { "NCS ", 2010 }, { "ARE ", 2012 }, { "SET ", 2013 }, { "IFO ", 2014 }, { "BIC ", 2015 }, { "WOK ", 2016 }, { "2DA ", 2017 }, { "TLK ", 2018 }, { "TXI ", 2022 }, { "GIT ", 2023 }, { "BTI ", 2024 }, { "UTI ", 2025 }, { "BTC ", 2026 }, { "UTC ", 2027 }, { "DLG ", 2029 }, { "ITP ", 2030 }, { "UTT ", 2032 }, { "DDS ", 2033 }, { "UTS ", 2035 }, { "LTR ", 2036 }, { "GFF ", 2037 }, { "FAC ", 2038 }, { "UTE ", 2040 }, { "UTD ", 2042 }, { "UTP ", 2044 }, { "DFT ", 2045 }, { "GIC ", 2046 }, { "GUI ", 2047 }, { "UTM ", 2051 }, { "DWK ", 2052 }, { "PWK ", 2053 }, { "JRL ", 2056 }, { "SAV ", 2057 }, { "UTW ", 2058 }, { "SSF ", 2060 }, { "HAK ", 2061 }, { "NDB ", 2064 }, { "PTM ", 2065 }, { "PTT ", 2066 }, { "LYT ", 3000 }, { "VIS ", 3001 }, { "RIM ", 3002 }, { "TPC ", 3007 }, { "MDX ", 3008 }, { "KEY ", 9999 }, { "BIF ", 9998 }, { "ERF ", 9997 } };

        /// <summary> List of resource type codes that are of GFF type. </summary>
        public static List<int> GFFResTypes = new List<int>() { 2012, 2014, 2015, 2023, 2025, 2027, 2029, 2030, 2032, 2035, 2037, 2038, 2040, 2042, 2044, 2046, 2047, 2051, 2056, 2058, 2065, 2066 };

        /// <summary> List of resource type codes that are of GFF type. </summary>
        public static List<ResourceType> GFFResourceTypes = new List<ResourceType>()
        {
            ResourceType.ARE, ResourceType.BIC, ResourceType.DLG, ResourceType.FAC,
            ResourceType.GFF, ResourceType.GIC, ResourceType.GIT, ResourceType.GUI,
            ResourceType.IFO, ResourceType.ITP, ResourceType.JRL, ResourceType.PTM,
            ResourceType.PTT, ResourceType.UTC, ResourceType.UTD, ResourceType.UTE,
            ResourceType.UTI, ResourceType.UTM, ResourceType.UTP, ResourceType.UTS,
            ResourceType.UTT, ResourceType.UTW,
        };

        /// <summary> List of resource type codes that are of GFF type. </summary>
        public static List<int> ERFResTypes = new List<int>() { 2011, 2057, 9997 };

        /// <summary> List of resource type codes that are of GFF type. </summary>
        public static List<ResourceType> ERFResourceTypes = new List<ResourceType>()
        {
            ResourceType.ERF, ResourceType.MOD, ResourceType.SAV,
        };
    }
}
