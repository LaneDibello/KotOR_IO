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
        public static void FisherYatesShuffle<T>(IList<T> list)
        {
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.Rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static class ThreadSafeRandom
        {
            [ThreadStatic]
            private static Random Local;

            public static Random Rng
            {
                get
                {
                    return Local ?? (Local = new Random(Seed));
                }
            }

            public static int Seed { get; private set; } = unchecked(Environment.TickCount * 31 + System.Threading.Thread.CurrentThread.ManagedThreadId);

            public static int GenerateSeed()
            {
                Seed = Rng.Next();
                return Seed;
            }

            public static void SetSeed(int seed)
            {
                Seed = seed;
                Local = null;
            }

            public static void RestartRng()
            {
                Local = null;
            }
        }

        static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            const int BYTES_TO_READ = sizeof(Int64);
            if (first.Length != second.Length)
                return false;

            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }

            return true;
        }

        static void Main(string[] args)
        {

            GFF j = new GFF(@"L:\laned\Documents\kotor stuffs\global.jrl");
            TLK t = new TLK(@"C:\Program Files (x86)\Steam\steamapps\common\swkotor\dialog.tlk");

            var questlist = (j.Top_Level.Fields.First(f => f.Label == "Categories") as GFF.LIST).Structs;

            foreach (var quest in questlist)
            {
                int talkID = (quest.Fields.First(f => f.Label == "Name") as GFF.CExoLocString).StringRef;
                Console.WriteLine(t[talkID]);
            }

            Console.ReadKey();



            //WOK w = new WOK(@"L:\laned\Documents\kotor stuffs\Walkmeshes\m01aa_01a.wok");
            //LYT l = new LYT(File.OpenRead(@"L:\laned\Documents\kotor stuffs\m01aa.lyt"));
            ////w.positon = new Tuple<float, float, float>(100, 100, 500);
            //w.translate(0, 0, 0);
            //foreach (var room in l.Rooms)
            //{
            //    room.X += 0;
            //    room.Y += 0;
            //    room.Z += 0;
            //}
            //w.WriteToFile(@"C:\Program Files (x86)\Steam\steamapps\common\swkotor\Override\m01aa_01a.wok");
            //l.WriteToFile(@"C:\Program Files (x86)\Steam\steamapps\common\swkotor\Override\m01aa.lyt");

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //WOK w = new WOK(@"L:\laned\Documents\kotor stuffs\Walkmeshes\m18ac_01a.wok");
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed.TotalSeconds);
            //Console.WriteLine(w.ToString());
            //Console.ReadKey();

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //DirectoryInfo di = new DirectoryInfo(@"L:\laned\Documents\kotor stuffs\Walkmeshes\");
            //foreach (FileInfo fi in di.GetFiles())
            //{
            //    WOK w = new WOK(fi.OpenRead());
            //    w.WriteToFile($@"L:\laned\Documents\kotor stuffs\TestWOKOut\{fi.Name}");
            //    if (FilesAreEqual(fi, new FileInfo($@"L:\laned\Documents\kotor stuffs\TestWOKOut\{fi.Name}")))
            //    {
            //        Console.WriteLine($"No Differences found for {fi.Name}");
            //    }
            //    else
            //    {
            //        Console.BackgroundColor = ConsoleColor.Red;
            //        Console.WriteLine($"{fi.Name} differs");
            //        Console.BackgroundColor = ConsoleColor.Black;
            //    }
            //}
            //sw.Stop();
            //Console.WriteLine($"Read and wrote all meshes in only {sw.Elapsed.TotalSeconds} seconds.");
            //Console.ReadKey();


            //var fileToRead = @"C:\Program Files (x86)\Steam\steamapps\common\swkotor\data\templates.bif";
            //BIF templates = new BIF(fileToRead);
            //GFF utc = new GFF(templates.VariableResourceTable.First(vre => vre.ResourceType == ResourceType.UTC).EntryData);

            //// Read GFF file.
            //var filename = @"C:\Dev\KIO Test\test1.git";
            //var fileinfo = new FileInfo(filename);
            //Console.WriteLine($" file size: {fileinfo.Length:N0} bytes");

            //GFF test = new GFF(filename);
            //Console.WriteLine($" read size: {test.ToRawData().Length:N0} bytes");

            //// Write GFF object to file.
            //var filename2 = @"C:\Dev\KIO Test\test2.git";
            //test.WriteToFile(filename2);

            //var fileinfo2 = new FileInfo(filename2);
            //Console.WriteLine($"write size: {fileinfo2.Length:N0} bytes");

            //Console.ReadLine();
        }
    }
}
