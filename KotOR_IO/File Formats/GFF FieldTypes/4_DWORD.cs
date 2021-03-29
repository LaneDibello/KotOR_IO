using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class DWORD : FIELD
        {
            public uint Value;

            //Construction
            public DWORD() : base(GffFieldType.DWORD) { }
            public DWORD(string label, uint value)
                : base(GffFieldType.DWORD, label)
            {
                Value = value;
            }
            internal DWORD(BinaryReader br, int offset)
                : base(GffFieldType.DWORD)
            {
                //header info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();

                //Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = (GffFieldType)br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                Value = br.ReadUInt32();

                //Label Logic
                br.BaseStream.Seek(LabelOffset + LabelIndex * 16, 0);
                Label = new string(br.ReadChars(16)).TrimEnd('\0');

            }

            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
            {
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, BitConverter.ToInt32((BitConverter.GetBytes(Value)), 0), this.GetHashCode());
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
                    return Value == (obj as DWORD).Value && Label == (obj as DWORD).Label;
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