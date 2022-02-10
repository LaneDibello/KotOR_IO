using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// A ResRef is a string used to store the name of a file used by the game or toolset.
        /// Up to 16 characters, not null-terminated, non-case-sensitive, and stored in all lower-case.
        /// </summary>
        public class ResRef : FIELD
        {
            /// <summary>
            /// String up to 16 characters long.
            /// </summary>
            public string Reference
            {
                get => _reference;
                set => _reference = value.ToLower();
            }
            private string _reference;

            /// <summary>
            /// Default constructor.
            /// </summary>
            public ResRef() : base(GffFieldType.ResRef) { }

            /// <summary>
            /// Construct with label and value.
            /// </summary>
            /// <param name="label"></param>
            /// <param name="reference"></param>
            public ResRef(string label, string reference)
                : base(GffFieldType.ResRef, label)
            {
                if (reference.Length > MAX_LABEL_LENGTH)
                    throw new Exception($"Reference \"{reference}\" with a length of {reference.Length} is longer than {MAX_LABEL_LENGTH} characters, and cannot be used for GFF type ResRef");
                Reference = reference;
            }

            /// <summary>
            /// Construct by reading a binary reader.
            /// </summary>
            /// <param name="br"></param>
            /// <param name="offset"></param>
            internal ResRef(BinaryReader br, int offset)
                : base(GffFieldType.ResRef)
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
                byte Size = br.ReadByte();
                Reference = new string(br.ReadChars(Size));
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
                Raw_Field_Data_Block.Add((byte)Reference.Length);
                Raw_Field_Data_Block.AddRange(Reference.ToCharArray().Select(x => (byte)x));
                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }
            }

            /// <summary>
            /// Test equality between two ResRef objects.
            /// </summary>
            /// <param name="right"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label
                if (!base.Equals(right))
                    return false;

                // Check value
                //return Reference.Equals((right as ResRef).Reference, StringComparison.InvariantCultureIgnoreCase);
                return Reference == (right as ResRef).Reference;
            }

            /// <summary>
            /// Generate a hash code for this ResRef.
            /// </summary>
            /// <returns></returns>
            //public override int GetHashCode()
            //{
            //    return new { Type, Reference, Label }.GetHashCode();
            //}

            /// <summary>
            /// Write ResRef information to string.
            /// </summary>
            /// <returns>[ResRef] "Label", "String"</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, \"{Reference}\"";
            }
        }
    }
} 