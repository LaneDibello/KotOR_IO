using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// Generic Form of a KotOR source file.
    /// <para/>Serves as a super class to:
    /// <see cref="TwoDA"/>, 
    /// <see cref="BIF"/>, 
    /// <see cref="ERF"/>, 
    /// <see cref="GFF_old"/>, 
    /// <see cref="KEY"/>, 
    /// <see cref="LTR"/>, 
    /// <see cref="RIM"/>, 
    /// <see cref="SSF"/>, 
    /// <see cref="TLK"/>, and
    /// <see cref="MiscType"/>
    /// </summary>
    public abstract class KFile
    {
        /// <summary> ASCII value of NUL character. </summary>
        protected const byte ASCII_NULL = 0;

        /// <summary> ASCII value of HT (horizontal tab) character. </summary>
        protected const byte ASCII_TAB = 9;

        /// <summary> ASCII value of LF (newline) character. </summary>
        protected const byte ASCII_NEWLINE = 10;

        ///// <summary></summary>
        //public string Name { get; protected set; }

        /// <summary> The 4 char file type. </summary>
        public string FileType { get; protected set; }

        /// <summary> The 4 char file version. </summary>
        public string Version { get; protected set; }

        /// <summary>
        /// Returns the raw byte data for this source file.
        /// </summary>
        /// <returns>Raw byte data for this source file</returns>
        public virtual byte[] ToRawData()
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Write(ms);
                return ms.ToArray(); // Stream is closed, but the array is still available.
            }
        }

        /// <summary>
        /// Writes BioWare File data.
        /// </summary>
        /// <param name="s">The Stream to which the File will be written.</param>
        internal abstract void Write(System.IO.Stream s);

        /// <summary>
        /// Writes the byte data for this file to the given path.
        /// </summary>
        /// <param name="path">Path to the file to write.</param>
        public virtual void WriteToFile(string path)
        {
            Write(System.IO.File.OpenWrite(path));
        }
    }
}
