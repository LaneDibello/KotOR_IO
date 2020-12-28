using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class LIST : FIELD
        {
            public List<STRUCT> Structs = new List<STRUCT>();

            public LIST() { }
            public LIST(BinaryReader br, int offset)
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
                Type = br.ReadInt32();
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

            //Other
            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
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

            public override bool Equals(object obj)
            {
                if ((obj == null) || !GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else if (Structs.Count() != (obj as LIST).Structs.Count())
                {
                    return false;
                }
                else if (Label != (obj as LIST).Label)
                {
                    return false;
                }
                else
                {
                    for (int i = 0; i < Structs.Count(); i++)
                    {
                        if (Structs[i].Equals((obj as LIST).Structs[i]))
                        {
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }

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


        }
    }
} 