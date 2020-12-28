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
            public STRUCT() { }

            public STRUCT(BinaryReader br, int index, int LabelIndex = -1)
            {
                Type = 14;

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
                    int Field_Type = br.ReadInt32();
                    FIELD data;
                    switch (Field_Type)
                    {
                        case 0:
                            data = new BYTE(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 1:
                            data = new CHAR(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 2:
                            data = new WORD(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 3:
                            data = new SHORT(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 4:
                            data = new DWORD(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 5:
                            data = new INT(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 6:
                            data = new DWORD64(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 7:
                            data = new INT64(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 8:
                            data = new FLOAT(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 9:
                            data = new DOUBLE(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 10:
                            data = new CExoString(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 11:
                            data = new ResRef(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 12:
                            data = new CExoLocString(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 13:
                            data = new VOID(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 14:
                            int lbl_index = br.ReadInt32();
                            data = new STRUCT(br, br.ReadInt32(), lbl_index);
                            break;
                        case 15:
                            data = new LIST(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 16:
                            data = new Orientation(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 17:
                            data = new Vector(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        case 18:
                            data = new StrRef(br, FieldOffset + DataOrDataOffset * 12);
                            break;
                        default:
                            throw new Exception(string.Format("UNEXPECTED FIELD TYPE \"{0}\", IN STRUCT INDEX \"{1}\"", Field_Type, index));
                    }
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
                        int Field_Type = br.ReadInt32();
                        FIELD data;
                        switch (Field_Type)
                        {
                            case 0:
                                data = new BYTE(br, FieldOffset + f_index * 12);
                                break;
                            case 1:
                                data = new CHAR(br, FieldOffset + f_index * 12);
                                break;
                            case 2:
                                data = new WORD(br, FieldOffset + f_index * 12);
                                break;
                            case 3:
                                data = new SHORT(br, FieldOffset + f_index * 12);
                                break;
                            case 4:
                                data = new DWORD(br, FieldOffset + f_index * 12);
                                break;
                            case 5:
                                data = new INT(br, FieldOffset + f_index * 12);
                                break;
                            case 6:
                                data = new DWORD64(br, FieldOffset + f_index * 12);
                                break;
                            case 7:
                                data = new INT64(br, FieldOffset + f_index * 12);
                                break;
                            case 8:
                                data = new FLOAT(br, FieldOffset + f_index * 12);
                                break;
                            case 9:
                                data = new DOUBLE(br, FieldOffset + f_index * 12);
                                break;
                            case 10:
                                data = new CExoString(br, FieldOffset + f_index * 12);
                                break;
                            case 11:
                                data = new ResRef(br, FieldOffset + f_index * 12);
                                break;
                            case 12:
                                data = new CExoLocString(br, FieldOffset + f_index * 12);
                                break;
                            case 13:
                                data = new VOID(br, FieldOffset + f_index * 12);
                                break;
                            case 14:
                                int lbl_index = br.ReadInt32();
                                data = new STRUCT(br, br.ReadInt32(), lbl_index);
                                break;
                            case 15:
                                data = new LIST(br, FieldOffset + f_index * 12);
                                break;
                            case 16:
                                data = new Orientation(br, FieldOffset + f_index * 12);
                                break;
                            case 17:
                                data = new Vector(br, FieldOffset + f_index * 12);
                                break;
                            case 18:
                                data = new StrRef(br, FieldOffset + f_index * 12);
                                break;
                            default:
                                throw new Exception(string.Format("UNEXPECTED FIELD TYPE \"{0}\", IN STRUCT INDEX \"{1}\"", Field_Type, index));
                        }
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


        }
    }
} 