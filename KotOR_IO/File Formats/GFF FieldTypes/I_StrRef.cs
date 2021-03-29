using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class StrRef : FIELD
        {
            public int LeadingValue;
            public int Reference;

            public StrRef() : base(GffFieldType.StrRef) { }
            public StrRef(string label, int leading_value, int reference)
                : base(GffFieldType.StrRef, label)
            {
                LeadingValue = leading_value;
                Reference = reference;
            }
            internal StrRef(BinaryReader br, int offset)
                : base(GffFieldType.StrRef)
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
                LeadingValue = br.ReadInt32();
                Reference = br.ReadInt32();

            }

            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
            {
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, Raw_Field_Data_Block.Count, this.GetHashCode());
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(LeadingValue));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Reference));
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
                    return LeadingValue == (obj as StrRef).LeadingValue && Reference == (obj as StrRef).Reference && Label == (obj as StrRef).Label;
                }
            }

            public override int GetHashCode()
            {
                return new { Type, LeadingValue, Reference, Label }.GetHashCode();
            }

            public override string ToString()
            {
                return $"{base.ToString()}, Leading Value = {LeadingValue}, StrRef = {Reference}";
            }
        }
    }
} 