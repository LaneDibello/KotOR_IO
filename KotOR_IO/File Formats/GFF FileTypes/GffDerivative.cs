using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO.GffFile
{
    public abstract class GffDerivative : KFile
    {
        public GFF GFF { get; private set; }

        protected GffDerivative(GFF gff)
        {
            GFF = gff;
        }

        internal override void Write(Stream s)
        {
            GFF.Write(s);
        }
    }
}
