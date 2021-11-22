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
            public Res (string resref, ResourceType type, byte[] data)
            {
                this.resref = resref;
                this.type = type;
                this.data = data;
            }
            internal Res (BinaryReader br, int res_off, int key_off)
            {
                throw new NotImplementedException();
            }

            private byte[] unused;

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

        //Public Properties
        public int Count => resources.Count;

        //Constructors
        internal override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        //Collection Interface
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
        public bool Remove(Res item)
        {
            return resources.Remove(item);
        }
    }
}
