using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// BioWare Talk Table Data.<para/>
    /// See: <see cref="TLK(Stream)"/>, 
    /// <seealso  cref="Write(Stream)"/>
    /// <remarks>
    /// <para/>Talk Tables contain literally all of the text, spoken or written, for nearly the entire game. Their purpose is to make an easily swappable file (in KotOR's case 'dialog.tlk') to integrate different languages.
    /// <para/>In addition to string references, they also reference sound files.
    /// </remarks>
    /// </summary>
    public class TLK : KFile
    {
        /// <summary>
        /// Initiates a new instance of the <see cref="TLK"/> class from raw byte data.
        /// </summary>
        /// <param name="rawData">A byte array containing the file data.</param>
        public TLK(byte[] rawData)
            : this(new MemoryStream(rawData))
        { }

        /// <summary>
        /// Reads the given BioWare Talk Table File from a file path.
        /// </summary>
        /// <param name="path">File path to read.</param>
        public TLK(string path)
            : this(File.OpenRead(path))
        { }

        /// <summary>
        /// Reads Bioware Talk Table Files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        protected TLK(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                // Header
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));
                LanguageID = br.ReadInt32();
                StringCount = br.ReadInt32();
                StringEntriesOffset = br.ReadInt32();

                // String Data Table
                for (int i = 0; i < StringCount; i++)
                {
                    String_Data SD = new String_Data();
                    SD.Flags = br.ReadInt32();
                    int tempFlags = SD.Flags;
                    if (tempFlags >= 4) { SD.SNDLENGTH_PRESENT = true; tempFlags -= 4; }
                    if (tempFlags >= 2) { SD.SND_PRESENT = true; tempFlags -= 2; }
                    if (tempFlags >= 1) { SD.TEXT_PRESENT = true; tempFlags--; }
                    SD.SoundResRef = new string(br.ReadChars(16)).TrimEnd('\0');
                    SD.VolumeVariance = br.ReadInt32();
                    SD.PitchVariance = br.ReadInt32();
                    SD.OffsetToString = br.ReadInt32();
                    SD.StringSize = br.ReadInt32();
                    SD.SoundLength = br.ReadSingle();

                    String_Data_Table.Add(SD);
                }

                // Populating String text from string entires
                foreach (String_Data SD in String_Data_Table)
                {
                    br.BaseStream.Seek(StringEntriesOffset + SD.OffsetToString, SeekOrigin.Begin);
                    SD.StringText = new string(br.ReadChars(SD.StringSize));
                }
            }
        }

        // FileType & Version in superclass

        /// <summary> The numerical ID for the Language that the string entries in this Talk Table will be in.
        /// <para/>See: <see cref="Reference_Tables.Language_IDs"/>. </summary>
        public int LanguageID { get; set; }

        /// <summary> The number of strings in this Talk Table. </summary>
        public int StringCount { get; set; }

        /// <summary> The byte offset from the start of the file to the String Entries. </summary>
        public int StringEntriesOffset { get; set; }

        /// <summary> The tabel containing all of the <see cref="String_Data"/> elements. This is the primary property of the <see cref="TLK"/>. </summary>
        public List<String_Data> String_Data_Table { get; set; } = new List<String_Data>();

        /// <summary>
        /// Gets a particular string from a string reference
        /// </summary>
        /// <param name="str_ref">The reference number (index) for this particular string.</param>
        /// <returns></returns>
        public string this[int str_ref]
        {
            get
            {
                return String_Data_Table[str_ref].StringText;
            }
            set
            {
                int delta_offset = value.Length - String_Data_Table[str_ref].StringSize;
                String_Data_Table[str_ref].StringSize = value.Length;
                String_Data_Table[str_ref].StringText = value;
                for (int i = str_ref + 1; i < StringCount; i++)
                {
                    String_Data_Table[i].OffsetToString += delta_offset;
                }
            }
        }

        /// <summary>
        /// Writes Bioware Talk Table data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        internal override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                // Header
                bw.Write(FileType.ToArray());
                bw.Write(Version.ToArray());
                bw.Write(LanguageID);
                bw.Write(StringCount);
                bw.Write(StringEntriesOffset);

                // String data table
                foreach (String_Data SD in String_Data_Table)
                {
                    bw.Write(SD.Flags);
                    bw.Write(SD.SoundResRef.PadRight(16, '\0').ToArray());
                    bw.Write(SD.VolumeVariance);
                    bw.Write(SD.PitchVariance);
                    bw.Write(SD.OffsetToString);
                    bw.Write(SD.StringSize);
                    bw.Write(SD.SoundLength);
                }

                // String text
                foreach (String_Data SD in String_Data_Table)
                {
                    bw.Seek(StringEntriesOffset + SD.OffsetToString, SeekOrigin.Begin);

                    bw.Write(SD.StringText.ToArray());
                }
            }
        }

        // String Data Table
        /// <summary>
        /// An element on the <see cref="String_Data_Table"/>.
        /// </summary>
        public class String_Data
        {
            /// <summary>
            /// Initiates a new instance of the <see cref="String_Data"/> class.
            /// </summary>
            public String_Data() { }

            /// <summary>
            /// Initiates a new instance of the <see cref="String_Data"/> class from text and sound information. <para/> NOTE: This feature is experimental and may not work properly.
            /// </summary>
            /// <param name="StringText">The Text contained in the string</param>
            /// <param name="SoundResRef">The 16 character reference for the sound file, if none exist set value to ""</param>
            /// <param name="SoundLength">The duration in seconds of the sound, if none sxist set to zero</param>
            public String_Data(string StringText, string SoundResRef, float SoundLength)
            {
                //**NOTE: all Caps flags don't appear to be used in Kotor, but be mindful of their implementation in other games.
                Flags = 7;
                this.SoundResRef = SoundResRef;
                VolumeVariance = 0;
                PitchVariance = 0;
                //OffsetToString should be calculated elsewhere.
                StringSize = StringText.Length;
                this.SoundLength = SoundLength;
                TEXT_PRESENT = true;
                SND_PRESENT = true;
                SNDLENGTH_PRESENT = true;
                this.StringText = StringText;
            }

            /// <summary> Bitwise Flags about this string reference. See: <see cref="TEXT_PRESENT"/>, <see cref="SND_PRESENT"/>, and <see cref="SNDLENGTH_PRESENT"/>. </summary>
            public int Flags;
            /// <summary> 16 character resource reference for the wave file associated with this sound. </summary>
            public string SoundResRef; //char[16]
            /// <summary> Marked as not used by the documentation, but presumably adjust volume. </summary>
            public int VolumeVariance;
            /// <summary> Marked as not used by the documentation, but presumably adjust pitch. </summary>
            public int PitchVariance;
            /// <summary> Byte offset from <see cref="StringEntriesOffset"/> to the string's text. </summary>
            public int OffsetToString;
            /// <summary> The size of the string in <see cref="char"/>s. </summary>
            public int StringSize;
            /// <summary> The duration of the sound in seconds. </summary>
            public float SoundLength;

            /// <summary> Whether or not text exist for the string. *Note: these flags don't see much use in KotOR. </summary>
            public bool TEXT_PRESENT = false;
            /// <summary> Whether or not a Sound exist for the string. *Note: these flags don't see much use in KotOR. </summary>
            public bool SND_PRESENT = false;
            /// <summary> Whether or not a sound length is present for this string. *Note: these flags don't see much use in KotOR. </summary>
            public bool SNDLENGTH_PRESENT = false;

            /// <summary> The text associated with the string. </summary>
            public string StringText;
        }
    }
}
