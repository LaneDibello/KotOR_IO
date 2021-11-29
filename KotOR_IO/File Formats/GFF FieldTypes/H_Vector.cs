using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// An Vector is a set of 3 float values representing a direction in 3D space.
        /// </summary>
        public class Vector : FIELD
        {
            /// <summary>
            /// X value.
            /// </summary>
            public float X { get; set; }

            /// <summary>
            /// Y value.
            /// </summary>
            public float Y { get; set; }

            /// <summary>
            /// Z value.
            /// </summary>
            public float Z { get; set; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public Vector() : base(GffFieldType.Vector) { }

            /// <summary>
            /// Construct with label and values.
            /// </summary>
            public Vector(string label, int x, int y, int z)
                : base(GffFieldType.Vector, label)
            {
                X = x;
                Y = y;
                Z = z;
            }

            /// <summary>
            /// Construct by reading a binary reader.
            /// </summary>
            internal Vector(BinaryReader br, int offset)
                : base(GffFieldType.Vector)
            {
                // Header Info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();
                int LabelCount = br.ReadInt32();
                int FieldDataOffset = br.ReadInt32();

                // Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = (GffFieldType)br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                int DataOrDataOffset = br.ReadInt32();

                // Label Logic
                br.BaseStream.Seek(LabelOffset + LabelIndex * 16, 0);
                Label = new string(br.ReadChars(16)).TrimEnd('\0');

                // Complex Value Logic
                br.BaseStream.Seek(FieldDataOffset + DataOrDataOffset, 0);
                X = br.ReadSingle();
                Y = br.ReadSingle();
                Z = br.ReadSingle();
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
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(X));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Y));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Z));
                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }
            }

            /// <summary>
            /// Test equality between two Vector objects.
            /// </summary>
            /// <param name="right"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                // Check null, self, type, Gff Type, and Label.
                if (!base.Equals(right))
                    return false;

                // Check values.
                var other = right as Vector;
                return X == other.X && Y == other.Y && Z == other.Z;
            }

            /// <summary>
            /// Generate a hash code for this Vector.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return new { Type, X, Y, Z, Label }.GetHashCode();
            }

            /// <summary>
            /// Write Vector information to string.
            /// </summary>
            /// <returns>[Vector] "Label", (X, Y, Z)</returns>
            public override string ToString()
            {
                return $"{base.ToString()}, ({X}, {Y}, {Z})";
            }
        }
    }
} 