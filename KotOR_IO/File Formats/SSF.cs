using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// BioWare Soundset Data (v1.1).<para/>
    /// See: <see cref="SSF(Stream)"/>, 
    /// <seealso  cref="Write(Stream)"/>
    /// <remarks>
    /// <para/>Soundsets contain numerical references to the various sound effects different creatures make in the game.
    /// <para/>*NOTE: This program is currently only compatible with SSF version 1.1 files
    /// </remarks>
    /// </summary>
    public class SSF : KFile
    {
        /// <summary>
        /// Initiates a new instance of the <see cref="SSF"/> class from raw byte data.
        /// </summary>
        /// <param name="rawData">A byte array containing the file data.</param>
        public SSF(byte[] rawData)
            : this(new MemoryStream(rawData))
        { }

        /// <summary>
        /// Reads the given BioWare SSF File
        /// </summary>
        /// <param name="path">File path to read.</param>
        public SSF(string path)
            : this(File.OpenRead(path))
        { }

        /// <summary>
        /// Reads Bioware Sound Set Format (v1.1) Files.
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        protected SSF(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                // Header
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));
                UnknownInt = br.ReadInt32();

                // String refs
                for (int k = 0; k < 28; k++)
                {
                    StringTable.Add(Reference_Tables.SSFields[k], br.ReadInt32());
                }

                EndPadding = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));

                TLKPopulated = false;
            }
        }

        /// <summary> A 32-bit int that I'm not entirely sure the purpose of, but is present in all <see cref="SSF"/>s. </summary>
        public int UnknownInt { get; protected set; }

        /// <summary> A table containing all the sounds in the Sound Set. The <see cref="object"/> value will be an <see cref="int"/> index into a talk table if <see cref="TLKPopulated"/> is false, and <see cref="Sound"/> if true. </summary>
        public Dictionary<string, object> StringTable { get; protected set; }

        /// <summary> A set 0xF bytes that pad until the end of the <see cref="SSF"/>, representing unused sounds. </summary>
        public byte[] EndPadding { get; set; }

        /// <summary> Whether the <see cref="SSF"/> has been populated by a <see cref="TLK"/> (usually dialog.tlk). If True then the string references have been converted into <see cref="Sound"/>s. </summary>
        public bool TLKPopulated { get; set; }

        /// <summary>
        /// Gets and Sets SoundSet values. The type will be <see cref="int"/> if <see cref="TLKPopulated"/> is false, and <see cref="Sound"/> if <see cref="TLKPopulated"/> is true.
        /// </summary>
        /// <param name="ssField">The string representation of the soundeffect from <see cref="Reference_Tables.SSFields"/></param>
        /// <returns></returns>
        public object this[string ssField]  // value from reference table
        {
            get
            {
                return StringTable[ssField];
            }
            set
            {
                StringTable[ssField] = value;
            }
        }

        /// <summary>
        /// Populates the <see cref="StringTable"/> objects with <see cref="Sound"/>s based on the <see cref="int"/> string references.
        /// </summary>
        /// <param name="t">The talk table used to populate the Soundset. (usually dialog.tlk)</param>
        public void PopulateTLK(TLK t)
        {
            if (!TLKPopulated)
            {
                int x = 0;
                object[] soundObjects = new object[28];
                StringTable.Values.CopyTo(soundObjects, 0);
                foreach (object o in soundObjects)
                {
                    Sound s = new Sound();
                    s.SRef = (int)o;
                    if ((int)o != -1)
                    {
                        s.SoundText = t.String_Data_Table[(int)o].StringText;
                        s.SoundFile = t.String_Data_Table[(int)o].SoundResRef;
                    }
                    else
                    {
                        s.SoundText = "No Sound";
                        s.SoundFile = "No Sound";
                    }
                    StringTable[Reference_Tables.SSFields[x]] = s;
                    x++;
                }
            }
            TLKPopulated = true;
        }

        /// <summary>
        /// Writes Bioware Soundset file (v1.1) data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        internal override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                //Header
                bw.Write(FileType.ToArray());
                bw.Write(Version.ToArray());
                bw.Write(UnknownInt);

                //string refs
                if (TLKPopulated)
                {
                    foreach (object o in StringTable.Values)
                    {
                        bw.Write((o as Sound).SRef);
                    }
                }
                else
                {
                    foreach (object o in StringTable.Values)
                    {
                        bw.Write((int)o);
                    }
                }

                //EndPadding
                bw.Write(EndPadding);
            }
        }

        /// <summary>
        /// Represents a sound in the <see cref="SSF"/>. This class is used when <see cref="TLKPopulated"/> is true.
        /// </summary>
        public class Sound
        {
            /// <summary> An <see cref="int"/> index into a <see cref="TLK"/> representing a particular sound and text. </summary>
            public int SRef { get; set; }
            /// <summary> The Filename of the audio file linked to this sound. </summary>
            public string SoundFile { get; set; }
            /// <summary> The text representation of this sound. </summary>
            public string SoundText { get; set; }
        }
    }
}
