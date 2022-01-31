using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// A DWORD64 is an 8-byte unsigned numeric value.
        /// </summary>
        public class DWORD64 : FIELD
        {
            /// <summary>
            /// 8-byte unsigned value
            /// </summary>
            public ulong Value { get; set; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public DWORD64() : base(GffFieldType.DWORD64) { }

            /// <summary>
            /// Construct with label and value.
            /// </summary>
            /// <param name="label"></param>
            /// <param name="value"></param>
            public DWORD64(string label, ulong value)
                : base(GffFieldType.DWORD64, label)
            {
                Value = value;
            }

            /// <summary>
            /// Construct by reading a binary reader.
            /// </summary>
            /// <param name="br"></param>
            /// <param name="offset"></param>
            internal DWORD64(BinaryReader br, int offset)
                : base(GffFieldType.DWORD64)
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
                Value = br.ReadUInt64();
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
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Value));
                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }
            }

            /// <summary>
            /// Test equality between two DWORD64 objects.
            /// </summary>
            /// <param name="right"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label
                if (!base.Equals(right))
                    return false;

                return Value == (right as DWORD64).Value;
            }

            /// <summary>
            /// Generate a hash code for this DWORD64.
            /// </summary>
            /// <returns></returns>
            //public override int GetHashCode()
            //{
            //    return new { Type, Value, Label }.GetHashCode();
            //}

            /// <summary>
            /// Write DWORD64 information to string.
            /// </summary>
            /// <returns>[DWORD64] "Label", Value</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, {Value}";
            }
        }
    }
} 