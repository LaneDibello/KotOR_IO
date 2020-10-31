using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// Letter-combo probability info for name generation.
    /// </summary>
    public class LTR : KFile
    {
        private static readonly List<char> letters = new List<char>("abcdefghijklmnopqrstuvwxyz'-".ToArray());

        //Header
        byte num_letters;

        /// <summary> Letter data </summary>
        public ltrdata data = new ltrdata();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="names"></param>
        public LTR(List<string> names)
        {
            FileType = "LTR ";
            Version = "V1.0";
            num_letters = 28;

            int midcount = 0;

            data = new ltrdata(28);

            foreach (string name in names)
            {
                if (name.Length < 3) { continue; }

                data.singles.start[letters.IndexOf(name[0])] += 1.0f;
                data.doubles[letters.IndexOf(name[0])].start[letters.IndexOf(name[1])] += 1.0f;
                data.triples[letters.IndexOf(name[0])][letters.IndexOf(name[1])].start[letters.IndexOf(name[2])] += 1.0f;

                data.singles.end[letters.IndexOf(name.Last())] += 1.0f;
                data.doubles[letters.IndexOf(name[name.Length - 2])].end[letters.IndexOf(name.Last())] += 1.0f;
                data.triples[letters.IndexOf(name[name.Length - 3])][letters.IndexOf(name[name.Length - 2])].end[letters.IndexOf(name.Last())] += 1.0f;

                for (int i = 1; i < name.Length - 3; i++)
                {
                    data.singles.middle[letters.IndexOf(name[i])] += 1.0f;
                    data.doubles[letters.IndexOf(name[i])].middle[letters.IndexOf(name[i + 1])] += 1.0f;
                    data.triples[letters.IndexOf(name[i])][letters.IndexOf(name[i + 1])].middle[letters.IndexOf(name[i + 2])] += 1.0f;
                    midcount++;
                }
            }

            float s = 0.0f, m = 0.0f, e = 0.0f;

            for (int i = 0; i < num_letters; i++)
            {
                if (data.singles.start[i] > 0.0f)
                {
                    data.singles.start[i] /= names.Count;
                    s = data.singles.start[i] += s;
                }
                if (data.singles.end[i] > 0.0f)
                {
                    data.singles.end[i] /= names.Count;
                    e = data.singles.end[i] += e;
                }
                if (data.singles.middle[i] > 0.0f)
                {
                    data.singles.middle[i] /= midcount;
                    m = data.singles.middle[i] += m;
                }
            }

            s = m = e = 0.0f;

            for (int i = 0; i < num_letters; i++)
            {
                for (int j = 0; j < num_letters; j++)
                {
                    if (data.doubles[i].start[j] > 0.0f)
                    {
                        data.doubles[i].start[j] /= names.Count;
                        s = data.doubles[i].start[j] += s;
                    }
                    if (data.doubles[i].end[j] > 0.0f)
                    {
                        data.doubles[i].end[j] /= names.Count;
                        e = data.doubles[i].end[j] += e;
                    }
                    if (data.doubles[i].middle[j] > 0.0f)
                    {
                        data.doubles[i].middle[j] /= midcount;
                        m = data.doubles[i].middle[j] += m;
                    }
                }
            }

            for (int i = 0; i < num_letters; i++)
            {
                for (int j = 0; j < num_letters; j++)
                {
                    for (int k = 0; k < num_letters; k++)
                    {
                        if (data.triples[i][j].start[k] > 0.0f)
                        {
                            data.triples[i][j].start[k] /= names.Count;
                            s = data.triples[i][j].start[k] += s;
                        }
                        if (data.triples[i][j].end[k] > 0.0f)
                        {
                            data.triples[i][j].end[k] /= names.Count;
                            e = data.triples[i][j].end[k] += e;
                        }
                        if (data.triples[i][j].middle[k] > 0.0f)
                        {
                            data.triples[i][j].middle[k] /= midcount;
                            m = data.triples[i][j].middle[k] += m;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes Letter Data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        internal override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                bw.Write(FileType.ToArray());
                bw.Write(Version.ToArray());

                bw.Write((byte)28);

                //singles
                foreach (float f in data.singles.start)
                {
                    bw.Write(f);
                }
                foreach (float f in data.singles.middle)
                {
                    bw.Write(f);
                }
                foreach (float f in data.singles.end)
                {
                    bw.Write(f);
                }

                //Doubles
                foreach (LTR.cdf c in data.doubles)
                {
                    foreach (float f in c.start)
                    {
                        bw.Write(f);
                    }
                    foreach (float f in c.middle)
                    {
                        bw.Write(f);
                    }
                    foreach (float f in c.end)
                    {
                        bw.Write(f);
                    }
                }

                //triples
                foreach (LTR.cdf[] ca in data.triples)
                {
                    foreach (LTR.cdf c in ca)
                    {
                        foreach (float f in c.start)
                        {
                            bw.Write(f);
                        }
                        foreach (float f in c.middle)
                        {
                            bw.Write(f);
                        }
                        foreach (float f in c.end)
                        {
                            bw.Write(f);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes a file to the given path using the Name property in this class object.
        /// </summary>
        /// <param name="path">Path to the file to write.</param>
        public void WriteToFile(string path)
        {
            Write(File.OpenWrite(path));
        }

        public struct cdf
        {
            public float[] start;
            public float[] middle;
            public float[] end;

            public cdf(int numletters)
            {
                start = new float[numletters];
                middle = new float[numletters];
                end = new float[numletters];
            }
        }

        public struct ltrdata
        {
            public cdf singles;
            public cdf[] doubles;
            public cdf[][] triples;

            public ltrdata(int numletters)
            {
                singles = new cdf(numletters);

                doubles = new cdf[numletters];
                for (int i = 0; i < numletters; i++)
                {
                    doubles[i] = new cdf(numletters);
                }

                triples = new cdf[numletters][];
                for (int i = 0; i < numletters; i++)
                {
                    triples[i] = new cdf[numletters];
                    for (int j = 0; j < numletters; j++)
                    {
                        triples[i][j] = new cdf(numletters);
                    }
                }

            }
        }
    }
}
