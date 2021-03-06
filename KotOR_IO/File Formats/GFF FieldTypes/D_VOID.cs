﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class VOID : FIELD
        {
            public List<byte> Data = new List<byte>();

            public VOID() : base(GffFieldType.VOID) { }
            public VOID(string label, List<byte> data)
                : base(GffFieldType.VOID, label)
            {
                Label = label;
                Data = data;
            }
            internal VOID(BinaryReader br, int offset)
                : base(GffFieldType.VOID)
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
                int Size = br.ReadInt32();
                for (int i = 0; i < Size; i++)
                {
                    Data.Add(br.ReadByte());
                }

            }

            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
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

            public override bool Equals(object obj)
            {
                if ((obj == null) || !GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    return Data == (obj as VOID).Data && Label == (obj as VOID).Label;
                }
            }

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

            public override string ToString()
            {
                return $"{base.ToString()}, Size of Data = {Data?.Count ?? 0}";
            }
        }
    }
} 