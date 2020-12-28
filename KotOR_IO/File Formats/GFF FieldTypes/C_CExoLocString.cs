using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KotOR_IO
{
    public partial class GFF
    {
        public class CExoLocString : FIELD
        {
            public int StringRef;

            public struct SubString
            {
                public int StringID;
                public string SString;
            }

            public List<SubString> Strings = new List<SubString>();

            public CExoLocString() { }
            public CExoLocString(BinaryReader br, int offset)
            {
                //Header Info
                br.BaseStream.Seek(24, 0);
                int LabelOffset = br.ReadInt32();
                int LabelCount = br.ReadInt32();
                int FieldDataOffset = br.ReadInt32();

                //Basic Field Data
                br.BaseStream.Seek(offset, 0);
                Type = br.ReadInt32();
                int LabelIndex = br.ReadInt32();
                int DataOrDataOffset = br.ReadInt32();

                //Label Logic
                br.BaseStream.Seek(LabelOffset + LabelIndex * 16, 0);
                Label = new string(br.ReadChars(16)).TrimEnd('\0');

                //Comlex Value Logic
                br.BaseStream.Seek(FieldDataOffset + DataOrDataOffset, 0);
                int TotalSize = br.ReadInt32();
                StringRef = br.ReadInt32();
                int StringCount = br.ReadInt32();
                for (int i = 0; i < StringCount; i++)
                {
                    SubString SS = new SubString();
                    SS.StringID = br.ReadInt32();
                    int StringLength = br.ReadInt32();
                    SS.SString = new string(br.ReadChars(StringLength));
                    Strings.Add(SS);
                }

            }

            internal override void collect_fields(ref List<Tuple<FIELD, int, int>> Field_Array, ref List<byte> Raw_Field_Data_Block, ref List<string> Label_Array, ref int Struct_Indexer, ref int List_Indices_Counter)
            {
                Tuple<FIELD, int, int> T = new Tuple<FIELD, int, int>(this, Raw_Field_Data_Block.Count, this.GetHashCode());
                int total_size = 8;
                foreach (SubString SS in Strings)
                {
                    total_size += 8 + SS.SString.Length;
                }
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(total_size));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(StringRef));
                Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(Strings.Count));
                foreach (SubString SS in Strings)
                {
                    Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(SS.StringID));
                    Raw_Field_Data_Block.AddRange(BitConverter.GetBytes(SS.SString.Length));
                    Raw_Field_Data_Block.AddRange(SS.SString.ToCharArray().Select(x => (byte)x));
                }


                Field_Array.Add(T);

                if (!Label_Array.Contains(Label))
                {
                    Label_Array.Add(Label);
                }
            }

            public override bool Equals(object obj)
            {
                if ((obj == null) || !GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else if (StringRef != -1 && (obj as CExoLocString).StringRef != -1)
                {
                    return StringRef == (obj as CExoLocString).StringRef && Label == (obj as CExoLocString).Label;
                }
                else if (Strings.Any() && (obj as CExoLocString).Strings.Any())
                {
                    for (int i = 0; i < Strings.Count; i++)
                    {
                        if (Strings[i].SString == (obj as CExoLocString).Strings[i].SString && Strings[i].StringID == (obj as CExoLocString).Strings[i].StringID)
                        {
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return Label == (obj as CExoLocString).Label;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                int partial_hash = 1;
                int[] IDArray = Strings.Select(x => x.StringID).ToArray();
                foreach (int i in IDArray)
                {
                    partial_hash *= i.GetHashCode();
                }
                string[] StringArray = Strings.Select(x => x.SString).ToArray();
                foreach (string s in StringArray)
                {
                    partial_hash *= s.GetHashCode();
                }
                return new { Type, StringRef, partial_hash, Label }.GetHashCode();
            }


        }
    }
} 