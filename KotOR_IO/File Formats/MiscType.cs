using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO.File_Formats
{
    /// <summary>
    /// Represents other Kotor source files that aren't yet implemented. Contains only raw byte data.
    /// <remarks>
    /// <para/>This is usually used for kotor source files such as Waves, textures, models, and other formats that don't have easily interpretable data.
    /// </remarks>
    /// </summary>
    public class MiscType : KFile
    {
        #region Properties

        ///<summary>The raw byte data for this source file.</summary>
        public byte[] Data { get; protected set; }

        #endregion

        #region Constructors

        ///<summary>Initiates a new instance of the <see cref="MiscType"/> class.</summary>
        public MiscType() { }

        /// <summary>
        /// Initiates a new instance of the <see cref="MiscType"/> class from byte data
        /// </summary>
        /// <param name="data">A byte array containing the file data.</param>
        public MiscType(byte[] data)
            : this(new MemoryStream(data))
        { }

        /// <summary>
        /// Reads miscellaneous Kotor source Files into a <see cref="MiscType"/> class.
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        /// <returns></returns>
        public MiscType(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                //header
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));

                //Data
                br.BaseStream.Seek(0, SeekOrigin.Begin);
                Data = br.ReadBytes((int)br.BaseStream.Length);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Writes a miscellaneous KotOR source file data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        public override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                //Data
                bw.Write(Data);
            }
        }

        #endregion
    }
}
