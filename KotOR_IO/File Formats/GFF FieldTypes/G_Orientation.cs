using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// An Orientation is a 
        /// </summary>
        public class Orientation : FIELD
        {
            /// <summary>
            /// Value 1
            /// </summary>
            public float Value1 { get; set; }

            /// <summary>
            /// Value 2
            /// </summary>
            public float Value2 { get; set; }

            /// <summary>
            /// Value 3
            /// </summary>
            public float Value3 { get; set; }

            /// <summary>
            /// Value 4
            /// </summary>
            public float Value4 { get; set; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public Orientation() : base(GffFieldType.Orientation) { }

            /// <summary>
            /// Construct with label and values.
            /// </summary>
            public Orientation(string label, int value1, int value2, int value3, int value4)
                : base(GffFieldType.Orientation, label)
            {
                Value1 = value1;
                Value2 = value2;
                Value3 = value3;
                Value4 = value4;
            }

            /// <summary>
            /// Construct by reading a binary reader.
            /// </summary>
            internal Orientation(BinaryReader br, int offset)
                : base(GffFieldType.Orientation)
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
                Value1 = br.ReadSingle();
                Value2 = br.ReadSingle();
                Value3 = br.ReadSingle();
                Value4 = br.ReadSingle();
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
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Value1));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Value2));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Value3));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Value4));
                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }
            }

            /// <summary>
            /// Test equality between two Orientation objects.
            /// </summary>
            /// <param name="right"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label.
                if (!base.Equals(right))
                    return false;

                // Check values.
                var other = right as Orientation;
                return Value1 == other.Value1 &&
                       Value2 == other.Value2 &&
                       Value3 == other.Value3 &&
                       Value4 == other.Value4;
            }

            /// <summary>
            /// Generate a hash code for this Orientation.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return new { Type, Value1, Value2, Value3, Value4, Label }.GetHashCode();
            }

            /// <summary>
            /// Write Orientation information to string.
            /// </summary>
            /// <returns>[Orientation] "Label", (v1, v2, v3, v4)</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, ({Value1}, {Value2}, {Value3}, {Value4})";
            }
        }
    }
} 