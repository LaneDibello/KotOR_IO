using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// BioWare Encapsulated Resource File Data. <para/>
    /// See: <see cref="ERF(Stream)"/>
    /// <seealso  cref="Write(Stream)"/>
    /// <remarks>
    /// <para/>ERF files often come in the form of Save games (.SAV), active modules (.MOD), texture packs (.ERF), as well as hack-pack (.HAK)
    /// <para/>They simply store data with Key references for quick reading and writing by the game.
    /// </remarks>
    /// </summary>
    public class ERF : KFile
    {
        /// <summary>
        /// Initiates a new instance of the <see cref="ERF"/> class from raw byte data.
        /// </summary>
        /// <param name="rawData">A byte array containing the file data.</param>
        public ERF(byte[] rawData)
            : this(new MemoryStream(rawData))
        { }

        /// <summary>
        /// Reads the given BioWare ERF File
        /// </summary>
        /// <param name="path">File path to read.</param>
        public ERF(string path)
            : this(File.OpenRead(path))
        { }

        /// <summary>
        /// Reads Bioware Encapsulated Resource Files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        protected ERF(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                //header
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));
                LanguageCount = br.ReadInt32();
                LocalizedStringSize = br.ReadInt32();
                EntryCount = br.ReadInt32();
                OffsetToLocalizedStrings = br.ReadInt32();
                OffsetToKeyList = br.ReadInt32();
                OffsetToResourceList = br.ReadInt32();
                BuildYear = br.ReadInt32();
                BuildDay = br.ReadInt32();
                DescriptionStrRef = br.ReadInt32();
                Reserved_block = br.ReadBytes(116);

                br.BaseStream.Seek(OffsetToLocalizedStrings, SeekOrigin.Begin); //seek to local strings
                for (int i = 0; i < LanguageCount; i++)
                {
                    String_List_Element SLE = new String_List_Element();
                    SLE.LanguageID = br.ReadInt32();
                    SLE.Language = Reference_Tables.Language_IDs[SLE.LanguageID];
                    SLE.StringSize = br.ReadInt32();
                    SLE.String = new string(br.ReadChars(SLE.StringSize));
                    Localized_String_List.Add(SLE);
                }

                br.BaseStream.Seek(OffsetToKeyList, SeekOrigin.Begin); //seek to key entries
                for (int i = 0; i < EntryCount; i++)
                {
                    Key key = new Key();
                    key.ResRef = new string(br.ReadChars(16)).TrimEnd('\0');
                    key.ResID = br.ReadInt32();
                    key.ResType = br.ReadInt16();
                    key.Type_string = Reference_Tables.Res_Types[key.ResType];
                    key.Unused = br.ReadInt16();
                    Key_List.Add(key);
                }

                br.BaseStream.Seek(OffsetToResourceList, SeekOrigin.Begin); //seek to resource list
                for (int i = 0; i < EntryCount; i++)
                {
                    Resource Res = new Resource();
                    Res.OffsetToResource = br.ReadInt32();
                    Res.ResourceSize = br.ReadInt32();
                    Resource_List.Add(Res);
                }

                foreach (Resource r in Resource_List) //populate resource_data for each resource in the list
                {
                    br.BaseStream.Seek(r.OffsetToResource, SeekOrigin.Begin); //seek to raw resource data
                    r.Resource_data = br.ReadBytes(r.ResourceSize);
                }
            }
        }

        // FileType & Version in superclass
        /// <summary> The number of strings in the Localized String List. </summary>
        public int LanguageCount { get; set; }

        /// <summary> The Total size (bytes) of Localized String List. </summary>
        public int LocalizedStringSize { get; set; }

        /// <summary> The number of files packed into the ERF. </summary>
        public int EntryCount { get; set; }

        /// <summary> The byte offset from the start of the file to the Localized String List. </summary>
        public int OffsetToLocalizedStrings { get; set; }

        /// <summary> The byte offset from the start of the file to the Key List. </summary>
        public int OffsetToKeyList { get; set; }

        /// <summary> The byte offset from the start of the file to the Resource List. </summary>
        public int OffsetToResourceList { get; set; }

        /// <summary> The number of years after 1900 that the ERF file was built. (i.e. 2019 == 119) </summary>
        public int BuildYear { get; set; }

        /// <summary> The number of days after January 1st the ERF file was built. (i.e. October 5th == 277) </summary>
        public int BuildDay { get; set; }

        /// <summary> A numerical string reference to a talk table (<see cref="TLK"/>) for the file description if one exist. </summary>
        public int DescriptionStrRef { get; set; }

        /// <summary> A block of 116 (usually null) bytes that are reserved for future backwards compatibility. </summary>
        public byte[] Reserved_block { get; set; } = new byte[116];

        /// <summary> List of localized strings. Used of descriptive content for the ERF file. (Not always present) </summary>
        public List<String_List_Element> Localized_String_List { get; set; } = new List<String_List_Element>();

        /// <summary> The List containing all of the Resource Reference Keys for the files in this ERF. (Used for populated Filenames and Types) </summary>
        public List<Key> Key_List { get; set; } = new List<Key>();

        /// <summary> The list containing all of the resources. This contians all of the data for the files within this ERF. </summary>
        public List<Resource> Resource_List { get; set; } = new List<Resource>();

        /// <summary>
        /// Gets the <see cref="byte"/> data for the specified resource, given the resource reference. (filename)
        /// </summary>
        /// <param name="filename">The string resource reference of the file.</param>
        /// <returns></returns>
        public byte[] this[string filename]
        {
            get
            {
                return Resource_List[(from k in Key_List where k.ResRef == filename select k.ResID).First()].Resource_data;
            }
        }

        /// <summary>
        /// Gets the <see cref="byte"/> data for the specified resource, given the index of the resource.
        /// </summary>
        /// <param name="index">The index of the resource in the resource array.</param>
        /// <returns></returns>
        public byte[] this[int index]
        {
            get
            {
                return Resource_List[index].Resource_data;
            }
        }

        /// <summary>
        /// Adds another resource to the <see cref="ERF"/>.
        /// </summary>
        /// <param name="res_ref">The name of the resource/file. *Maximum of 16 Characters</param>
        /// <param name="file_data">A byte array containing the data for the resource</param>
        public void Append_File(string res_ref, byte[] file_data)
        {
            //header
            EntryCount++;
            OffsetToResourceList += 24;

            //key
            Key k = new Key();
            k.ResRef = res_ref;
            k.ResID = Key_List.Last().ResID + 1;
            StringBuilder sb = new StringBuilder(4);
            sb.Append(new char[4] { (char)file_data[0], (char)file_data[1], (char)file_data[2], (char)file_data[3], });
            k.ResType = (short)Reference_Tables.TypeCodes[sb.ToString()];
            k.Type_string = k.Type_string = Reference_Tables.Res_Types[k.ResType];
            k.Unused = 0;
            Key_List.Add(k);

            //Offset Correction
            int TotalOffset = OffsetToResourceList + EntryCount * 8;
            foreach (Resource res in Resource_List)
            {
                res.OffsetToResource = TotalOffset;
                TotalOffset += res.ResourceSize;
            }

            //resource
            Resource r = new Resource();
            r.Resource_data = file_data;
            r.ResourceSize = file_data.Length;
            r.OffsetToResource = Resource_List.Last().OffsetToResource + Resource_List.Last().ResourceSize;
            Resource_List.Add(r);
        }

        /// <summary>
        /// Writes Bioware Encapsulated Resource File data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        internal override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                // Header
                bw.Write(FileType.ToArray());
                bw.Write(Version.ToArray());
                bw.Write(LanguageCount);
                bw.Write(LocalizedStringSize);
                bw.Write(EntryCount);
                bw.Write(OffsetToLocalizedStrings);
                bw.Write(OffsetToKeyList);
                bw.Write(OffsetToResourceList);
                bw.Write(BuildYear);
                bw.Write(BuildDay);
                bw.Write(DescriptionStrRef);
                bw.Write(Reserved_block);

                // Localized String List
                bw.Seek(OffsetToLocalizedStrings, SeekOrigin.Begin);
                foreach (String_List_Element SLE in Localized_String_List)
                {
                    bw.Write(SLE.LanguageID);
                    bw.Write(SLE.StringSize);
                    bw.Write(SLE.String.ToArray());
                }

                // Key List
                bw.Seek(OffsetToKeyList, SeekOrigin.Begin);
                foreach (Key EK in Key_List)
                {
                    bw.Write(EK.ResRef.PadRight(16, '\0').ToArray());
                    bw.Write(EK.ResID);
                    bw.Write(EK.ResType);
                    bw.Write(EK.Unused);
                }

                // Resource List
                bw.Seek(OffsetToResourceList, SeekOrigin.Begin);
                foreach (Resource ER in Resource_List)
                {
                    bw.Write(ER.OffsetToResource);
                    bw.Write(ER.ResourceSize);
                }

                // Resource Data
                foreach (Resource ER in Resource_List)
                {
                    bw.Seek(ER.OffsetToResource, SeekOrigin.Begin);
                    bw.Write(ER.Resource_data);
                }
            }
        }

        /// <summary>
        /// A localized string element in the Localized String list.
        /// </summary>
        public class String_List_Element
        {
            ///<summary>The ID that represents what language this entry is in.</summary>
            public int LanguageID;
            ///<summary>The language that LanguageID references according to <see cref="Reference_Tables.Language_IDs"/></summary>
            public string Language;
            ///<summary>The size of the string in chars/bytes</summary>
            public int StringSize;
            ///<summary>The localized string. Populated by a char array of size StringSize</summary>
            public string String;
        }

        /// <summary>
        /// The Key that contains the filename, index, and file type of a resource.
        /// </summary>
        public class Key
        {
            ///<summary>The Resource Reference (aka Filename). Max of 16 chars long.</summary>
            public string ResRef;
            ///<summary>The Resource ID. This is a 0-based index into the Resource List</summary>
            public int ResID;
            ///<summary>The File type ID. <para/>See: 
            ///<see cref="Reference_Tables.Res_Types"/></summary>
            public short ResType;
            ///<summary>The File Type extension populated from <see cref="Reference_Tables.Res_Types"/></summary>
            public string Type_string;
            ///<summary>An unused 16-bit integer (usually null) present in every ERF Key.</summary>
            public short Unused;
        }

        /// <summary>
        /// Contains the byte offset and size of each file in the ERF.
        /// </summary>
        public class Resource
        {
            ///<summary>The offset from begining of ERF file (usually into Resource data section) to the data for this resource.</summary>
            public int OffsetToResource;
            ///<summary>The size of the resource (file) in bytes</summary>
            public int ResourceSize;
            ///<summary>The raw byte data for the resource (file)</summary>
            public byte[] Resource_data;
        }
    }
}
