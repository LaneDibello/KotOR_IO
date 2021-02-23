using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class Vector : FIELD
        {
            public float X;
            public float Y;
            public float Z;

            public Vector() : base(GffFieldType.Vector) { }
            public Vector(string label, int x, int y, int z)
                : base(GffFieldType.Vector, label)
            {
                X = x;
                Y = y;
                Z = z;
            }
            internal Vector(BinaryReader br, int offset)
                : base(GffFieldType.Vector)
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
                X = br.ReadSingle();
                Y = br.ReadSingle();
                Z = br.ReadSingle();

            }

            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
            {
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, Raw_Field_Data_Block.Count, this.GetHashCode());
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(X));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Y));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Z));
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
                    return X == (obj as Vector).X && Y == (obj as Vector).Y && Z == (obj as Vector).Z && Label == (obj as Vector).Label;
                }
            }

            public override int GetHashCode()
            {
                return new { Type, X, Y, Z, Label }.GetHashCode();
            }

            public override string ToString()
            {
                return $"{base.ToString()}, ({X}, {Y}, {Z})";
            }
        }
    }
} 