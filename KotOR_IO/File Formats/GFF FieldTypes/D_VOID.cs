using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// VOID data is an arbitrary sequence of bytes to be interpreted by the application
        /// in a programmer-defined fashion.
        /// </summary>
        public class VOID : FIELD
        {
            /// <summary>
            /// Byte data.
            /// </summary>
            public List<byte> Data { get; set; } = new List<byte>();

            /// <summary>
            /// Default constructor.
            /// </summary>
            public VOID() : base(GffFieldType.VOID) { }

            /// <summary>
            /// Construct with label and data.
            /// </summary>
            /// <param name="label"></param>
            /// <param name="data"></param>
            public VOID(string label, List<byte> data)
                : base(GffFieldType.VOID, label)
            {
                Label = label;
                Data = data;
            }

            /// <summary>
            /// Construct by reading a binary reader.
            /// </summary>
            /// <param name="br"></param>
            /// <param name="offset"></param>
            internal VOID(BinaryReader br, int offset)
                : base(GffFieldType.VOID)
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
                int Size = br.ReadInt32();
                for (int i = 0; i < Size; i++)
                {
                    Data.Add(br.ReadByte());
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
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Data.Count));
                Raw_Field_Data_Block.AddRange(Data);
                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }
            }

            /// <summary>
            /// Test equality between two VOID objects.
            /// </summary>
            /// <param name="right"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label
                if (!base.Equals(right))
                    return false;

                // Check data
                return Data == (right as VOID).Data;
            }

            /// <summary>
            /// Generate a hash code for this VOID.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                int partial_hash = 1;
                byte[] dataArray = Data.ToArray();
                foreach (byte b in dataArray)
                {
                    partial_hash *= b.GetHashCode();
                }
                return new { Type, partial_hash, Label }.GetHashCode();
            }

            /// <summary>
            /// Write VOID information to string.
            /// </summary>
            /// <returns>[VOID] "Label", Size of Data = _</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, Size of Data = {Data?.Count ?? 0}";
            }
        }
    }
} 