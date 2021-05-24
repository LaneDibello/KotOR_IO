using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace KotOR_IO.GFFTypes
{
    #region UTI/Item Refs
    public partial class UTI
    {
        private const string PATH2DA = "data\\2da.bif";
        private const string ITEMPROPDEF = "itempropdef";
        private const string PARAMTABLE = "iprp_paramtable";
        private const string COSTTABLE = "iprp_costtable";
        private const string RACETYPESTABLE = "racialtypes";
        private const string FEATSTABLE = "feat";
        private const string SPELLSTABLE = "spells";
        private const string SUBTYPECOL = "subtyperesref";
        private const string COSTCOL = "costtableresref";
        private const string NAMECOL = "name";
        private const string PARAM1COL = "param1resref";
        private const string PARAM2COL = "param2resref";
        private const string PARAMTABLECOL = "tableresref";
        private const string LABELCOL = "label";
        private const string BASEITEMTABLE = "baseitems";
        private const string ITEMPROPTABLE = "itemprops";
        private const string PROPCOLUMN = "propcolumn";

        private static List<int> omitracetypes = new List<int>() { 0, 1, 2, 3, 4 };
        private static List<int> omitfeats = new List<int>() { 0, 2, 23, 25, 27, 34, 38, 41, 45, 48, 52, 54, 58, 59, 70, 71, 72, 73, 74, 75, 76, 86, 87 };
        private static List<int> omitspells = new List<int>() { 0, 1, 2, 3, 5, 21, 39, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 80, 126, 130 };

        /// <summary>
        /// Table used for determining values for the BodyVariation and TextureVariation Fields for armors. 
        /// The Dictionary Key is the the base item that this applies to.
        /// The first value of the tuple is the value of BodyVariation
        /// The second value (S) is the upper bound [1,S], of the potential values of TextureVariation
        /// </summary>
        private static Dictionary<baseitems, Tuple<byte, byte>> ArmorVars = new Dictionary<baseitems, Tuple<byte, byte>>
        {
            { baseitems.Jedi_Robe, new Tuple<byte, byte>(9, 4) },
            { baseitems.Jedi_Knight_Robe, new Tuple<byte, byte>(9, 4) },
            { baseitems.Jedi_Master_Robe, new Tuple<byte, byte>(9, 4) },
            { baseitems.Armor_Class_4, new Tuple<byte, byte>(3, 7) },
            { baseitems.Armor_Class_5, new Tuple<byte, byte>(4, 8) },
            { baseitems.Armor_Class_6, new Tuple<byte, byte>(5, 7) },
            { baseitems.Armor_Class_7, new Tuple<byte, byte>(6, 7) },
            { baseitems.Armor_Class_8, new Tuple<byte, byte>(7, 6) },
            { baseitems.Armor_Class_9, new Tuple<byte, byte>(8, 6) },
            { baseitems.Basic_Clothing, new Tuple<byte, byte>(2, 8) },
            { baseitems.Revan_Armor, new Tuple<byte, byte>(10, 2) }
        };

        /// <summary>
        /// Table used for determining the value for ModelVariation on the items that use it. 
        /// The dictionary value is the upper bound, [1, U], of the range of values that ModelVariation may be.
        /// Items that are not from the keylist of ArmorVars and are not listed in this collection have a ModelVariation of 1.
        /// </summary>
        private static Dictionary<baseitems, byte> ModelVars = new Dictionary<baseitems, byte> {
            { (baseitems)0, 3 },
            { (baseitems)1, 4 },
            { (baseitems)3, 5 },
            { (baseitems)4, 3 },
            { (baseitems)5, 3 },
            { (baseitems)6, 4 },
            { (baseitems)7, 4 },
            { (baseitems)8, 8 },
            { (baseitems)9, 7 },
            { (baseitems)10, 7 },
            { (baseitems)11, 20 },
            { (baseitems)12, 6 },
            { (baseitems)13, 6 },
            { (baseitems)14, 4 },
            { (baseitems)15, 2 },
            { (baseitems)16, 2 },
            { (baseitems)17, 2 },
            { (baseitems)18, 4 },
            { (baseitems)19, 2 },
            { (baseitems)20, 6 },
            { (baseitems)21, 2 },
            { (baseitems)22, 3 },
            { (baseitems)23, 4 },
            { (baseitems)24, 2 },
            { (baseitems)44, 13 },
            { (baseitems)45, 6 },
            { (baseitems)46, 8 },
            { (baseitems)47, 9 },
            { (baseitems)48, 3 },
            { (baseitems)49, 3 },
            { (baseitems)50, 3 },
            { (baseitems)53, 6 },
            { (baseitems)54, 3 },
            { (baseitems)55, 5 },
            { (baseitems)56, 3 },
            { (baseitems)58, 4 },
            { (baseitems)66, 3 },
            { (baseitems)67, 3 },
            { (baseitems)68, 3 },
            { (baseitems)69, 3 },
            { (baseitems)73, 2 },
            { (baseitems)74, 2 },
            { (baseitems)76, 9 },
        };

        /// <summary>
        /// Matches up each baseitem (int key) to a list of properties. 
        /// If the second value of the Tuple is true, these values are omitted.
        /// If it's false, then everything is omited, but this value.
        /// </summary>
        private static Dictionary<int, Tuple<List<int>, bool>> PropOmits = new Dictionary<int, Tuple<List<int>, bool>>
        {
            { 0,  new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 1,  new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 2,  new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 3,  new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 4,  new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 5,  new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 6,  new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 7,  new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 8,  new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 9,  new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 10, new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 11, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 12, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 13, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 14, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 15, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 16, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 17, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 18, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 19, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 20, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 21, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 22, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 23, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 24, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 25, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 26, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 27, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 28, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 29, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 30, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 31, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 32, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 33, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 34, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 35, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 36, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 37, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 38, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 39, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 40, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 41, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 42, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 43, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 44, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 45, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 46, new Tuple<List<int>, bool>(new List<int> { 10 }, true) },
            { 47, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 30, 31, 32, 37, 38, 39, 40, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 48, new Tuple<List<int>, bool>(new List<int> { 10, 29, 37, 45, 46, 47, 48, 50, 51, 52, 53, 54, 58 }, true) },
            { 49, new Tuple<List<int>, bool>(new List<int> { 10, 29, 37, 45, 46, 47, 48, 50, 51, 52, 53, 54, 58 }, true) },
            { 50, new Tuple<List<int>, bool>(new List<int> { 10, 29, 37, 45, 46, 47, 48, 50, 51, 52, 53, 54, 58 }, true) },
            { 51, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 52, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 53, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 54, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 55, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 56, new Tuple<List<int>, bool>(new List<int> { 10, 58 }, false) },
            { 57, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 58, new Tuple<List<int>, bool>(new List<int> { 46 }, false) },
            { 59, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 60, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 61, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 62, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 63, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 64, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 65, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 66, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 30, 31, 32, 37, 38, 39, 40, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 67, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 30, 31, 32, 37, 38, 39, 40, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 68, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 30, 31, 32, 37, 38, 39, 40, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 69, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 30, 31, 32, 37, 38, 39, 40, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 70, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 30, 31, 32, 37, 38, 39, 40, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 71, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 30, 31, 32, 37, 38, 39, 40, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 72, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 30, 31, 32, 37, 38, 39, 40, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 73, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 30, 31, 32, 37, 38, 39, 40, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 74, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 30, 31, 32, 37, 38, 39, 40, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 75, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 76, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
            { 77, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 8, 10, 11, 12, 13, 15, 22, 28, 29, 32, 37, 45, 46, 47, 48, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 78, new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 79, new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 80, new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 81, new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 82, new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 83, new Tuple<List<int>, bool>(new List<int> { 10, 23, 29, 30, 37, 42, 45, 46, 47, 48, 50, 51, 52, 53, 58 }, true) },
            { 84, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 85, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 86, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 87, new Tuple<List<int>, bool>(new List<int> {  }, false) },
            { 88, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 30, 31, 32, 37, 38, 39, 40, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 89, new Tuple<List<int>, bool>(new List<int> { 5, 6, 7, 10, 11, 12, 13, 22, 23, 28, 29, 45, 30, 31, 32, 37, 38, 39, 40, 42, 46, 47, 48, 49, 50, 51, 52, 53, 55, 56, 58 }, true) },
            { 90, new Tuple<List<int>, bool>(new List<int> { 59 }, false) },
            { 91, new Tuple<List<int>, bool>(new List<int> { 10 }, false) },
        };
    }
    public enum baseitems : int
    {
        [Description("Quarter Staff")]
        Quarter_Staff = 0,
        [Description("Stun Baton")]
        Stun_Baton = 1,
        [Description("Long Sword")]
        Long_Sword = 2,
        [Description("Vibro Sword")]
        Vibro_Sword = 3,
        [Description("Short Sword")]
        Short_Sword = 4,
        [Description("Vibro Blade")]
        Vibro_Blade = 5,
        [Description("Double-Bladed Sword")]
        Double_Bladed_Sword = 6,
        [Description("Vibro Double-Blade")]
        Vibro_Double_Blade = 7,
        [Description("Lightsaber")]
        Lightsaber = 8,
        [Description("Double-Bladed Lightsaber")]
        Double_Bladed_Lightsaber = 9,
        [Description("Short Lightsaber")]
        Short_Lightsaber = 10,
        [Description("Lightsaber Crystals")]
        Lightsaber_Crystals = 11,
        [Description("Blaster Pistol")]
        Blaster_Pistol = 12,
        [Description("Heavy Blaster")]
        Heavy_Blaster = 13,
        [Description("Hold Out Blaster")]
        Hold_Out_Blaster = 14,
        [Description("Ion Blaster")]
        Ion_Blaster = 15,
        [Description("Disruptor Pistol")]
        Disrupter_Pistol = 16,
        [Description("Sonic Pistol")]
        Sonic_Pistol = 17,
        [Description("Ion Rifle")]
        Ion_Rifle = 18,
        [Description("Bowcaster")]
        Bowcaster = 19,
        [Description("Blaster Carbine")]
        Blaster_Carbine = 20,
        [Description("Disruptor Rifle")]
        Disrupter_Rifle = 21,
        [Description("Sonic Rifle")]
        Sonic_Rifle = 22,
        [Description("Repeating Blaster")]
        Repeating_Blaster = 23,
        [Description("Heavy Repeating Blaster")]
        Heavy_Repeating_Blaster = 24,
        [Description("Frag Grenade")]
        Fragmentation_Grenades = 25,
        [Description("Stun Grenade")]
        Stun_Grenades = 26,
        [Description("Thermal Detonator")]
        Thermal_Detonator = 27,
        [Description("Poison Grenade")]
        Poison_Grenade = 28,
        [Description("Flash Grenade")]
        Flash_Grenade = 29,
        [Description("Sonic Grenade")]
        Sonic_Grenade = 30,
        [Description("Adhesive Grenade")]
        Adhesive_Grenade = 31,
        [Description("Cryoban Grenade")]
        Cryoban_Grenade = 32,
        [Description("Fire Grenade")]
        Fire_Grenade = 33,
        [Description("Ion Grenade")]
        Ion_Grenade = 34,
        [Description("Jedi Robe")]
        Jedi_Robe = 35,
        [Description("Jedi Knight Robe")]
        Jedi_Knight_Robe = 36,
        [Description("Jedi Master Robe")]
        Jedi_Master_Robe = 37,
        [Description("Defense Bonus 4")]
        Armor_Class_4 = 38,
        [Description("Defense Bonus 5")]
        Armor_Class_5 = 39,
        [Description("Defense Bonus 6")]
        Armor_Class_6 = 40,
        [Description("Defense Bonus 7")]
        Armor_Class_7 = 41,
        [Description("Defense Bonus 8")]
        Armor_Class_8 = 42,
        [Description("Defense Bonus 9")]
        Armor_Class_9 = 43,
        [Description("Mask")]
        Mask = 44,
        [Description("Gauntlets")]
        Gauntlets = 45,
        [Description("Forearm Bands")]
        Forearm_Bands = 46,
        [Description("Belt")]
        Belt = 47,
        [Description("Implant 1")]
        Implant_1 = 48,
        [Description("Implant 2")]
        Implant_2 = 49,
        [Description("Implant 3")]
        Implant_3 = 50,
        [Description("Datapad")]
        Data_Pad = 52,
        [Description("Adrenaline")]
        Adrenaline = 53,
        [Description("Combat Shots")]
        Combat_Shots = 54,
        [Description("Medical Equipment")]
        Medical_Equipment = 55,
        [Description("Droid Repair Equipment")]
        Droid_Repair_Equipment = 56,
        [Description("Credits")]
        Credits = 57,
        [Description("Trap Kit")]
        Trap_Kit = 58,
        [Description("Security Spikes")]
        Security_Spikes = 59,
        [Description("Programming Spikes")]
        Programming_Spikes = 60,
        [Description("Glow Rod")]
        Glow_Rod = 61,
        [Description("Collar Light")]
        Collar_Light = 62,
        [Description("Torch")]
        Torch = 63,
        [Description("Plot Useable Items")]
        Plot_Useable_Items = 64,
        [Description("Aesthetic Item")]
        Aesthetic_Item = 65,
        [Description("Droid Light Plating")]
        Droid_Light_Plating = 66,
        [Description("Droid Medium Plating")]
        Droid_Medium_Plating = 67,
        [Description("Droid Heavy Plating")]
        Droid_Heavy_Plating = 68,
        [Description("Droid Search Scope")]
        Droid_Search_Scope_x = 69,
        [Description("Droid Motion Sensors")]
        Droid_Motion_Sensors_x = 70,
        [Description("Droid Sonic Sensors")]
        Droid_Sonic_Sensors_x = 71,
        [Description("Droid Targeting Computers")]
        Droid_Targeting_Computers = 72,
        [Description("Droid Computer Spike Mount")]
        Droid_Computer_Spike_Mount_x = 73,
        [Description("Droid Security Spike Mount")]
        Droid_Security_Spike_Mount = 74,
        [Description("Droid Shield")]
        Droid_Shield = 75,
        [Description("Droid Utility Device")]
        Droid_Utility_Device = 76,
        [Description("Blaster Rifle")]
        Blaster_Rifle = 77,
        [Description("Gaffi Stick")]
        Ghaffi_Stick = 78,
        [Description("Wookiee Warblade")]
        Wookie_Warblade = 79,
        [Description("Gamorrean Battleaxe")]
        Gammorean_Battleaxe = 80,
        [Description("Creature Weapon Slashing")]
        Creature_Item_Slash = 81,
        [Description("Creature Weapon Piercing")]
        Creature_Item_Pierce = 82,
        [Description("Creature Weapon Slashing Piercing")]
        Creature_Weapon_Sl_Prc = 83,
        [Description("Creature_Hide_Item")]
        Creature_Hide_Item = 84,
        [Description("Basic Clothing")]
        Basic_Clothing = 85,
        [Description("Pazaak Card")]
        Pazaak_Card = 86,
        [Description("Pazaak Side Deck")]
        Pazaak_Sideboard = 87,
        [Description("Belt")]
        Stealth_Unit = 88,
        [Description("Revan Armor")]
        Revan_Armor = 89,
        [Description("Disguise")]
        Disguise_Item = 90,
        [Description("Squad Recovery Kit")]
        Squad_Recovery_kit = 91,
    }
    public enum itemprops : byte
    {
        [Description("Attribute Bonus")]
        Ability_Bonus = 0,
        [Description("Defense Bonus")]
        AC_Bonus = 1,
        [Description("Defense Bonus vs Alignment Group")]
        AC_Bonus_vs_Alignment_Group = 2,
        [Description("Defense Bonus vs Damage Type")]
        AC_Bonus_vs_Damage_Type = 3,
        [Description("Defense Bonus vs Racial Group")]
        AC_Bonus_vs_Racial_Group = 4,
        [Description("Enhancement Bonus")]
        Enhancement_Bonus = 5,
        [Description("Enhancement Bonus vs Alignment Group")]
        Enhancement_Bonus_vs__Alignment_Group = 6,
        [Description("Enhancement Bonus vs Racial Group")]
        Enchancement_Bonus_vs_Racial_Group = 7,
        [Description("Attack Penalty")]
        Attack_Penalty = 8,
        [Description("Bonus Feat")]
        Bonus_Feat = 9,
        [Description("Activate Item")]
        Activate_Item_IE_CAST_SPELL = 10,
        [Description("Damage Bonus")]
        Damage_Bonus = 11,
        [Description("Damage Bonus vs Alignment Group")]
        Damage_Bonus_vs_Alignment_Group = 12,
        [Description("Damage Bonus vs Racial Group")]
        Damage_Bonus_vs_Racial_Group = 13,
        [Description("Damage Immunity")]
        Damage_Immunity = 14,
        [Description("Damage Penalty")]
        Damage_Penalty = 15,
        [Description("Damage Reduction")]
        Damage_Reduction = 16,
        [Description("Damage Resistance")]
        Damage_Resistance = 17,
        [Description("Damage Vulnerability")]
        Damage_Vulnerability = 18,
        [Description("Decreased Attribute Score")]
        Decreased_Ability_Score = 19,
        [Description("Decreased DB")]
        Decreased_AC = 20,
        [Description("Decreased Skill Modifier")]
        Decreased_Skill_Modifier = 21,
        [Description("Extra Melee Damage Type")]
        Extra_Melee_Damage_Type = 22,
        [Description("Extra Ranged Damage Type")]
        Extra_Ranged_Damage_Type = 23,
        [Description("Immunity")]
        Immunity = 24,
        [Description("Improved Force Resistance")]
        Improved_Force_Resistance = 25,
        [Description("Improved Saving Throws")]
        Improved_Saving_Throws = 26,
        [Description("Improved Saving Throws: Specific")]
        Improved_Saving_Throws_Specific = 27,
        [Description("Keen")]
        Keen = 28,
        [Description("Light")]
        Light = 29,
        [Description("Mighty")]
        Mighty = 30,
        [Description("No Damage")]
        No_Damage = 31,
        [Description("On Hit Properties")]
        On_Hit_Properties = 32,
        [Description("Reduced Saving Throws")]
        Reduced_Saving_Throws = 33,
        [Description("Reduced Saving Throws: Specific")]
        Reduced_Saving_Throws_Specific = 34,
        [Description("Regeneration")]
        Regeneration = 35,
        [Description("Skill Bonus")]
        Skill_Bonus = 36,
        [Description("Security Spike")]
        Security = 37,
        [Description("Attack Bonus")]
        Attack_Bonus = 38,
        [Description("Attack Bonus vs Alignment Group")]
        Attack_Bonus_vs_Alignment_Group = 39,
        [Description("Attack Bonus vs Racial Group")]
        Attack_Bonus_vs_Racial_Group = 40,
        [Description("To Hit Penalty")]
        To_Hit_Penalty = 41,
        [Description("Unlimited Ammunition")]
        Unlimited_Ammunition = 42,
        [Description("Alignment Limitation")]
        Use_Limitation_Alignment_Group = 43,
        [Description("Class Limitation")]
        Use_Limitation_Class = 44,
        [Description("User Limitation")]
        Use_Limitation_Racial_Type = 45,
        [Description("Trap")]
        Trap = 46,
        [Description("Stealth Field Nullifier")]
        True_Seeing = 47,
        [Description("On Massive Hit")]
        On_Monster_Hit = 48,
        [Description("Massive Criticals")]
        Massive_Criticals = 49,
        [Description("Freedom of Movement")]
        Freedom_of_Movement = 50,
        [Description("Monster Damage")]
        Monster_Damage = 51,
        [Description("Special Walk")]
        Special_Walk = 52,
        [Description("Computer Spike")]
        Computer_Spike = 53,
        [Description("Regeneration Force Points")]
        Regeneration_Force_Points = 54,
        [Description("Blaster Bolt Deflection: Increase")]
        Blaster_Bolt_Deflection_Increase = 55,
        [Description("Blaster Bolt Deflection: Decrease")]
        Blaster_Bolt_Deflection_Decrease = 56,
        [Description("Feat Required")]
        Use_Limitation_Feat = 57,
        [Description("Droid Repair Kit")]
        Droid_Repair = 58,
        [Description("Disguise")]
        Disguise = 59,
    }
    #endregion
}