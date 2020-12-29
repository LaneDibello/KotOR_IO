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
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));
                Unknown = br.ReadBytes(4);
                int FileCount = br.ReadInt32();
                int File_Table_Offset = br.ReadInt32();
                IsExtension = br.ReadBoolean();
                Reserved = br.ReadBytes(99);

                List<unordered_rFile> unordered_File_Table = new List<unordered_rFile>();

                br.BaseStream.Seek(File_Table_Offset, 0);
                //File Table
                for (int i = 0; i < FileCount; i++)
                {
                    unordered_rFile URF = new unordered_rFile();
                    rFile RF = new rFile();
                    RF.Label = new string(br.ReadChars(16)).TrimEnd('\0');
                    RF.TypeID = br.ReadInt32();
                    URF.Index = br.ReadInt32();
                    URF.DataOffset = br.ReadInt32();
                    URF.DataSize = br.ReadInt32();
                    URF.rf = RF;

                    unordered_File_Table.Add(URF);
                }

                //populate FileData and File_Table
                foreach (unordered_rFile URF in unordered_File_Table.OrderBy(x => x.Index)) //Deals with rare non-linear index case
                {
                    br.BaseStream.Seek(URF.DataOffset, SeekOrigin.Begin);
                    URF.rf.File_Data = br.ReadBytes(URF.DataSize);

                    File_Table.Add(URF.rf);
                }
            }
        }

        // FileType & Version in superclass

        /// <summary>
        /// 4 bytes that appear to be null in every <see cref="RIM"/> I've come across so far.
        /// </summary>
        public byte[] Unknown;

        /// <summary>
        /// Denotes a <see cref="RIM"/> that is an extension to another (marked by a filename ending in 'x').
        /// </summary>
        public bool IsExtension;

        /// <summary>
        /// 99 empty bytes, probably reserved for backwards compatibility.
        /// </summary>
        public byte[] Reserved;

        /// <summary>
        /// All of the <see cref="rFile"/>s contained within the <see cref="RIM"/> at their respective Index. This is the primary property of the <see cref="RIM"/>.
        /// </summary>
        public List<rFile> File_Table = new List<rFile>();

        /// <summary>
        /// Appends a new KFile to the end of the <see cref="RIM"/>.
        /// </summary>
        /// <param name="file">The <see cref="KFile"/> to be added.</param>
        /// <param name="filename">The name of the file.</param>
        public void Append_File(KFile file, string filename)
        {
            rFile RF = new rFile();
            RF.Label = filename;
            RF.TypeID = Reference_Tables.TypeCodes[file.FileType];

            using (MemoryStream ms = new MemoryStream())
            {
                file.Write(ms);
                RF.File_Data = ms.ToArray();
            }
            File_Table.Add(RF);
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
                        return new GFF_old(file.File_Data);
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
                bw.Write(File_Table.Count());
                bw.Write(120); //Should always be 120
                bw.Write(IsExtension);

                bw.Write(Reserved);

                int file_indexer = 0; //Tracks the current file index
                int file_data_counter = 120 + File_Table.Count * 32 + 8; //Tracks the byte offset into file data, Starts After the file table, and 8 bytes of null padding

                //File Table
                bw.Seek(120, SeekOrigin.Begin);
                foreach (rFile RF in File_Table)
                {
                    bw.Write(RF.Label.PadRight(16, '\0').ToArray());
                    bw.Write(RF.TypeID);
                    bw.Write(file_indexer);
                    file_indexer++;
                    bw.Write(file_data_counter);
                    bw.Write(RF.File_Data.Count());
                    file_data_counter += RF.File_Data.Count() + 16;
                }

                //File Data
                byte[] f_data_padding = new byte[8] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0};
                bw.BaseStream.Seek(120 + File_Table.Count * 32, 0);
                foreach (rFile RF in File_Table)
                {
                    bw.Write(f_data_padding);
                    bw.Write(RF.File_Data);
                    bw.Write(f_data_padding); //Each file is padded with 16 nulls, 8 before, 8 after, for unclear reasons.
                }

                //Padding the end with 6 bytes because kotor likes that for some reason.
                bw.Seek(0, SeekOrigin.End);
                byte[] padend = new byte[6] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
                bw.Write(padend);
            }
        }
        
        internal struct unordered_rFile
        {
            internal rFile rf;
            internal int Index;
            internal int DataOffset;
            internal int DataSize;
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
            ///<summary> The data contained within this file. </summary>
            public byte[] File_Data; // Populated from the FileData block.

            /// <summary>
            /// Writes human readable summary of the rFile.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return Enum.IsDefined(typeof(ResourceType), TypeID) ?
                    $"{((ResourceType)TypeID).ToDescription()}({TypeID}), {Label}" :
                    $"??? ({TypeID}), {Label}";
            }
        }
    }
}
