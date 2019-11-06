using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KotOR_IO;

namespace test8
{
    class Program
    {
        static void Main(string[] args)
        {
            ERF e = KReader.ReadERF(File.OpenRead("L:\\laned\\Documents\\kotor stuffs\\Biff Reader Test\\MYtest.erf"));

            MemoryStream ms = new MemoryStream();

            File.OpenRead("L:\\laned\\Documents\\kotor stuffs\\Biff Reader Test\\g_bandon.ute").CopyTo(ms);

            e.Append_File("g_bandon", ms.ToArray());

            byte[] b = e["g_bandon"];

            Console.Write("");

            //GFF g = KReader.ReadGFF(File.OpenRead("L:\\laned\\Documents\\kotor stuffs\\Biff Reader Test\\g_bandon.ute"));

            //foreach (GFF.Field gf in g.Field_Array)
            //{
            //    string tmp;
            //    switch (gf.Type)
            //    {
            //        case 0:
            //            tmp = "byte";
            //            break;
            //        case 1:
            //            tmp = "char";
            //            break;
            //        case 2:
            //        case 3:
            //            tmp = "short";
            //            break;
            //        case 4:
            //        case 5:
            //            tmp = "int";
            //            break;
            //        case 6:
            //        case 7:
            //            tmp = "long";
            //            break;
            //        case 8:
            //            tmp = "float";
            //            break;
            //        case 9:
            //            tmp = "double";
            //            break;
            //        case 10:
            //            tmp = "string";
            //            break;
            //        case 11:
            //            tmp = "string";
            //            break;
            //        case 12:
            //            tmp = "string";
            //            break;
            //        case 13:
            //            tmp = "byte[]";
            //            break;
            //        case 14:
            //            tmp = "GFF.GFFStruct";
            //            break;
            //        case 15:
            //            tmp = "List<object>";
            //            break;
            //        default:
            //            tmp = "";
            //            break;
            //    }

            //    Console.WriteLine("public " + tmp + " " + gf.Label.TrimEnd('\0') + ";");


            //}


            //Console.ReadKey();

        }
    }
}
