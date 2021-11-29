using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// An StrRef is a 
        /// </summary>
        public class StrRef : FIELD
        {
            /// <summary>
            /// 
            /// </summary>
            public int LeadingValue { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int Reference { get; set; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public StrRef() : base(GffFieldType.StrRef) { }

            /// <summary>
            /// Construct with label and values.
            /// </summary>
            public StrRef(string label, int leading_value, int reference)
                : base(GffFieldType.StrRef, label)
            {
                LeadingValue = leading_value;
                Reference = reference;
            }

            /// <summary>
            /// Construct by reading a binary reader.
            /// </summary>
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

            /// <summary>
            /// Collect fields recursively.
            /// </summary>
            /// <param name="Field_Array"></param>
            /// <param name="Raw_Field_Data_Block"></param>
            /// <param name="Label_Array"></param>
            /// <param name="Struct_Indexer"></param>
            /// <param name="List_Indices_Counter"></param>
            internal override void collect_fields(
                ref List<Tuple<FIELD, int, int>> Field_Array,
                ref List<byte> Raw_Field_Data_Block,
                ref List<string> Label_Array,
                ref int Struct_Indexer,
                ref int List_Indices_Counter)
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

            /// <summary>
            /// Test equality between two StrRef objects.
            /// </summary>
            /// <param name="right"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label.
                if (!base.Equals(right))
                    return false;

                // Check values.
                var other = right as StrRef;
                return LeadingValue == other.LeadingValue && Reference == other.Reference;
            }

            /// <summary>
            /// Generate a hash code for this StrRef.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return new { Type, LeadingValue, Reference, Label }.GetHashCode();
            }

            /// <summary>
            /// Write StrRef information to string.
            /// </summary>
            /// <returns>[StrRef] "Label", Leading Value = _, StrRef = _</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, Leading Value = {LeadingValue}, StrRef = {Reference}";
            }
        }
    }
} 