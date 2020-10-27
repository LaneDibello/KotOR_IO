using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO.File_Formats
{
    /// <summary>
    /// Generic Form of a KotOR source file.
    /// <para/>Serves as a super class to:
    /// <see cref="TwoDA"/>, 
    /// <see cref="BIF"/>, 
    /// <see cref="ERF"/>, 
    /// <see cref="GFF"/>, 
    /// <see cref="KEY"/>, 
    /// <see cref="LTR"/>, 
    /// <see cref="RIM"/>, 
    /// <see cref="SSF"/>, 
    /// <see cref="TLK"/>, and
    /// <see cref="MiscType"/>
    /// </summary>
    public abstract class KFile
    {
        /// <summary>The 4 char file type</summary>
        public string FileType { get; protected set; }
        /// <summary>The 4 char file version</summary>
        public string Version { get; protected set; }
        /// <summary>Writes BioWare File data</summary>
        /// <param name="s">The Stream to which the File will be written</param>
        public abstract void Write(System.IO.Stream s);
    }
}
