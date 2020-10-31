using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// BioWare Key Data.<para/>
    /// See: <see cref="KEY(Stream)"/>, 
    /// <seealso  cref="Write(Stream)"/>
    /// <remarks>
    /// <para/>Key files contain a large array of references with IDs that refer to existing BIF files. They serve as a sort of catalog for whenever the game needs to find a specific file. 
    /// <para/>The only, and most important, Key file in SW:KotOR is 'chitin.key' which is used to reference every single BIF file, and their contents.
    /// </remarks>
    /// </summary>
    public class KEY : KFile
    {
        /// <summary>
        /// Reads the given BioWare KEY File
        /// </summary>
        /// <param name="path"></param>
        public KEY(string path)
            : this(File.OpenRead(path))
        { }

        /// <summary>
        /// Reads Bioware Key Files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        public KEY(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                // Header
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));
                BIFCount = br.ReadInt32();
                KeyCount = br.ReadInt32();
                OffsetToFileTable = br.ReadInt32();
                OffsetToKeyTable = br.ReadInt32();
                BuildYear = br.ReadInt32();
                BuildDay = br.ReadInt32();
                Reserved = br.ReadBytes(32);

                // File Table
                br.BaseStream.Seek(OffsetToFileTable, SeekOrigin.Begin);
                for (int i = 0; i < BIFCount; i++)
                {
                    KEY.FileEntry FE = new KEY.FileEntry();
                    FE.FileSize = br.ReadInt32();
                    FE.FilenameOffset = br.ReadInt32();
                    FE.FilenameSize = br.ReadInt16();
                    FE.Drives = br.ReadInt16();
                    FileTable.Add(FE);
                }

                // Filenames
                foreach (KEY.FileEntry FE in FileTable)
                {
                    br.BaseStream.Seek(FE.FilenameOffset, SeekOrigin.Begin);
                    FE.Filename = new string(br.ReadChars(FE.FilenameSize));
                }

                // Key Table
                br.BaseStream.Seek(OffsetToKeyTable, SeekOrigin.Begin);
                for (int i = 0; i < KeyCount; i++)
                {
                    KEY.KeyEntry KE = new KEY.KeyEntry();
                    KE.ResRef = new string(br.ReadChars(16)).TrimEnd('\0');
                    KE.ResourceType = br.ReadInt16();
                    KE.Type_Text = Reference_Tables.Res_Types[KE.ResourceType];
                    KE.ResID = br.ReadInt32();
                    KE.IDx = KE.ResID >> 20;
                    KE.IDy = KE.ResID - (KE.IDx << 20);

                    KeyTable.Add(KE);
                }
            }
        }

        // FileType & Version in superclass

        /// <summary> The number of <see cref="BIF"/> files this key controls. </summary>
        public int BIFCount { get; set; }

        /// <summary> The Number of resources in all of the <see cref="BIF"/>s linked to this key. </summary>
        public int KeyCount { get; set; }

        /// <summary> Byte offset to the <see cref="FileTable"/> from beginning of the file. </summary>
        public int OffsetToFileTable { get; set; }

        /// <summary> Byte offset to the <see cref="KeyTable"/> from beginning of the file. </summary>
        public int OffsetToKeyTable { get; set; }

        /// <summary> The number of years after 1900 that the KEY file was built. (i.e. 2019 == 119) </summary>
        public int BuildYear { get; set; }

        /// <summary> The number of days after January 1st the ERF file was built. (i.e. October 5th == 277) </summary>
        public int BuildDay { get; set; }

        /// <summary> 32 (usually) empty bytes reserved for future use. </summary>
        public byte[] Reserved { get; set; }

        /// <summary> A List containing all of the <see cref="FileEntry"/>s associated with the linked <see cref="BIF"/> files. </summary>
        public List<FileEntry> FileTable { get; set; } = new List<FileEntry>();

        /// <summary> A list of all the <see cref="KeyEntry"/>s associted with the linked <see cref="BIF"/> files. </summary>
        public List<KeyEntry> KeyTable { get; set; } = new List<KeyEntry>();

        /// <summary>
        /// Writes Bioware Key File data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        internal override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                //Header
                bw.Write(FileType.ToArray());
                bw.Write(Version.ToArray());
                bw.Write(BIFCount);
                bw.Write(KeyCount);
                bw.Write(OffsetToFileTable);
                bw.Write(OffsetToKeyTable);
                bw.Write(BuildYear);
                bw.Write(BuildDay);
                bw.Write(Reserved);

                //File Table
                bw.Seek(OffsetToFileTable, SeekOrigin.Begin);
                foreach (KEY.FileEntry FE in FileTable)
                {
                    bw.Write(FE.FileSize);
                    bw.Write(FE.FilenameOffset);
                    bw.Write(FE.FilenameSize);
                    bw.Write(FE.Drives);
                }

                //Filenames
                foreach (KEY.FileEntry FE in FileTable)
                {
                    bw.Seek(FE.FilenameOffset, SeekOrigin.Begin);
                    bw.Write(FE.Filename.ToArray());
                }

                //Key Table
                bw.Seek(OffsetToKeyTable, SeekOrigin.Begin);
                foreach (KEY.KeyEntry KE in KeyTable)
                {
                    bw.Write(KE.ResRef.PadRight(16, '\0').ToArray());
                    bw.Write(KE.ResourceType);
                    bw.Write(KE.ResID);
                }
            }
        }

        /// <summary>
        /// Writes a file to the given path using the Name property in this class object.
        /// </summary>
        /// <param name="path">Path to the file to write.</param>
        public void WriteToFile(string path)
        {
            Write(File.OpenWrite(path));
        }

        // File Table
        /// <summary> An Entry that describes basic info about a <see cref="BIF"/> file. </summary>
        public class FileEntry
        {
            ///<summary>The size of the <see cref="BIF"/> file in bytes.</summary>
            public int FileSize;
            ///<summary>The byte offset from the start of the file to the <see cref="BIF"/>'s filename.</summary>
            public int FilenameOffset;
            ///<summary>The size of the filename in <see cref="char"/>s</summary>
            public short FilenameSize;
            ///<summary>A 16-bit number representing which drive the <see cref="BIF"/> is installed on.</summary>
            public short Drives;

            ///<summary>The Filename of the <see cref="BIF"/> as a path from the <see cref="KEY"/>'s root directory</summary>
            public string Filename;
        }

        // Key Table
        /// <summary> An entry containing every resources string reference, Type, and ID. </summary>
        public class KeyEntry
        {
            ///<summary>The name of the resource. (16 <see cref="char"/>s)</summary>
            public string ResRef;
            ///<summary>The Resource Type ID of this resource. See: <see cref="Reference_Tables.Res_Types"/> </summary>
            public short ResourceType;
            ///<summary>The file extension representation of this resource. Obtained from <see cref="Reference_Tables.Res_Types"/> </summary>
            public string Type_Text; //Populated from Reference_Tables.Res_types[ResourceType]
            ///<summary>
            ///A unique ID number that denotes both which BIF this resource refers to, and the index of this resource in the <see cref="BIF.VariableResourceTable"/>
            ///<para/> ResID = (x &lt;&lt; 20) + y
            ///<para/> Where y is an index into <see cref="FileTable"/> to specify a <see cref="BIF"/>, and x is an index into that <see cref="BIF"/>'s <see cref="BIF.VariableResourceTable"/>.
            /// </summary>
            public int ResID;

            ///<summary>The x component of <see cref="ResID"/> which references an index in the <see cref="FileTable"/></summary>
            public int IDx;
            ///<summary>The y component of <see cref="ResID"/> which is an index into the <see cref="BIF.VariableResourceTable"/></summary>
            public int IDy;
        }
    }
}
