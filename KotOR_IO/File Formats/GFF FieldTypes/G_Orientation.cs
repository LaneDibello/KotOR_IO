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
            public float value1;
            public float value2;
            public float value3;
            public float value4;

            public Orientation() { }
            public Orientation(string Label, int value1, int value2, int value3, int value4)
            {
                this.Type = 16;
                if (Label.Length > 16) { throw new Exception($"Label \"{Label}\" is longer than 16 characters, and is invalid."); }
                this.Label = Label;
                this.value1 = value1;
                this.value2 = value2;
                this.value3 = value3;
                this.value4 = value4;
            }
            internal Orientation(BinaryReader br, int offset)
            {
                //Header Info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();
                int LabelCount = br.ReadInt32();
                int FieldDataOffset = br.ReadInt32();

                //Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                int DataOrDataOffset = br.ReadInt32();

                //Label Logic
                br.BaseStream.Seek(LabelOffset + LabelIndex * 16, 0);
                Label = new string(br.ReadChars(16)).TrimEnd('\0');

                //Comlex Value Logic
                br.BaseStream.Seek(FieldDataOffset + DataOrDataOffset, 0);
                value1 = br.ReadSingle();
                value2 = br.ReadSingle();
                value3 = br.ReadSingle();
                value4 = br.ReadSingle();

            }

            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
            {
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, Raw_Field_Data_Block.Count, this.GetHashCode());
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(value1));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(value2));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(value3));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(value4));
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
                    return value1 == (obj as Orientation).value1 && value2 == (obj as Orientation).value2 && value3 == (obj as Orientation).value3 && value4 == (obj as Orientation).value4 && Label == (obj as Orientation).Label;
                }
            }

            public override int GetHashCode()
            {
                return new { Type, value1, value2, value3, value4, Label }.GetHashCode();
            }


        }
    }
} 