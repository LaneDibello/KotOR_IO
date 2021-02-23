using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class STRUCT : FIELD
        {
            //Declarations
            public int Struct_Type;

            public List<FIELD> Fields = new List<FIELD>();

            //Construction
            public STRUCT() : base(GffFieldType.Struct) { }
            public STRUCT(string label, int structType, List<FIELD> fields)
                : base(GffFieldType.Struct, label)
            {
                Struct_Type = structType;
                Fields = fields;
            }
            internal STRUCT(BinaryReader br, int index, int LabelIndex = -1)
                : base(GffFieldType.Struct)
            {
                //Header Info
                br.BaseStream.Seek(8, 0);
                int StructOffset = br.ReadInt32();
                int StuctCount = br.ReadInt32();
                int FieldOffset = br.ReadInt32();
                int FieldCount = br.ReadInt32();
                int LabelOffset = br.ReadInt32();
                int LabelCount = br.ReadInt32();
                int FieldDataOffset = br.ReadInt32();
                int FieldDataCount = br.ReadInt32();
                int FieldIndicesOffset = br.ReadInt32();

                //label logic
                if (LabelIndex < 0 || LabelIndex > LabelCount)
                {
                    Label = "";
                }
                else
                {
                    br.BaseStream.Seek(LabelOffset + LabelIndex * 16, 0);
                    Label = new string(br.ReadChars(16)).TrimEnd('\0');
                }

                //Struct Info
                br.BaseStream.Seek(StructOffset + index * 12, 0);
                Struct_Type = br.ReadInt32();
                int DataOrDataOffset = br.ReadInt32();
                int S_FieldCount = br.ReadInt32();

                //If there is only one field, it pulls it from the field area
                if (S_FieldCount == 1)
                {
                    br.BaseStream.Seek(FieldOffset + DataOrDataOffset * 12, 0);
                    GffFieldType fieldType = (GffFieldType)br.ReadInt32();
                    FIELD data = CreateNewField(br, index, FieldOffset, DataOrDataOffset, fieldType);
                    Fields.Add(data);
                }
                //If there is more than one field, a set of Indices are read off, and then those fields are read in
                else if (S_FieldCount > 1)
                {
                    for (int i = 0; i < S_FieldCount; i++)
                    {
                        br.BaseStream.Seek(FieldIndicesOffset + DataOrDataOffset + 4 * i, 0);
                        int f_index = br.ReadInt32();

                        br.BaseStream.Seek(FieldOffset + f_index * 12, 0);
                        GffFieldType fieldType = (GffFieldType)br.ReadInt32();
                        FIELD data = CreateNewField(br, index, FieldOffset, f_index, fieldType);
                        Fields.Add(data);
                    }
                }
                else if (S_FieldCount == 0)
                {
                    return;
                }
                else
                {
                    throw new Exception(string.Format("BAD FIELD COUNT \"{0}\", IN STRUCT INDEX \"{1}\"", S_FieldCount, index));
                }
            }

            private static FIELD CreateNewField(BinaryReader br, int index, int fieldOffset, int dataOrDataOffset, GffFieldType fieldType)
            {
                FIELD data;
                switch (fieldType)
                {
                    case GffFieldType.BYTE:
                        data = new BYTE(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.CHAR:
                        data = new CHAR(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.WORD:
                        data = new WORD(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.SHORT:
                        data = new SHORT(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.DWORD:
                        data = new DWORD(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.INT:
                        data = new INT(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.DWORD64:
                        data = new DWORD64(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.INT64:
                        data = new INT64(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.FLOAT:
                        data = new FLOAT(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.DOUBLE:
                        data = new DOUBLE(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.CExoString:
                        data = new CExoString(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.ResRef:
                        data = new ResRef(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.CExoLocString:
                        data = new CExoLocString(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.VOID:
                        data = new VOID(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.Struct:
                        int lbl_index = br.ReadInt32();
                        data = new STRUCT(br, br.ReadInt32(), lbl_index);
                        break;
                    case GffFieldType.List:
                        data = new LIST(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.Orientation:
                        data = new Orientation(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.Vector:
                        data = new Vector(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    case GffFieldType.StrRef:
                        data = new StrRef(br, fieldOffset + dataOrDataOffset * 12);
                        break;
                    default:
                        throw new Exception(string.Format("UNEXPECTED FIELD TYPE \"{0}\", IN STRUCT INDEX \"{1}\"", fieldType, index));
                }

                return data;
            }



            //Other
            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
            {
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, Struct_Indexer, this.GetHashCode());
                Struct_Indexer++;
                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }

                foreach (FIELD F in Fields)
                {
                    F.collect_fields(ref Field_Array, ref Raw_Field_Data_Block, ref Label_Array, ref Struct_Indexer, ref List_Indices_Counter);
                }
            }

            public override bool Equals(object obj)
            {
                if ((obj == null) || !GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    if (Fields.Count() != (obj as STRUCT).Fields.Count())
                    {
                        return false;
                    }
                    else if (Struct_Type != (obj as STRUCT).Struct_Type)
                    {
                        return false;
                    }
                    else if (Label != (obj as STRUCT).Label)
                    {
                        return false;
                    }
                    else
                    {
                        foreach (FIELD f in Fields)
                        {
                            if ((obj as STRUCT).Fields.Contains(f))
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
            }

            public override int GetHashCode()
            {
                int partial_hash = 1;
                FIELD[] fieldArray = Fields.ToArray();
                foreach (FIELD F in fieldArray)
                {
                    partial_hash *= F.GetHashCode();
                }
                return new { Type, Struct_Type, partial_hash, Label }.GetHashCode();
            }

            public override string ToString()
            {
                return $"{base.ToString()}, Struct_Type = {Struct_Type}, # of Fields = {Fields?.Count ?? 0}";
            }
        }
    }
} 