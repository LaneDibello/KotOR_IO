﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class WORD : FIELD
        {
            public ushort Value;

            //Construction
            public WORD() : base(GffFieldType.WORD) { }
            public WORD(string label, ushort value)
                : base(GffFieldType.WORD, label)
            {
                Value = value;
            }
            internal WORD(BinaryReader br, int offset)
                : base(GffFieldType.WORD)
            {
                //header info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();

                //Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = (GffFieldType)br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                Value = br.ReadUInt16();

                //Label Logic
                br.BaseStream.Seek(LabelOffset + LabelIndex * 16, 0);
                Label = new string(br.ReadChars(16)).TrimEnd('\0');

            }

            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
            {
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, (int)Value, this.GetHashCode());
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
                    return Value == (obj as WORD).Value && Label == (obj as WORD).Label;
                }
            }

            public override int GetHashCode()
            {
                return new { Type, Value, Label }.GetHashCode();
            }

            public override string ToString()
            {
                return $"{base.ToString()}, {Value}";
            }
        }
    }
} 