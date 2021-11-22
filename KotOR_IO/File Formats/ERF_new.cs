using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// BioWare Encapsulated Resource File Data. <para/>
    /// See: <see cref="ERF_old(Stream)"/>
    /// <seealso  cref="Write(Stream)"/>
    /// <remarks>
    /// <para/>ERF files often come in the form of Save games (.SAV), active modules (.MOD), texture packs (.ERF), as well as hack-pack (.HAK)
    /// <para/>They simply store data with Key references for quick reading and writing by the game.
    /// </remarks>
    /// </summary>
    public class ERF_new : KFile, IEnumerable<ERF_new.Res>
    {
        //Types
        public class Res : IEquatable<Res>
        {
            public Res () { }
            public Res (string resref, ResourceType type, byte[] data, short unused = 0)
            {
                this.resref = resref;
                this.type = type;
                this.data = data;
                this.unused = unused;
            }

            public short unused;
            public string resref;
            public ResourceType type;
            public byte[] data;

            public int size { get { return data.Length; } }

            public bool Equals(Res other)
            {
                return (other.resref == resref && other.type == type);
            }
            public override int GetHashCode()
            {
                return resref.GetHashCode() * type.GetHashCode();
            }
            public override string ToString()
            {
                return $"{resref}.{type.ToDescription().ToLower().Trim()}";
            }
        }
        
        //Private Member Variables
        private List<Res> resources = new List<Res>();
        private Dictionary<LanguageID, string> LocalizedStrings = new Dictionary<LanguageID, string>();
        private int BuildYear;
        private int BuildDay;
        private int DescriptionStrRef;
        private byte[] Reserved_block;

        //Constructors
        public ERF_new(byte[] rawData)
            : this(new MemoryStream(rawData))
        { }
        public ERF_new(string path)
            : this(File.OpenRead(path))
        { }
        protected ERF_new(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));
                int LanguageCount = br.ReadInt32();
                int LocalizedStringSize = br.ReadInt32();
                int EntryCount = br.ReadInt32();
                int OffsetToLocalizedStrings = br.ReadInt32();
                int OffsetToKeyList = br.ReadInt32();
                int OffsetToResourceList = br.ReadInt32();
                BuildYear = br.ReadInt32();
                BuildDay = br.ReadInt32();
                DescriptionStrRef = br.ReadInt32();
                Reserved_block = br.ReadBytes(116);

                br.BaseStream.Seek(OffsetToLocalizedStrings, SeekOrigin.Begin);
                for (int i = 0; i < LanguageCount; i++)
                {
                    int LanguageID = br.ReadInt32();
                    int StringSize = br.ReadInt32();
                    string str = new string(br.ReadChars(StringSize));
                    LocalizedStrings.Add((KotOR_IO.LanguageID)LanguageID, str);
                }

                List<Tuple<string, int, short, short>> keys = new List<Tuple<string, int, short, short>>();
                br.BaseStream.Seek(OffsetToKeyList, SeekOrigin.Begin); //seek to key entries
                for (int i = 0; i < EntryCount; i++)
                {
                    string ResRef = new string(br.ReadChars(16)).TrimEnd('\0');
                    int ResID = br.ReadInt32();
                    short ResType = br.ReadInt16();
                    short Unused = br.ReadInt16();
                    keys.Add(new Tuple<string, int, short, short>(ResRef, ResID, ResType, Unused));
                }

                List<Tuple<int, int>> res_offs = new List<Tuple<int, int>>();
                br.BaseStream.Seek(OffsetToResourceList, SeekOrigin.Begin); //seek to resource list
                for (int i = 0; i < EntryCount; i++)
                {
                    int OffsetToResource = br.ReadInt32();
                    int ResourceSize = br.ReadInt32();
                    res_offs.Add(new Tuple<int, int>(OffsetToResource, ResourceSize));
                }

                foreach (var k in keys) //populate res data
                {
                    int resID = k.Item2;
                    br.BaseStream.Seek(res_offs[resID].Item1, 0);
                    byte[] data = br.ReadBytes(res_offs[resID].Item2);
                    resources.Add(new Res(k.Item1, (ResourceType)k.Item3, data, k.Item4));
                }
            }
        }

        //Writing
        internal override void Write(Stream s)
        {
            int OffsetToLocalizedStrings = 160;
            int LocalizedStringSize = 0;
            foreach (var ls in LocalizedStrings) LocalizedStringSize += 8 + ls.Value.Length;
            int OffsetToKeyList = OffsetToLocalizedStrings + LocalizedStringSize;
            int OffsetToResourceList = OffsetToKeyList + (16 + 4 + 2 + 2) * resources.Count();
            int BuildYear = DateTime.Now.Year - 1900;
            int BuildDay = DateTime.Now.DayOfYear;

            using (BinaryWriter bw = new BinaryWriter(s))
            {
                // Header
                bw.Write(FileType.ToArray());
                bw.Write(Version.ToArray());
                bw.Write(LocalizedStrings.Count());
                bw.Write(LocalizedStringSize);
                bw.Write(resources.Count());
                bw.Write(OffsetToLocalizedStrings);
                bw.Write(OffsetToKeyList);
                bw.Write(OffsetToResourceList);
                bw.Write(BuildYear);
                bw.Write(BuildDay);
                bw.Write(DescriptionStrRef);
                bw.Write(Reserved_block);

                // Localized String List
                bw.Seek(OffsetToLocalizedStrings, SeekOrigin.Begin);
                foreach (var ls in LocalizedStrings)
                {
                    bw.Write((int)ls.Key);
                    bw.Write(ls.Value.Length);
                    bw.Write(ls.Value.ToArray());
                }

                // Key List
                bw.Seek(OffsetToKeyList, SeekOrigin.Begin);
                int i = 0;
                foreach (var r in resources)
                {
                    bw.Write(r.resref.PadRight(16, '\0').ToArray());
                    bw.Write(i); i++;
                    bw.Write((short)r.type);
                    bw.Write(r.unused);
                }

                // Resource List
                bw.Seek(OffsetToResourceList, SeekOrigin.Begin);
                int OffsetToNextResource = OffsetToResourceList + 8 * resources.Count();
                List<int> res_offs = new List<int>();
                foreach (var r in resources)
                {
                    bw.Write(OffsetToNextResource); 
                    res_offs.Add(OffsetToNextResource);
                    OffsetToNextResource += r.size;
                    bw.Write(r.size);
                }

                // resource Data
                for (int j = 0; j < resources.Count; j++)
                {
                    bw.Seek(res_offs[j], 0);
                    bw.Write(resources[j].data);
                }

            }
        }

        //Public members
        public IEnumerator<Res> GetEnumerator()
        {
            return resources.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return resources.GetEnumerator();
        }
        public void AddRes(Res item)
        {
            resources.Add(item);
        }
        public void AddRes(string resref, byte[] data)
        {
            StringBuilder sb = new StringBuilder(4);
            sb.Append(new char[4] { (char)data[0], (char)data[1], (char)data[2], (char)data[3], });
            resources.Add(new Res(resref, (ResourceType)Reference_Tables.TypeCodes[sb.ToString()], data));
        }
        public void Clear()
        {
            resources.Clear();
        }
        public bool Contains(Res item)
        {
            return resources.Contains(item);
        }
        public void CopyTo(Res[] array, int arrayIndex = 0)
        {
            resources.CopyTo(array, arrayIndex);
        }
        public int Count => resources.Count;
        public bool Remove(Res item)
        {
            return resources.Remove(item);
        }
        public bool Remove(string resref, ResourceType type)
        {
            return resources.RemoveAll(r => r.resref == resref && r.type == type) > 0;
        }
        public Res this[int i]
        {
            get
            {
                return resources[i];
            }
            set
            {
                resources[i] = value;
            }
        }
        public Res[] this[string resref]
        {
            get { return resources.Where(r => r.resref == resref).ToArray(); }
        }
        public Res this[string resref, ResourceType type]
        {
            get { return resources.First(r => r.resref == resref && r.type == type); }
            set 
            { 
                resources.First(r => r.resref == resref && r.type == type).resref = value.resref;
                resources.First(r => r.resref == resref && r.type == type).type = value.type;
                resources.First(r => r.resref == resref && r.type == type).data = value.data;
            }
        }
    }
}
