using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// BioWare 'RIM' Data.<para/>
    /// See: <see cref="RIM(Stream)"/>, 
    /// <seealso  cref="Write(Stream)"/>
    /// <remarks>
    /// <para/>RIM data stores bulk data that is not active, nor mutable. It is primarily used as the base template for each module upon load-in, where the data is then exported to an ERF file by the engine for faster future reading.
    /// <para/>*NOTE: RIM files are not well documented, so support for issues with this class may be limited.
    /// </remarks>
    /// </summary>
    public class RIM : KFile
    {
        /// <summary>
        /// Initiates a new instance of the <see cref="RIM"/> class from raw byte data.
        /// </summary>
        /// <param name="rawData">A byte array containing the file data.</param>
        public RIM(byte[] rawData)
            : this(new MemoryStream(rawData))
        { }

        /// <summary>
        /// Reads the given BioWare RIM File.
        /// </summary>
        /// <param name="path">File path to read.</param>
        public RIM(string path)
            : this(File.OpenRead(path))
        { }

        /// <summary>
        /// Reads Bioware "RIM" Files.
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        protected RIM(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                // Header
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));
                Unknown = br.ReadBytes(4);
                FileCount = br.ReadInt32();
                File_Table_Offset = br.ReadInt32();
                IsExtension = br.ReadBoolean();
                Reserved = br.ReadBytes(99);

                br.BaseStream.Seek(File_Table_Offset, SeekOrigin.Begin);
                // File Table
                for (int i = 0; i < FileCount; i++)
                {
                    rFile RF = new rFile();
                    RF.Label = new string(br.ReadChars(16)).TrimEnd('\0');
                    RF.TypeID = br.ReadInt32();
                    RF.Index = br.ReadInt32();
                    RF.DataOffset = br.ReadInt32();
                    RF.DataSize = br.ReadInt32();
                    File_Table.Add(RF);
                }

                // Populate FileData
                foreach (rFile RF in File_Table)
                {
                    br.BaseStream.Seek(RF.DataOffset, SeekOrigin.Begin);
                    RF.File_Data = br.ReadBytes(RF.DataSize + 4); // Add an extra four bytes of padding into the null separater to eleminate size bound errors.
                }
            }
        }

        // FileType & Version in superclass

        /// <summary>
        /// 4 bytes that appear to be null in every <see cref="RIM"/> I've come across so far.
        /// </summary>
        public byte[] Unknown;

        /// <summary>
        /// The number of files contained within the <see cref="RIM"/>.
        /// </summary>
        public int FileCount;

        /// <summary>
        /// Byte offset from start of the file to the <see cref="File_Table"/>.
        /// </summary>
        public int File_Table_Offset;

        /// <summary>
        /// Denotes a <see cref="RIM"/> that is an extension to another (marked by a filename ending in 'x').
        /// </summary>
        public bool IsExtension;

        /// <summary>
        /// 99 empty bytes, probably reserved for backwards compatibility.
        /// </summary>
        public byte[] Reserved;

        /// <summary>
        /// All of the <see cref="rFile"/>s contained within the <see cref="RIM"/>. This is the primary property of the <see cref="RIM"/>.
        /// </summary>
        public List<rFile> File_Table = new List<rFile>();

        /// <summary>
        /// Gets byte data from the RIM from the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte[] this[int index]
        {
            get
            {
                return File_Table[index].File_Data;
            }
        }

        /// <summary>
        /// Gets byte data from the RIM with the given filename.
        /// </summary>
        /// <param name="filename">The Name (res_ref) of the file being referenced</param>
        /// <returns></returns>
        public byte[] this[string filename]
        {
            get
            {
                return File_Table.Where(rf => rf.Label == filename).FirstOrDefault().File_Data;
            }
        }

        /// <summary>
        /// Appends a new KFile to the end of the <see cref="RIM"/>.
        /// </summary>
        /// <param name="file">The <see cref="KFile"/> to be added.</param>
        /// <param name="filename">The name of the file.</param>
        public void Append_File(KFile file, string filename)
        {
            FileCount++;

            foreach (rFile r in File_Table)
            {
                r.DataOffset += 32;
            }

            rFile rf = new rFile();
            rf.Label = filename;
            rf.TypeID = Reference_Tables.TypeCodes[file.FileType];
            rf.Index = File_Table.Count();

            using (MemoryStream ms = new MemoryStream())
            {
                file.Write(ms);
                rf.File_Data = ms.ToArray();
            }

            rf.DataSize = rf.File_Data.Count();
            rf.DataOffset = File_Table.Last().DataOffset + File_Table.Last().DataSize + 16;

            File_Table.Add(rf);
        }

        /// <summary>
        /// Returns a formatted <see cref="KFile"/> read form the resource at the given index.
        /// </summary>
        /// <param name="index">The index of the file to be returned</param>
        /// <returns></returns>
        public KFile GetKFile(int index)
        {
            var file = File_Table[index];
            switch ((ResourceType)file.TypeID)
            {
                case ResourceType.TwoDA:
                    return new TwoDA(file.File_Data);

                case ResourceType.TLK:
                    return new TLK(file.File_Data);

                case ResourceType.SSF:
                    return new SSF(file.File_Data);

                case ResourceType.RIM:
                    return new RIM(file.File_Data);

                case ResourceType.BIF:
                    return new BIF(file.File_Data);

                default:
                    if (Reference_Tables.ERFResourceTypes.Contains((ResourceType)file.TypeID))
                    {
                        return new ERF(file.File_Data);
                    }
                    else if (Reference_Tables.GFFResourceTypes.Contains((ResourceType)file.TypeID))
                    {
                        return new GFF(file.File_Data);
                    }
                    else
                    {
                        return new MiscType(file.File_Data);
                    }
            }
        }

        /// <summary>
        /// Writes Bioware 'RIM' data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        internal override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                //Header
                bw.Write(FileType.ToArray());
                bw.Write(Version.ToArray());
                bw.Write(Unknown);
                bw.Write(FileCount);
                bw.Write(File_Table_Offset);
                bw.Write(IsExtension);

                bw.Write(Reserved);

                //File Table
                bw.Seek(File_Table_Offset, SeekOrigin.Begin);
                foreach (rFile RF in File_Table)
                {
                    bw.Write(RF.Label.PadRight(16, '\0').ToArray());
                    bw.Write(RF.TypeID);
                    bw.Write(RF.Index);
                    bw.Write(RF.DataOffset);
                    bw.Write(RF.DataSize);
                }

                //File Data
                foreach (rFile RF in File_Table)
                {
                    bw.Seek(RF.DataOffset, SeekOrigin.Begin);
                    bw.Write(RF.File_Data);
                }

                //Padding the end with 6 bytes because kotor likes that for some reason.
                bw.Seek(0, SeekOrigin.End);
                byte[] padend = new byte[6] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
                bw.Write(padend);
            }
        }

        // File Table
        /// <summary>
        /// A File contained within the <see cref="RIM"/>.
        /// </summary>
        public class rFile // todo: create constructor?
        {
            ///<summary> The file's name. (max 16 <see cref="char"/>s) </summary>
            public string Label;
            ///<summary> The type ID from <see cref="Reference_Tables.Res_Types"/>. </summary>
            public int TypeID;
            ///<summary> The Index of this file in <see cref="File_Table"/>. </summary>
            public int Index;
            ///<summary> Byte offset of <see cref="File_Data"/> from start of the <see cref="RIM"/>. </summary>
            public int DataOffset;
            ///<summary> The size of <see cref="File_Data"/> in bytes. </summary>
            public int DataSize;
            ///<summary> The data contained within this file. </summary>
            public byte[] File_Data; // Populated from the FileData block.
        }
    }
}
