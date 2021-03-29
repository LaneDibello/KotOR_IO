using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class Orientation : FIELD
        {
            public float Value1;
            public float Value2;
            public float Value3;
            public float Value4;

            public Orientation() : base(GffFieldType.Orientation) { }
            public Orientation(string label, int value1, int value2, int value3, int value4)
                : base(GffFieldType.Orientation, label)
            {
                Value1 = value1;
                Value2 = value2;
                Value3 = value3;
                Value4 = value4;
            }
            internal Orientation(BinaryReader br, int offset)
                : base(GffFieldType.Orientation)
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
                Value1 = br.ReadSingle();
                Value2 = br.ReadSingle();
                Value3 = br.ReadSingle();
                Value4 = br.ReadSingle();

            }

            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
            {
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, Raw_Field_Data_Block.Count, this.GetHashCode());
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Value1));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Value2));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Value3));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Value4));
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
                    return Value1 == (obj as Orientation).Value1 && Value2 == (obj as Orientation).Value2 && Value3 == (obj as Orientation).Value3 && Value4 == (obj as Orientation).Value4 && Label == (obj as Orientation).Label;
                }
            }

            public override int GetHashCode()
            {
                return new { Type, Value1, Value2, Value3, Value4, Label }.GetHashCode();
            }

            public override string ToString()
            {
                return $"{base.ToString()}, ({Value1}, {Value2}, {Value3}, {Value4})";
            }
        }
    }
} 