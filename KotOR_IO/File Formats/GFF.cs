using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    /// <summary>
    /// General File Format - The Format used for about 50% of the files in this game.
    /// Including, but not limited to: Creatures, Doors, Placeables, Items, Dialogue,
    /// GUIs, and Module Layouts.
    /// </summary>
    public partial class GFF : KFile
    {
        /// <summary> Bytes in an int. </summary>
        protected const int SIZEOF_INT = 4;

        /// <summary>
        /// Top level of this container.
        /// </summary>
        public STRUCT Top_Level { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GFF()
        {
            Top_Level = new STRUCT();
        }

        /// <summary>
        /// Construct GFF using a custom top level STRUCT.
        /// </summary>
        public GFF(string FileType, string Version, STRUCT Top_Level)
        {
            this.FileType = FileType;
            this.Version = Version;
            this.Top_Level = Top_Level;
        }

        /// <summary>
        /// Construct GFF by reading a raw byte[] data stream.
        /// </summary>
        public GFF(byte[] rawData)
            : this(new MemoryStream(rawData))
        { }

        /// <summary>
        /// Construct GFF by reading file at path.
        /// </summary>
        public GFF(string path)
            : this(File.OpenRead(path))
        { }

        /// <summary>
        /// Construct GFF by reading a generic data stream.
        /// </summary>
        public GFF(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));

                // Begin reading top level STRUCT data.
                Top_Level = new STRUCT(br, 0);
            }

        }

        /// <summary>
        /// Write GFF data to a generic data stream.
        /// </summary>
        internal override void Write(Stream s)
        {
            // List out all the fields in the file, followed by their DataOrDataOffset, and then their HasCode.
            List<Tuple<FIELD, int, int>> Field_Array = new List<Tuple<FIELD, int, int>>();

            // Contains all the unique labels used by the file.
            List<string> Label_Array = new List<string>();

            // Raw Arrays that will store the bytes to be written to the File.
            List<byte> Raw_Struct_Array = new List<byte>();
            List<byte> Raw_Field_Array = new List<byte>();
            List<byte> Raw_Label_Array = new List<byte>();
            List<byte> Raw_Field_Data_Block = new List<byte>();
            List<byte> Raw_Field_Indices_Array = new List<byte>();
            List<byte> Raw_List_Indices_Array = new List<byte>();

            // Indexers/Counters
            int Struct_Indexer = 0;
            int List_Indices_Counter = 0;

            // Recursive field collection call
            Top_Level.collect_fields(ref Field_Array, ref Raw_Field_Data_Block, ref Label_Array, ref Struct_Indexer, ref List_Indices_Counter);

            // Preparing raw struct data
            for (int i = 0; i < Struct_Indexer; i++)
            {
                STRUCT S = Field_Array.Where(x => x.Item1.Type == GffFieldType.Struct && x.Item2 == i).Select(x => x.Item1 as STRUCT).FirstOrDefault();
                Raw_Struct_Array.AddRange(BitConverter.GetBytes(S.Struct_Type));
                int f_count = S.Fields.Count;
                if (f_count == 1)
                {
                    int f_hash = S.Fields[0].GetHashCode();
                    int f_index = Field_Array.FindIndex(x => x.Item3 == f_hash);

                    Raw_Struct_Array.AddRange(BitConverter.GetBytes(f_index));
                }
                else if (f_count > 1)
                {
                    Raw_Struct_Array.AddRange(BitConverter.GetBytes(Raw_Field_Indices_Array.Count));
                    foreach (FIELD F in S.Fields)
                    {
                        int f_hash = F.GetHashCode();
                        int f_index = Field_Array.FindIndex(x => x.Item3 == f_hash);

                        if (f_index == -1) { throw new Exception("Bad field index, was their a hashing issue?"); }

                        Raw_Field_Indices_Array.AddRange(BitConverter.GetBytes(f_index));
                    }

                    // Depricated code for when I was using a counter and handling Field Indices separately
                    //Raw_Struct_Array.AddRange(BitConverter.GetBytes(Field_Indices_Counter));
                    //Field_Indices_Counter += 4 * S.Fields.Count;
                }
                else
                {
                    // Empty Struct case (this is rare)
                    Raw_Struct_Array.AddRange(BitConverter.GetBytes(-1));
                }

                Raw_Struct_Array.AddRange(BitConverter.GetBytes(f_count));
            }

            // Preparing raw Field data
            foreach (Tuple<FIELD, int, int> T in Field_Array)
            {
                Raw_Field_Array.AddRange(BitConverter.GetBytes((int)T.Item1.Type)); //Field Type
                int lbl_index = Label_Array.IndexOf(T.Item1.Label); //Find Label index
                Raw_Field_Array.AddRange(BitConverter.GetBytes(lbl_index)); //Label index
                Raw_Field_Array.AddRange(BitConverter.GetBytes(T.Item2)); //DataOrDataOffset
            }

            // Preparing raw Label data
            foreach (string l in Label_Array)
            {
                Raw_Label_Array.AddRange(l.PadRight(16, '\0').ToCharArray().Select(x => (byte)x));
            }

            // Field Data block should've been prapared during collect_fields call
            // Field Indice block should've been prapared while generating struct data

            // Preparing raw List indices data
            for (int i = 0; i < List_Indices_Counter; i = Raw_List_Indices_Array.Count)
            {
                LIST L = Field_Array.Where(x => x.Item1.Type == GffFieldType.List && x.Item2 == Raw_List_Indices_Array.Count).Select(x => x.Item1 as LIST).FirstOrDefault();
                Raw_List_Indices_Array.AddRange(BitConverter.GetBytes(L.Structs.Count));
                foreach (STRUCT S in L.Structs)
                {
                    int s_hash = S.GetHashCode();
                    int s_index = Field_Array.Where(x => x.Item3 == s_hash).FirstOrDefault().Item2;
                    Raw_List_Indices_Array.AddRange(BitConverter.GetBytes(s_index));
                }
            }

            // Header Calculations
            // 56 bytes in the header: 12 ints (4 bytes each) + type and version (4 bytes each)
            int StructOffset = 12 * SIZEOF_INT + SIZEOF_FILEINFO;
            int StructCount = Raw_Struct_Array.Count / 12;
            int FieldOffset = StructOffset + StructCount * 12;
            int FieldCount = Field_Array.Count;
            int LabelOffset = FieldOffset + FieldCount * 12;
            int LabelCount = Label_Array.Count;
            int FieldDataOffset = LabelOffset + LabelCount * 16;
            int FieldDataCount = Raw_Field_Data_Block.Count;
            int FieldIndicesOffset = FieldDataOffset + FieldDataCount;
            int FieldIndicesCount = Raw_Field_Indices_Array.Count;
            int ListIndicesOffset = FieldIndicesOffset + FieldIndicesCount;
            int ListIndicesCount = Raw_List_Indices_Array.Count;

            // Writing
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                // Write header information
                bw.Write(FileType.ToCharArray());
                bw.Write(Version.ToCharArray());
                bw.Write(StructOffset);
                bw.Write(StructCount);
                bw.Write(FieldOffset);
                bw.Write(FieldCount);
                bw.Write(LabelOffset);
                bw.Write(LabelCount);
                bw.Write(FieldDataOffset);
                bw.Write(FieldDataCount);
                bw.Write(FieldIndicesOffset);
                bw.Write(FieldIndicesCount);
                bw.Write(ListIndicesOffset);
                bw.Write(ListIndicesCount);

                // Write data
                bw.Write(Raw_Struct_Array.ToArray());           // structs
                bw.Write(Raw_Field_Array.ToArray());            // fields
                bw.Write(Raw_Label_Array.ToArray());            // labels
                bw.Write(Raw_Field_Data_Block.ToArray());       // field data
                bw.Write(Raw_Field_Indices_Array.ToArray());    // fields indices
                bw.Write(Raw_List_Indices_Array.ToArray());     // list indices
            }
        }

        /// <summary>
        /// Write GFF data to a new file at the given path.
        /// </summary>
        public override void WriteToFile(string path)
        {
            Write(File.OpenWrite(path));
        }

        /// <summary>
        /// Write GFF data to a raw byte[].
        /// </summary>
        /// <returns>byte[] of GFF data</returns>
        public override byte[] ToRawData()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Write(ms);
                return ms.ToArray(); // Stream is closed, but the array is still available.
            }
        }
    }
}