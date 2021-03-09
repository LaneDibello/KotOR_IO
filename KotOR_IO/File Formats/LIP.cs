using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KotOR_IO
{
    /// <summary>
    /// BioWare 'LIP' data. <para/>
    /// Tracks Lip movements of speaking NPCs
    /// </summary>
    public class LIP : KFile
    {
        /// <summary>
        /// Initiates a new instance of the <see cref="LIP"/> class from raw byte data.
        /// </summary>
        /// <param name="rawData"></param>
        public LIP(byte[] rawData)
            : this(new MemoryStream(rawData))
        { }

        /// <summary>
        /// Reads the given BioWare LIP File.
        /// </summary>
        /// <param name="path">File path to read.</param>
        public LIP(string path)
            : this(File.OpenRead(path))
        { }

        protected LIP(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                //Header
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));

                //Entry Info
                SoundLength = br.ReadSingle();
                int EntryCount = br.ReadInt32();

                for (int i = 0; i < EntryCount; i++)
                {
                    LipEntry le;
                    le.TimeStamp = br.ReadSingle();
                    le.State = (LipState)br.ReadByte();
                    Entries.Add(le);
                }
            }
        }

        /// <summary>
        /// The total length of teh the sound file these lips are synced to
        /// </summary>
        public float SoundLength;

        /// <summary>
        /// An Entry in the lip table, matching a mouth shape to a timestamp
        /// </summary>
        public struct LipEntry
        {
            /// <summary>
            /// The timestamp in seconds from the start of the soundfile
            /// </summary>
            public float TimeStamp;
            /// <summary>
            /// The current mouth shape
            /// </summary>
            public LipState State;
        }

        /// <summary>
        /// The Lip Entries in this file. The first value is a time stamp in seconds from the start of the sound file.
        /// The second is the mouth animation from the <see cref="LipState"/> enum.
        /// </summary>
        public List<LipEntry> Entries = new List<LipEntry>();

        /// <summary>
        /// The current shape of the speaker's mouth. (single byte)
        /// </summary>
        public enum LipState : byte
        {
            ee = 0x0, // "teeth"
            eh = 0x1, // "bet"
            a  = 0x2, // "sofa"
            ah = 0x3, // "cat"
            oh = 0x4, // "boat"
            oo = 0x5, // "too"
            y  = 0x6, // "yes"
            s  = 0x7, // "sick"
            f  = 0x8, // "fish"
            n  = 0x9, // "nacho"
            th = 0xA, // "think"
            m  = 0xB, // "moose"
            t  = 0xC, // "table"
            j  = 0xD, // "jungle"
            l  = 0xE, // "lick"
            k  = 0xF  // "catch"
        }




        internal override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                bw.Write(FileType.ToCharArray());
                bw.Write(Version.ToCharArray());

                bw.Write(SoundLength);
                bw.Write(Entries.Count);

                foreach (LipEntry LE in Entries)
                {
                    bw.Write(LE.TimeStamp);
                    bw.Write((byte)LE.State);
                }
            }
        }
    }
}
