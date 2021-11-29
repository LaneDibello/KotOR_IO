using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// A BYTE is a 1-byte signed numeric value.
        /// </summary>
        public class BYTE : FIELD
        {
            /// <summary>
            /// Byte value
            /// </summary>
            public byte Value { get; set; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public BYTE() : base(GffFieldType.BYTE) { }

            /// <summary>
            /// Construct with label and value.
            /// </summary>
            /// <param name="label"></param>
            /// <param name="value"></param>
            public BYTE(string label, byte value)
                : base(GffFieldType.BYTE, label)
            {
                Value = value;
            }

            /// <summary>
            /// Construct by reading a binary reader.
            /// </summary>
            /// <param name="br"></param>
            /// <param name="offset"></param>
            internal BYTE(BinaryReader br, int offset)
                : base(GffFieldType.BYTE)
            {
                //header info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();

                //Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = (GffFieldType)br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                Value = br.ReadByte();

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
            /// Test equality between two BYTE objects.
            /// </summary>
            /// <param name="right"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label
                if (!base.Equals(right))
                    return false;

                return Value == (right as BYTE).Value;
            }

            /// <summary>
            /// Generate a hash code for this BYTE.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return new { Type, Value, Label }.GetHashCode();
            }

            /// <summary>
            /// Write BYTE information to string.
            /// </summary>
            /// <returns>[BYTE] "Label", Value</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, {Value}";
            }
        }

    }
} 