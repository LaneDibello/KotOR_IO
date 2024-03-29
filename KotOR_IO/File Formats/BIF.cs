﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// BioWare Built In File Data. <para/>
    /// See: <see cref="BIF(Stream)"/>, 
    /// <seealso  cref="Write(Stream)"/>, 
    /// <seealso  cref="AttachKey(KEY, string)"/>
    /// <remarks>
    /// <para/>BIF data stores the bulk of the template files for the game. Items, characters, models, sound data, scripts and many other things that will appear in multiple different location in game find there place in BIFs. 
    /// <para/>BIF data, though, is rather useless without a Key file to reference all of the data stored within. Which is the purpose of the AttachKey Method.
    /// </remarks>
    /// </summary>
    public class BIF : KFile
    {
        /// <summary>
        /// Initiates a new instance of the <see cref="BIF"/> class from raw byte data.
        /// </summary>
        /// <param name="rawData">A byte array containing the file data.</param>
        public BIF(byte[] rawData)
            : this(new MemoryStream(rawData))
        { }

        /// <summary>
        /// Reads the given BioWare BIF File
        /// </summary>
        /// <param name="path">File path to read.</param>
        public BIF(string path)
            : this(File.OpenRead(path))
        { }

        /// <summary>
        /// Reads Bioware Built In Files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        protected BIF(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                // Get header info
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));
                VariableResourceCount = br.ReadInt32();
                FixedResourceCount = br.ReadInt32();
                VariableTableOffset = br.ReadInt32();

                // Get Variable Resource Table
                for (int i = 0; i < VariableResourceCount; i++)
                {
                    VariableResourceEntry VRE = new VariableResourceEntry(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                    VariableResourceTable.Add(VRE);
                }

                // Get Fixed Resource Table
                for (int i = 0; i < FixedResourceCount; i++)
                {
                    FixedResourceEntry FRE = new FixedResourceEntry(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                    FixedResourceTable.Add(FRE);
                }

                // Populate Variable Resource Data
                foreach (VariableResourceEntry VRE in VariableResourceTable)
                {
                    VRE.EntryData = br.ReadBytes(VRE.FileSize);
                }

                // Populate Fixed Resource Data
                foreach (FixedResourceEntry FRE in FixedResourceTable)
                {
                    FRE.EntryData = br.ReadBytes(FRE.FileSize);
                }
            }
        }

        // FileType & Version in superclass.

        ///<summary>The name of the of the BIF file in the form of a path from the key directory. Used exclusively for Key attachment.</summary>
        public string Name { get; set; }

        ///<summary>The Number of Variable resources contained within the BIF</summary>
        public int VariableResourceCount { get; set; }

        ///<summary>The Number of Fixed Resources in the BIF *NOTE: This is not used in KotOR, though may be present in other Aurora Games</summary>
        public int FixedResourceCount { get; set; }

        ///<summary>The Offset to the variable resource table as bytes from the beginning of teh file. (This value is usually 20)</summary>
        public int VariableTableOffset { get; set; }

        ///<summary>The Table Containing all of the Variable Resource Data for this BIF. This is the main object of importance in a BIF, containing the actual functional content of the file.</summary>
        public List<VariableResourceEntry> VariableResourceTable { get; set; } = new List<VariableResourceEntry>();

        ///<summary>The Table Containing all of the Fixed Variable Resources. *NOTE: Fixed Resources are not used by KotOR</summary>
        public List<FixedResourceEntry> FixedResourceTable { get; set; } = new List<FixedResourceEntry>();

        /// <summary>
        /// Takes data from a given KEY class and uses it to populate the data in the BIF file. 
        /// </summary>
        /// <param name="k">The KEY to be attached. (Usually chitin.key for KotOR)</param>
        /// <param name="Filename">
        /// The filename for this BIF file which will be used to index its ID from the KEY. 
        /// <para>This is given in the form of a path from the KEY's directory.</para>
        /// <para>For Example: if the BIF file 'file' is located in a directory called 'data' (like KotOR BIFs), then the Filename would be "data\\file.bif"</para>
        /// </param>
        public void AttachKey(KEY k, string Filename)
        {
            Name = Filename;

            // Get index the BIF file
            int xIndex = 0;

            foreach (KEY.FileEntry FE in k.FileTable)
            {
                if (FE.Filename.Trim('\0') == Filename)
                {
                    break;
                }
                else
                {
                    xIndex++;
                }
            }

            // parse the list down to just the section the bif cares about
            var BifSectionX = k.KeyTable.Where(ke => ke.IDx == xIndex);

            // Compare each VariableResourceEntry to each Key_entry to match up IDy and assign its ResRef
            foreach (VariableResourceEntry VRE in VariableResourceTable)
            {
                var BifSectionY = BifSectionX.Where(ke => ke.IDy == VRE.IDy);
                if (BifSectionY.Any())
                {
                    VRE.ResRef = BifSectionY.Last().ResRef;
                }
            }
        }

        /// <summary>
        /// Writes Bioware Built In File data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        internal override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                // Header
                bw.Write(FileType.ToArray());
                bw.Write(Version.ToArray());
                bw.Write(VariableResourceCount);
                bw.Write(FixedResourceCount);
                bw.Write(VariableTableOffset);

                // Variable Resource Tabale
                bw.Seek(VariableTableOffset, SeekOrigin.Begin);
                foreach (VariableResourceEntry VRE in VariableResourceTable)
                {
                    bw.Write(VRE.ID);
                    bw.Write(VRE.Offset);
                    bw.Write(VRE.FileSize);
                    bw.Write((int)VRE.ResourceType);
                }

                // Fixed Resource Table *NOT USED*
                foreach (FixedResourceEntry FRE in FixedResourceTable)
                {
                    bw.Write(FRE.ID);
                    bw.Write(FRE.Offset);
                    bw.Write(FRE.PartCount);
                    bw.Write(FRE.FileSize);
                    bw.Write((int)FRE.ResourceType);
                }

                // Variable resource Data
                foreach (VariableResourceEntry VRE in VariableResourceTable)
                {
                    bw.Write(VRE.EntryData);
                }

                // Fixed Resource Data
                foreach (FixedResourceEntry FRE in FixedResourceTable)
                {
                    bw.Write(FRE.EntryData);
                }
            }
        }

        // Variable Resource Table
        /// <summary>
        /// One of the elements that makes up the Variable Resource Table. Contains basic meta data for each resource a resource in the BIF.
        /// </summary>
        public class VariableResourceEntry
        {
            /// <summary>
            /// Creates instance of VariableResourceEntry
            /// </summary>
            /// <param name="id">ID, which is separated into IDx and IDy</param>
            /// <param name="offset">Offset</param>
            /// <param name="filesize">Size of the entry data</param>
            /// <param name="type">Int representation of the ResourceType</param>
            public VariableResourceEntry(int id, int offset, int filesize, int type)
            {
                ID = id;
                IDx = ID >> 20;
                IDy = ID - (IDx << 20);
                Offset = offset;
                EntryData = new byte[FileSize];
                FileSize = filesize;
                ResourceType = (ResourceType)type;
            }

            /// <summary>
            /// <para>A unique ID number for this resource.</para>
            /// <para>It Contains two parts (x and y) which denote Resource index, as well as combatibility with Key files.</para>
            ///  </summary>
            public int ID { get; set; }

            /// <summary>Byte offset from the start of teh BIF to the data for this resource.</summary>
            public int Offset { get; set; }

            /// <summary>The size in bytes of this resource</summary>
            public int FileSize { get; set; }

            /// <summary> Enum representation of the file type of this resource. </summary>
            public ResourceType ResourceType { get; set; }

            /// <summary>
            /// The X component of the ID. This is rather useless in BIF files, however is included for consitancy with Key files.
            /// <para>It is calculated by bit shifting ID right by 20. (ID &gt;&gt; 20)</para>
            /// </summary>
            public int IDx { get; set; }

            /// <summary>
            /// The Y component of the ID. This denotes an index of this resource entry in the BIF. This value will match the repective ID in this BIF's Key file.
            /// <para>This is calculated by subtracting the X component bit shifted left 20 from the ID. (ID - (x &lt;&lt; 20))</para>
            /// </summary>
            public int IDy { get; set; }

            /// <summary>The raw byte data of this resource.</summary>
            public byte[] EntryData { get; set; }

            /// <summary>Resource Reference string (aka filename) of this resource. This is populated when Attaching Key data. </summary>
            public string ResRef { get; set; } = null;

            /// <summary>
            /// Writes human readable summary of the VariableResourceEntry.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return Enum.IsDefined(typeof(ResourceType), ResourceType) ?
                    $"{ResourceType.ToDescription()}({(int)ResourceType}), {ResRef ?? "null"}" :
                    $"??? ({ResourceType}), {ResRef ?? "null"}";
            }
        }

        // Fixed Resource Table
        /// <summary>
        /// Fixed Variable Resource. *NOTE: Fixed Resources are not used by KotOR, therefore no further documentation will be provided.
        /// </summary>
        public class FixedResourceEntry
        {
            /// <summary>
            /// Creates instance of FixedResourceEntry
            /// </summary>
            /// <param name="id"></param>
            /// <param name="offset"></param>
            /// <param name="partCount"></param>
            /// <param name="filesize"></param>
            /// <param name="type"></param>
            public FixedResourceEntry(int id, int offset, int partCount, int filesize, int type)
            {
                ID = id;
                Offset = offset;
                PartCount = partCount;
                FileSize = filesize;
                EntryData = new byte[FileSize];
                ResourceType = (ResourceType)type;
            }

            /// <summary></summary>
            public int ID { get; set; }
            /// <summary></summary>
            public int Offset { get; set; }
            /// <summary></summary>
            public int PartCount { get; set; }
            /// <summary></summary>
            public int FileSize { get; set; }
            /// <summary></summary>
            public ResourceType ResourceType { get; set; }

            /// <summary></summary>
            public int IDx { get; set; }
            /// <summary></summary>
            public int IDy { get; set; }

            /// <summary></summary>
            public byte[] EntryData { get; set; }
        }
    }
}
