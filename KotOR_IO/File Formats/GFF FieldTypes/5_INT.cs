using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class INT : FIELD
        {
            public int Value;

            //Construction
            public INT() : base(GffFieldType.INT) { }
            public INT(string label, int value)
                : base(GffFieldType.INT, label)
            {
                Value = value;
            }
            internal INT(BinaryReader br, int offset)
                : base(GffFieldType.INT)
            {
                //header info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();

                //Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = (GffFieldType)br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                Value = br.ReadInt32();

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
                    return Value == (obj as INT).Value && Label == (obj as INT).Label;
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