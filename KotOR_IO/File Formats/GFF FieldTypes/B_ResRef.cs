using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class ResRef : FIELD
        {
            public string Reference;

            public ResRef() { }
            public ResRef(string Label, string Reference)
            {
                this.Type = 11;
                if (Label.Length > 16) { throw new Exception($"Label \"{Label}\" is longer than 16 characters, and is invalid."); }
                this.Label = Label;
                if (Reference.Length <= 16)
                {
                    this.Reference = Reference;
                }
                else
                {
                    throw new Exception($"Reference \"{Reference}\" with a length of {Reference.Length} is longer than 16, and cannot be used for GFF type ResRef");
                }
            }
            internal ResRef(BinaryReader br, int offset)
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
                byte Size = br.ReadByte();
                Reference = new string(br.ReadChars(Size));

            }

            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
            {
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, Raw_Field_Data_Block.Count, this.GetHashCode());
                Raw_Field_Data_Block.Add((byte)Reference.Length);
                Raw_Field_Data_Block.AddRange(Reference.ToCharArray().Select(x => (byte)x));
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
                    return Reference == (obj as ResRef).Reference && Label == (obj as ResRef).Label;
                }
            }

            public override int GetHashCode()
            {
                return new { Type, Reference, Label }.GetHashCode();
            }


        }
    }
} 