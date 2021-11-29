using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// A FLOAT is a 4-byte single-precision floating point value.
        /// </summary>
        public class FLOAT : FIELD
        {
            /// <summary>
            /// 4-byte single-precision floating point value
            /// </summary>
            public float Value { get; set; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public FLOAT() : base(GffFieldType.FLOAT) { }

            /// <summary>
            /// Construct with label and value.
            /// </summary>
            /// <param name="label"></param>
            /// <param name="value"></param>
            public FLOAT(string label, float value)
                : base(GffFieldType.FLOAT, label)
            {
                Value = value;
            }

            /// <summary>
            /// Construct by reading a binary reader.
            /// </summary>
            /// <param name="br"></param>
            /// <param name="offset"></param>
            internal FLOAT(BinaryReader br, int offset)
                : base(GffFieldType.FLOAT)
            {
                //header info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();

                //Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = (GffFieldType)br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                Value = br.ReadSingle();

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
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, BitConverter.ToInt32((BitConverter.GetBytes(Value)), 0), this.GetHashCode());
                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }
            }

            /// <summary>
            /// Test equality between two FLOAT objects.
            /// </summary>
            /// <param name="right"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label
                if (!base.Equals(right))
                    return false;

                // Check value
                return Value == (right as FLOAT).Value;
            }

            /// <summary>
            /// Generate a hash code for this FLOAT.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return new { Type, Value, Label }.GetHashCode();
            }

            /// <summary>
            /// Write FLOAT information to string.
            /// </summary>
            /// <returns>[FLOAT] "Label", Value</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, {Value}";
            }
        }
    }
} 