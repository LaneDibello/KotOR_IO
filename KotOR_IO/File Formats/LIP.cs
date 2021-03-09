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
        #region Methods

        //constructors
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
                    LipEntry le = new LipEntry(br.ReadSingle(), (LipState)br.ReadByte());
                    Entries.Add(le);
                }
            }
        }

        //overrides
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

        #endregion

        #region Properties

        /// <summary>
        /// The total length of the the sound file these lips are synced to
        /// </summary>
        public float SoundLength { get; set; }

        /// <summary>
        /// An Entry in the lip table, matching a mouth shape to a timestamp
        /// </summary>
        public struct LipEntry
        {
            /// <summary>
            /// The timestamp in seconds from the start of the soundfile
            /// </summary>
            public float TimeStamp { get; set; }
            /// <summary>
            /// The current mouth shape
            /// </summary>
            public LipState State { get; set; }

            /// <summary>
            /// Constructs a new instance of the LipEntry Struct
            /// </summary>
            /// <param name="time_stamp"></param>
            /// <param name="lip"></param>
            public LipEntry(float time_stamp, LipState lip)
            {
                TimeStamp = time_stamp;
                State = lip;
            }
        }

        /// <summary>
        /// The Lip Entries in this file. The first value is a time stamp in seconds from the start of the sound file.
        /// The second is the mouth animation from the <see cref="LipState"/> enum.
        /// </summary>
        public List<LipEntry> Entries { get; set; } = new List<LipEntry>(); //migrate to a protected set once we work out entry editing logic

        #endregion

        /// <summary>
        /// The current shape of the speaker's mouth. (single byte)
        /// </summary>
        public enum LipState : byte
        {
            /// <summary> As in "teeth" or "speed" </summary>
            ee = 0x0,
            /// <summary> As in "bet" or "red" </summary>
            eh = 0x1,
            /// <summary> Like 'a' in "sofa" </summary>
            a = 0x2,
            /// <summary> As in "cat" or "bad "</summary>
            ah = 0x3,
            /// <summary> As in "boat" or "toad" </summary>
            oh = 0x4,
            /// <summary> As in "Blue", also the 'wh' shape form "wheel"</summary>
            oo = 0x5,
            /// <summary> As in "yes" or "yankee"</summary>
            y = 0x6,
            /// <summary> As in "sick", also 'ts' from "nets" </summary>
            s = 0x7,
            /// <summary> As in "fish", also 'v' from "very"</summary>
            f = 0x8,
            /// <summary> As in "nacho", also 'ng' from "running" </summary>
            n = 0x9, 
            /// <summary> As in "think" or "that"</summary>
            th = 0xA,
            /// <summary> As in "moose", also 'p' from "pop" or 'b' from "book" </summary>
            m = 0xB,
            /// <summary> As in "table", also 'd' from "door" </summary>
            t = 0xC,
            /// <summary> As in "jury", also 'sh' from "shape"</summary>
            j = 0xD,
            /// <summary> As in "lick", also 'r' in "run"</summary>
            l = 0xE,
            /// <summary> As in "cake", also 'g' from "go" </summary>
            k = 0xF
        }

        
    }
}
