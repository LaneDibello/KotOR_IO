using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// A LIST is a collection of STRUCT objects.
        /// </summary>
        public class LIST : FIELD
        {
            /// <summary>
            /// Structs contained in this LIST.
            /// </summary>
            public List<STRUCT> Structs { get; set; } = new List<STRUCT>();

            /// <summary>
            /// Default constructor.
            /// </summary>
            public LIST() : base(GffFieldType.List) { }

            /// <summary>
            /// Construct using a custom list of structs.
            /// </summary>
            /// <param name="label"></param>
            /// <param name="structs"></param>
            public LIST(string label, List<STRUCT> structs)
                : base(GffFieldType.List, label)
            {
                Label = label;
                Structs = structs;
            }

            /// <summary>
            /// Construct by reading from a binary reader.
            /// </summary>
            /// <param name="br"></param>
            /// <param name="offset"></param>
            internal LIST(BinaryReader br, int offset)
                : base(GffFieldType.List)
            {
                //Header Info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();
                int LabelCount = br.ReadInt32();
                int FieldDataOffset = br.ReadInt32();
                int FieldDataCount = br.ReadInt32();
                int FieldIndicesOffset = br.ReadInt32();
                int FieldIndicesCount = br.ReadInt32();
                int ListIndicesOffset = br.ReadInt32();


                //Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = (GffFieldType)br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                int DataOrDataOffset = br.ReadInt32();

                //Label Logic
                br.BaseStream.Seek(LabelOffset + LabelIndex * 16, 0);
                Label = new string(br.ReadChars(16)).TrimEnd('\0');

                //Comlex Value Logic
                br.BaseStream.Seek(ListIndicesOffset + DataOrDataOffset, 0);
                int Size = br.ReadInt32();
                for (int i = 0; i < Size; i++)
                {
                    br.BaseStream.Seek(ListIndicesOffset + DataOrDataOffset + 4 + i * 4, 0);
                    int s_index = br.ReadInt32();
                    Structs.Add(new STRUCT(br, s_index));
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
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, List_Indices_Counter, this.GetHashCode());
                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }

                List_Indices_Counter += 4 + 4 * Structs.Count;

                foreach (STRUCT S in Structs)
                {
                    S.collect_fields(ref Field_Array, ref Raw_Field_Data_Block, ref Label_Array, ref Struct_Indexer, ref List_Indices_Counter);
                }

            }

            /// <summary>
            /// Test equality between two LIST objects.
            /// </summary>
            /// <returns>True if the lists have the same Struct_Type, Label, and Fields.</returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label
                if (!base.Equals(right))
                    return false;

                var other = right as LIST;
                
                // Check number of Structs
                if (Structs.Count() != other.Structs.Count())
                    return false;

                // Check each STRUCT
                for (int i = 0; i < Structs.Count(); i++)
                {
                    if (!Structs[i].Equals(other.Structs[i]))
                        return false;
                }

                // All checks passed
                return true;
            }

            /// <summary>
            /// Generate a hash code for this LIST.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                int partial_hash = 1;
                STRUCT[] StructArray = Structs.ToArray();
                foreach (STRUCT S in StructArray)
                {
                    partial_hash *= S.GetHashCode();
                }

                return new { Type, partial_hash, Label }.GetHashCode();
            }

            /// <summary>
            /// Write LIST information to string.
            /// </summary>
            /// <returns>[LIST] "Label", # of Structs = _</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, # of Structs = {Structs?.Count ?? 0}";
            }
        }
    }
} 