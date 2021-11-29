using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// A CExoString is a string with length less than uint.MaxValue.
        /// </summary>
        public class CExoString : FIELD
        {
            /// <summary>
            /// String value
            /// </summary>
            public string CEString { get; set; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public CExoString() : base(GffFieldType.CExoString) { }

            /// <summary>
            /// Construct with label and value.
            /// </summary>
            /// <param name="label"></param>
            /// <param name="ceStr"></param>
            public CExoString(string label, string ceStr)
                : base(GffFieldType.CExoString, label)
            {
                CEString = ceStr;
            }

            /// <summary>
            /// Construct by reading a binary reader.
            /// </summary>
            /// <param name="br"></param>
            /// <param name="offset"></param>
            internal CExoString(BinaryReader br, int offset)
                : base(GffFieldType.CExoString)
            {
                //Header Info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();
                int LabelCount = br.ReadInt32();
                int FieldDataOffset = br.ReadInt32();

                //Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = (GffFieldType)br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                int DataOrDataOffset = br.ReadInt32();

                //Label Logic
                br.BaseStream.Seek(LabelOffset + LabelIndex * 16, 0);
                Label = new string(br.ReadChars(16)).TrimEnd('\0');

                //Comlex Value Logic
                br.BaseStream.Seek(FieldDataOffset + DataOrDataOffset, 0);
                int Size = br.ReadInt32();
                CEString = new string(br.ReadChars(Size));
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
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(CEString.Length));
                Raw_Field_Data_Block.AddRange(CEString.ToCharArray().Select(x => (byte)x));
                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }
            }

            /// <summary>
            /// Test equality between two CExoString objects.
            /// </summary>
            /// <param name="right"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label
                if (!base.Equals(right))
                    return false;

                // Check value
                return CEString == (right as CExoString).CEString;
            }

            /// <summary>
            /// Generate a hash code for this CExoString.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return new { Type, CEString, Label }.GetHashCode();
            }

            /// <summary>
            /// Write CExoString information to string.
            /// </summary>
            /// <returns>[CExoString] "Label", "String"</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, \"{CEString}\"";
            }
        }
    }
} 