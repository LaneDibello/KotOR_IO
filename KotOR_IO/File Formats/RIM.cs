using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO.File_Formats
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
        #region Nested Classes

        //File Table
        ///<summary>A File contained within the <see cref="RIM"/></summary>
        public class rFile
        {
            ///<summary>The file's name. (max 16 <see cref="char"/>s)</summary>
            public string Label;
            ///<summary>The type ID from <see cref="Reference_Tables.Res_Types"/></summary>
            public int TypeID;
            ///<summary>The Index of this file in <see cref="File_Table"/></summary>
            public int Index;
            ///<summary>Byte offset of <see cref="File_Data"/> from start of the <see cref="RIM"/></summary>
            public int DataOffset;
            ///<summary>The size of <see cref="File_Data"/> in bytes</summary>
            public int DataSize;

            ///<summary>The data contained within this file</summary>
            public byte[] File_Data; //populated from the FileData block
        }

        #endregion

        #region Constructors

        ///<summary>Initiates a new instance of the <see cref="RIM"/> class.</summary>
        public RIM() { }

        /// <summary>
        /// Initiates a new instance of the <see cref="RIM"/> class, given a set of Kotor source files, and filenames.
        /// </summary>
        /// <param name="files">An array of <see cref="KFile"/>s containing the resources to be added to be put in the <see cref="RIM"/></param>
        /// <param name="filenames">The names of the <see cref="KFile"/>s being added.</param>
        /// <param name="_IsExtension">Indicates whether this <see cref="RIM"/> is an extension of another. (Usually indicated by a RIM filename ending in 'x', leave "false" if unsure.) </param>
        public RIM(KFile[] files, string[] filenames, bool _IsExtension)
        {
            FileType = "RIM ";
            Version = "V1.0";

            Unknown = new byte[] { 0x0, 0x0, 0x0, 0x0 };
            FileCount = files.Count();
            File_Table_Offset = 120;
            IsExtension = _IsExtension;

            Reserved = new byte[99];
            for (int i = 0; i < 99; i++) { Reserved[i] = 0x0; }

            int totalOffset = File_Table_Offset + (32 * files.Count()) + 8;
            for (int i = 0; i < files.Count(); i++)
            {
                rFile rf = new rFile();
                rf.Label = filenames[i];
                rf.TypeID = Reference_Tables.TypeCodes[files[i].FileType];
                rf.Index = i;

                MemoryStream ms = new MemoryStream();

                files[i].Write(ms);
                //if (files[i] is TwoDA) { kWriter.Write(files[i] as TwoDA, ms); }
                //else if (files[i] is BIF) { kWriter.Write(files[i] as BIF, ms); }
                //else if (files[i] is ERF) { kWriter.Write(files[i] as ERF, ms); }
                //else if (files[i] is GFF) { kWriter.Write(files[i] as GFF, ms); }
                //else if (files[i] is KEY) { kWriter.Write(files[i] as KEY, ms); }
                //else if (files[i] is RIM) { kWriter.Write(files[i] as RIM, ms); }
                //else if (files[i] is SSF) { kWriter.Write(files[i] as SSF, ms); }
                //else if (files[i] is TLK) { kWriter.Write(files[i] as TLK, ms); }
                //else { kWriter.Write(files[i] as MiscType, ms); }

                rf.File_Data = ms.ToArray();
                rf.DataSize = rf.File_Data.Count();
                rf.DataOffset = totalOffset;

                File_Table.Add(rf);

                totalOffset += rf.DataSize + 16;
            }

        }

        /// <summary>
        /// Reads Bioware "RIM" Files 
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        public RIM(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                //header
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));
                Unknown = br.ReadBytes(4);
                FileCount = br.ReadInt32();
                File_Table_Offset = br.ReadInt32();
                IsExtension = br.ReadBoolean();
                Reserved = br.ReadBytes(99);

                br.BaseStream.Seek(File_Table_Offset, SeekOrigin.Begin);
                //File Table
                for (int i = 0; i < FileCount; i++)
                {
                    RIM.rFile RF = new RIM.rFile();
                    RF.Label = new string(br.ReadChars(16)).TrimEnd('\0');
                    RF.TypeID = br.ReadInt32();
                    RF.Index = br.ReadInt32();
                    RF.DataOffset = br.ReadInt32();
                    RF.DataSize = br.ReadInt32();
                    File_Table.Add(RF);
                }

                //populate FileData
                foreach (RIM.rFile RF in File_Table)
                {
                    br.BaseStream.Seek(RF.DataOffset, SeekOrigin.Begin);
                    RF.File_Data = br.ReadBytes(RF.DataSize + 4); //Add an extra four bytes of padding into the null separater to eleminate size bound errors
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Writes Bioware 'RIM' data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        public override void Write(Stream s)
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
                foreach (RIM.rFile RF in File_Table)
                {
                    bw.Write(RF.Label.PadRight(16, '\0').ToArray());
                    bw.Write(RF.TypeID);
                    bw.Write(RF.Index);
                    bw.Write(RF.DataOffset);
                    bw.Write(RF.DataSize);
                }

                //File Data
                foreach (RIM.rFile RF in File_Table)
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

        /// <summary>
        /// Appends a new KFile to the end of the <see cref="RIM"/>
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

            MemoryStream ms = new MemoryStream();

            file.Write(ms);
            //if (file is TwoDA) { kWriter.Write(file as TwoDA, ms); }
            //else if (file is BIF) { kWriter.Write(file as BIF, ms); }
            //else if (file is ERF) { kWriter.Write(file as ERF, ms); }
            //else if (file is GFF) { kWriter.Write(file as GFF, ms); }
            //else if (file is KEY) { kWriter.Write(file as KEY, ms); }
            //else if (file is RIM) { kWriter.Write(file as RIM, ms); }
            //else if (file is SSF) { kWriter.Write(file as SSF, ms); }
            //else if (file is TLK) { kWriter.Write(file as TLK, ms); }
            //else { kWriter.Write(file as MiscType, ms); }

            rf.File_Data = ms.ToArray();
            rf.DataSize = rf.File_Data.Count();
            rf.DataOffset = File_Table.Last().DataOffset + File_Table.Last().DataSize + 16;

            File_Table.Add(rf);
        }

        /// <summary>
        /// Gets byte data from the RIM from the given index
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
        /// Gets byte data from the RIM with the given filename
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
        /// Returns a formatted <see cref="KFile"/> read form the resource at the given index.
        /// </summary>
        /// <param name="index">The index of the file to be returned</param>
        /// <returns></returns>
        public KFile GetKFile(int index)
        {
            using (MemoryStream ms = new MemoryStream(File_Table[index].File_Data))
            {
                if (File_Table[index].TypeID == 2017)
                {
                    return new TwoDA(ms);
                }
                else if (File_Table[index].TypeID == 9998)
                {
                    return new BIF(ms);
                }
                else if (Reference_Tables.ERFResTypes.Contains(File_Table[index].TypeID))
                {
                    return new ERF(ms);
                }
                else if (Reference_Tables.GFFResTypes.Contains(File_Table[index].TypeID))
                {
                    return new GFF(ms);
                }
                else if (File_Table[index].TypeID == 3002)
                {
                    return new RIM(ms);
                }
                else if (File_Table[index].TypeID == 2060)
                {
                    return new SSF(ms);
                }
                else if (File_Table[index].TypeID == 2018)
                {
                    return new TLK(ms);
                }
                else
                {
                    return new MiscType(ms);
                }
            }
        }

        #endregion

        #region Properties

        //FileType & Version in superclass
        ///<summary>4 bytes that appear to be null in every <see cref="RIM"/> I've come across so far.</summary>
        public byte[] Unknown;
        ///<summary>The number of files contained within the <see cref="RIM"/></summary>
        public int FileCount;
        ///<summary>Byte offset from start of the file to the <see cref="File_Table"/></summary>
        public int File_Table_Offset;
        ///<summary>Denotes a <see cref="RIM"/> that is an extension to another (marked by a filename ending in 'x')</summary>
        public bool IsExtension;

        ///<summary>99 empty bytes, probably reserved for backwards compatibility</summary>
        public byte[] Reserved;

        ///<summary>All of the <see cref="rFile"/>s contained within the <see cref="RIM"/>. This is the primary property of the <see cref="RIM"/></summary>
        public List<rFile> File_Table = new List<rFile>();

        #endregion
    }
}
