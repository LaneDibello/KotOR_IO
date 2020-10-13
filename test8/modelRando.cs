using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KotOR_IO;

namespace testcontent
{
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        Random rng = new Random(55);
    //        DirectoryInfo di = new DirectoryInfo("L:\\laned\\Documents\\kotor stuffs\\DumbIdea\\modules");
    //        foreach (FileInfo f in di.EnumerateFiles())
    //        {
    //            RIM r = KReader.ReadRIM(f.OpenRead());

    //            foreach (RIM.rFile rf in r.File_Table.Where(k => k.TypeID == 2027))
    //            {
    //                GFF g = new GFF(rf.File_Data);

    //                int temp = rng.Next(1, 508);
    //                if (temp == 0 || temp == 29 || temp == 82) { temp = 200; }
    //                g.Field_Array.Where(k => k.Label == "Appearance_Type").FirstOrDefault().Field_Data = temp;
    //                g.Field_Array.Where(k => k.Label == "Appearance_Type").FirstOrDefault().DataOrDataOffset = temp;

    //                MemoryStream ms = new MemoryStream();

    //                kWriter.Write(g, ms);

    //                rf.File_Data = ms.ToArray();
    //            }

    //            kWriter.Write(r, f.OpenWrite());

    //        }
    //    }
    //}
}



