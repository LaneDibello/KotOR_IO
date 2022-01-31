using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// A CHAR is a 1-byte character value.
        /// </summary>
        public class CHAR : FIELD
        {
            /// <summary>
            /// Character value
            /// </summary>
            public char Value { get; set; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public CHAR() : base(GffFieldType.CHAR) { }

            /// <summary>
            /// Construct with label and value.
            /// </summary>
            /// <param name="label"></param>
            /// <param name="value"></param>
            public CHAR(string label, char value)
                : base(GffFieldType.CHAR, label)
            {
                Value = value;
            }

            /// <summary>
            /// Construct by reading a binary reader.
            /// </summary>
            /// <param name="br"></param>
            /// <param name="offset"></param>
            internal CHAR(BinaryReader br, int offset)
                : base(GffFieldType.CHAR)
            {
                //header info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();

                //Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = (GffFieldType)br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                Value = br.ReadChar();

                //Label Logic
                br.BaseStream.Seek(LabelOffset + LabelIndex * 16, 0);
                Label = new string(br.ReadChars(16)).TrimEnd('\0');
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
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, (int)Value, this.GetHashCode());
                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }
            }

            /// <summary>
            /// Test equality between two CHAR objects.
            /// </summary>
            /// <param name="right"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label
                if (!base.Equals(right))
                    return false;

                return Value == (right as CHAR).Value;
            }

            /// <summary>
            /// Generate a hash code for this BYTE.
            /// </summary>
            /// <returns></returns>
            //public override int GetHashCode()
            //{
            //    return new { Type, Value, Label }.GetHashCode();
            //}

            /// <summary>
            /// Write CHAR information to string.
            /// </summary>
            /// <returns>[CHAR] "Label", Value</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, \'{Value}\'";
            }
        }
    }
} 