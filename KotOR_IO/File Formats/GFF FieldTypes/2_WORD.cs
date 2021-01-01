using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class WORD : FIELD
        {
            public ushort value;

            //Construction
            public WORD() { }
            public WORD(string Label, ushort value)
            {
                this.Type = 2;
                if (Label.Length > 16) { throw new Exception($"Label \"{Label}\" is longer than 16 characters, and is invalid."); }
                this.Label = Label;
                this.value = value;
            }
            internal WORD(BinaryReader br, int offset)
            {
                //header info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();

                //Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                value = br.ReadUInt16();

                //Label Logic
                br.BaseStream.Seek(LabelOffset + LabelIndex * 16, 0);
                Label = new string(br.ReadChars(16)).TrimEnd('\0');

            }

            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
            {
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, (int)value, this.GetHashCode());
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
                    return value == (obj as WORD).value && Label == (obj as WORD).Label;
                }
            }

            public override int GetHashCode()
            {
                return new { Type, value, Label }.GetHashCode();
            }


        }
    }
} 