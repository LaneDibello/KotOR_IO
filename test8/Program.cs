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


        static void Main(string[] args)
        {
            //DirectoryInfo di = new DirectoryInfo("C:\\Program Files (x86)\\Steam\\steamapps\\common\\swkotor\\modules - Copy");
            //foreach (FileInfo fi in di.EnumerateFiles())
            //{
            //    RIM r = new RIM(Path.Combine(fi.DirectoryName, fi.Name));
            //    r.WriteToFile(Path.Combine("C:\\Program Files (x86)\\Steam\\steamapps\\common\\swkotor\\modules\\", fi.Name));
            //}

            ERF_new e = new ERF_new(@"C:\Program Files (x86)\Steam\steamapps\common\swkotor\TexturePacks\swpc_tex_gui.erf");
            e.WriteToFile(@"C:\Program Files (x86)\Steam\steamapps\common\swkotor\TexturePacks\swpc_tex_gui2.erf");
            //RIM r = new RIM("D:\\ExampleFiles\\danm13.rim");
            //r.WriteToFile("D:\\ExampleFiles\\danm13T.rim");
            //RIM r2 = new RIM("D:\\ExampleFiles\\danm13T.rim");
            Console.Write("");
        }
    }
}
