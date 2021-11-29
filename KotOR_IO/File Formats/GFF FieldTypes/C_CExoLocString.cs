using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// A CExoLocString is a localized string. It can contain 0 or more CExoStrings, each one for
        /// a different language and possibly gender.
        /// </summary>
        public class CExoLocString : FIELD
        {
            /// <summary>
            /// Index into the user's dialog.tlk file, which contains a list of most localized text in the
            /// game or toolset. If -1, then the LocString does not reference dialog.tlk at all.
            /// </summary>
            public int StringRef { get; set; }

            /// <summary>
            /// Collection of localized strings.
            /// </summary>
            public List<SubString> Strings { get; set; } = new List<SubString>();

            /// <summary>
            /// Default constructor.
            /// </summary>
            public CExoLocString() : base(GffFieldType.CExoLocString) { }

            /// <summary>
            /// Construct with label, string ref, and list of strings.
            /// </summary>
            /// <param name="label"></param>
            /// <param name="strRef"></param>
            /// <param name="strings"></param>
            public CExoLocString(string label, int strRef, List<SubString> strings)
                : base(GffFieldType.CExoLocString, label)
            {
                StringRef = strRef;
                Strings = strings;
            }

            /// <summary>
            /// Construct by reading a binary reader.
            /// </summary>
            /// <param name="br"></param>
            /// <param name="offset"></param>
            internal CExoLocString(BinaryReader br, int offset)
                : base(GffFieldType.CExoLocString)
            {
                // Header Info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();
                int LabelCount = br.ReadInt32();
                int FieldDataOffset = br.ReadInt32();

                // Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = (GffFieldType)br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                int DataOrDataOffset = br.ReadInt32();

                // Label Logic
                br.BaseStream.Seek(LabelOffset + LabelIndex * 16, 0);
                Label = new string(br.ReadChars(16)).TrimEnd('\0');

                // Complex Value Logic
                br.BaseStream.Seek(FieldDataOffset + DataOrDataOffset, 0);
                int TotalSize = br.ReadInt32();
                StringRef = br.ReadInt32();
                int StringCount = br.ReadInt32();

                for (int i = 0; i < StringCount; i++)
                {
                    SubString SS = new SubString();
                    SS.StringID = br.ReadInt32();
                    int StringLength = br.ReadInt32();
                    SS.SString = new string(br.ReadChars(StringLength));
                    Strings.Add(SS);
                }
            }

            /// <summary>
            /// Collect fields recursively.
            /// </summary>
            /// <param name="Field_Array"></param>
            /// <param name="Raw_Field_Data_Block"></param>
            /// <param name="Label_Array"></param>
            /// <param name="Struct_Indexer"></param>
            /// <param name="List_Indices_Counter"></param>
            internal override void collect_fields(
                ref List<Tuple<FIELD, int, int>> Field_Array,
                ref List<byte> Raw_Field_Data_Block,
                ref List<string> Label_Array,
                ref int Struct_Indexer,
                ref int List_Indices_Counter)
            {
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, Raw_Field_Data_Block.Count, this.GetHashCode());

                int total_size = 8;
                foreach (SubString SS in Strings)
                {
                    total_size += 8 + SS.SString.Length;
                }

                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(total_size));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(StringRef));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Strings.Count));

                foreach (SubString SS in Strings)
                {
                    Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(SS.StringID));
                    Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(SS.SString.Length));
                    Raw_Field_Data_Block.AddRange(SS.SString.ToCharArray().Select(x => (byte)x));
                }

                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }
            }

            /// <summary>
            /// Test equality between two CExoLocString objects.
            /// </summary>
            /// <param name="right"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label
                if (!base.Equals(right))
                    return false;

                var other = right as CExoLocString;

                // Check if using StringRef. If both refs are not -1, check for matching refs.
                if (StringRef != -1 && other.StringRef != -1)
                    return StringRef == other.StringRef;

                // Return false if the two collections don't have equal Counts.
                if (Strings.Count != other.Strings.Count)
                    return false;

                // Check that each SubString, by exact index, has the same string and ID.
                for (int i = 0; i < Strings.Count; i++)
                {
                    if (Strings[i].SString  != other.Strings[i].SString ||
                        Strings[i].StringID != other.Strings[i].StringID)
                    {
                        return false;
                    }
                }

                // All checks passed.
                return true;
            }

            /// <summary>
            /// Generate a hash code for this CExoLocString.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                int partial_hash = 1;
                int[] IDArray = Strings.Select(x => x.StringID).ToArray();
                foreach (int i in IDArray)
                {
                    partial_hash *= i.GetHashCode();
                }
                string[] StringArray = Strings.Select(x => x.SString).ToArray();
                foreach (string s in StringArray)
                {
                    partial_hash *= s.GetHashCode();
                }
                return new { Type, StringRef, partial_hash, Label }.GetHashCode();
            }

            /// <summary>
            /// Write CExoLocString information to string.
            /// </summary>
            /// <returns>[CExoLocString] "Label", "StringRef", # of Strings = _</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, \"{StringRef}\", # of Strings = {Strings?.Count ?? 0}";
            }
        }

        /// <summary>
        /// A CExoLocString SubString is a string with a StringID.
        /// </summary>
        public struct SubString
        {
            /// <summary>
            /// ID = LanguageID * 2 + Gender (0 for neutral/masculine, 1 for feminine)
            /// </summary>
            public int StringID { get; set; }

            /// <summary>
            /// String value
            /// </summary>
            public string SString { get; set; }

            /// <summary>
            /// Construct with ID and String.
            /// </summary>
            /// <param name="StringID"></param>
            /// <param name="SString"></param>
            public SubString(int StringID, string SString)
            {
                this.StringID = StringID;
                this.SString = SString;
            }

            /// <summary>
            /// Write SubString information to a string.
            /// </summary>
            /// <returns>(ID:'String')</returns>
            public override string ToString()
            {
                return $"({StringID}:\'{SString}\')";
            }
        }
    }
} 