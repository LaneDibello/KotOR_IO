using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/*TODO: 
 * 
 * Create detailed constructors, with lots of back-end to make creating these files very user friendly
 * Consider separate class for GFF derivatives
 * Continue populating XML documentation.
 * ADD: GFF add field, add struct, add label, etc. These will be used in "set" accessors in Blueprints. GFF constructors will accept different blueprints, and call their own GFF seeding method.
 * 
 * 
 */

namespace KotOR_IO 
{
    /// <summary>
    /// Contains methods for reading common Bioware/SW:KotOR File formats
    /// </summary>
    public static class KReader 
    {
        /// <summary>
        /// Reads Bioware 2-Dimensional Array (v2.b) files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        static public TwoDA Read2DA(Stream s)
        {
            TwoDA t = new TwoDA();

            BinaryReader br = new BinaryReader(s);

            //Get header info
            t.FileType = new string(br.ReadChars(4));
            t.Version = new string(br.ReadChars(4));

            br.ReadByte();

            //Get Column Labels
            List<char> TempString = new List<char>();

            while (br.PeekChar() != 0)
            {
                TempString.Clear();
                while (br.PeekChar() != 9) //May have to make this go one past the current limit
                {
                    TempString.Add(br.ReadChar());
                }
                t.Columns.Add(new string(TempString.ToArray()));
                br.ReadByte();
            }

            br.ReadByte();

            //Get row count
            t.Row_Count = br.ReadInt32();

            //Skip row indexes (maybe a bad idea, but who cares)
            for(int i = 0; i < t.Row_Count; i++)
            {
                while (br.PeekChar() != 9)
                {
                    br.ReadByte();
                }
                br.ReadByte();
            }

            //generate index column
            List<object> index_list = new List<object>();
            for (int i = 0; i < t.Row_Count; i++) { index_list.Add(Convert.ToString(i)); }
            t.Data.Add("row_index", index_list);

            //populate collumn keys
            foreach (string c in t.Columns) { List<object> tempColumn = new List<object>(); t.Data.Add(c, tempColumn); }

            //get offsets
            for (int i = 0; i < (1 + (t.Row_Count * t.Columns.Count())); i++) //iterates through the number of cells
            {
                t.Offsets.Add(br.ReadInt16());
            }
            int DataOffset = (int)br.BaseStream.Position;

            //Populate data
            int OffsetIndex = 0;
            for (int i = 0; i < t.Row_Count; i++)
            {
                for (int k = 0; k < t.Columns.Count(); k++)
                {
                    br.BaseStream.Seek(DataOffset + t.Offsets[OffsetIndex], SeekOrigin.Begin);
                    TempString.Clear();
                    while (br.PeekChar() != 0)
                    {
                        TempString.Add(br.ReadChar());
                    }
                    t.Data[t.Columns[k]].Add(new string(TempString.ToArray()));
                    br.ReadByte();
                    OffsetIndex++;
                }
            }
            t.IsParsed = false;
            br.Close();
            return t;
        }

        /// <summary>
        /// Reads Bioware Built In Files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        static public BIF ReadBIF(Stream s)
        {
            BIF b = new BIF();

            BinaryReader br = new BinaryReader(s);

            //Get header info
            b.FileType = new string(br.ReadChars(4));
            b.Version = new string(br.ReadChars(4));
            b.Variable_Resource_Count = br.ReadInt32();
            b.Fixed_Resource_Count = br.ReadInt32();
            b.Variable_Table_Offset = br.ReadInt32();

            //Get Variable Resource Table
            for (int i = 0; i < b.Variable_Resource_Count; i++)
            {
                BIF.Var_Res_Entry VRE = new BIF.Var_Res_Entry();
                VRE.ID = br.ReadInt32();
                VRE.Offset = br.ReadInt32();
                VRE.File_Size = br.ReadInt32();
                VRE.Resource_Type_code = br.ReadInt32();
                try { VRE.Resource_Type_text = Reference_Tables.Res_Types[VRE.Resource_Type_code]; }
                catch (KeyNotFoundException e) { VRE.Resource_Type_code = 0; VRE.Resource_Type_text = "null"; }
                VRE.IDx = VRE.ID >> 20;
                VRE.IDy = VRE.ID - (VRE.IDx << 20);
                VRE.Entry_Data = new byte[VRE.File_Size];
                b.Variable_Resource_Table.Add(VRE);
            }

            //Get Fixed Resource Table
            for (int i = 0; i < b.Fixed_Resource_Count; i++)
            {
                BIF.Fixed_Res_Entry FRE = new BIF.Fixed_Res_Entry();
                FRE.ID = br.ReadInt32();
                FRE.Offset = br.ReadInt32();
                FRE.PartCount = br.ReadInt32();
                FRE.File_Size = br.ReadInt32();
                FRE.Resource_Type_code = br.ReadInt32();
                FRE.Resource_Type_text = Reference_Tables.Res_Types[FRE.Resource_Type_code];
                FRE.Entry_Data = new byte[FRE.File_Size];
                b.Fixed_Resource_Table.Add(FRE);
            }

            //Populate Variable Resource Data
            foreach (BIF.Var_Res_Entry VRE in b.Variable_Resource_Table)
            {
                VRE.Entry_Data = br.ReadBytes(VRE.File_Size);
            }

            //Populate Fixed Resource Data
            foreach (BIF.Fixed_Res_Entry FRE in b.Fixed_Resource_Table)
            {
                FRE.Entry_Data = br.ReadBytes(FRE.File_Size);
            }

            br.Close();
            return b;
        }

        /// <summary>
        /// Reads Bioware Encapsulated Resource Files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        static public ERF ReadERF(Stream s)
        {
            ERF e = new ERF();
            BinaryReader br = new BinaryReader(s);

            //header
            e.FileType = new string(br.ReadChars(4));
            e.Version = new string(br.ReadChars(4));
            e.LanguageCount = br.ReadInt32();
            e.LocalizedStringSize = br.ReadInt32();
            e.EntryCount = br.ReadInt32();
            e.OffsetToLocalizedStrings = br.ReadInt32();
            e.OffsetToKeyList = br.ReadInt32();
            e.OffsetToResourceList = br.ReadInt32();
            e.BuildYear = br.ReadInt32();
            e.BuildDay = br.ReadInt32();
            e.DescriptionStrRef = br.ReadInt32();
            e.Reserved_block = br.ReadBytes(116);

            br.BaseStream.Seek(e.OffsetToLocalizedStrings, SeekOrigin.Begin); //seek to local strings
            for(int i = 0;  i < e.LanguageCount; i++)
            {
                ERF.String_List_Element SLE = new ERF.String_List_Element();
                SLE.LanguageID = br.ReadInt32();
                SLE.Language = Reference_Tables.Language_IDs[SLE.LanguageID];
                SLE.StringSize = br.ReadInt32();
                SLE.String = new string(br.ReadChars(SLE.StringSize));
                e.Localized_String_List.Add(SLE);
            }

            br.BaseStream.Seek(e.OffsetToKeyList, SeekOrigin.Begin); //seek to key entries
            for(int i = 0; i < e.EntryCount; i++)
            {
                ERF.Key key = new ERF.Key();
                key.ResRef = new string(br.ReadChars(16)).TrimEnd('\0');
                key.ResID = br.ReadInt32();
                key.ResType = br.ReadInt16();
                key.Type_string = Reference_Tables.Res_Types[key.ResType];
                key.Unused = br.ReadInt16();
                e.Key_List.Add(key);
            }

            br.BaseStream.Seek(e.OffsetToResourceList, SeekOrigin.Begin); //seek to resource list
            for(int i = 0; i < e.EntryCount; i++)
            {
                ERF.Resource Res = new ERF.Resource();
                Res.OffsetToResource = br.ReadInt32();
                Res.ResourceSize = br.ReadInt32();
                e.Resource_List.Add(Res);
            }

            foreach(ERF.Resource r in e.Resource_List) //populate resource_data for each resource in the list
            {
                br.BaseStream.Seek(r.OffsetToResource, SeekOrigin.Begin); //seek to raw resource data
                r.Resource_data = br.ReadBytes(r.ResourceSize);
            }

            br.Close();
            return e;
        }

        /// <summary>
        /// Reads Bioware General File Format Files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        static public GFF ReadGFF(Stream s)
        {
            GFF g = new GFF();

            BinaryReader br = new BinaryReader(s);

            //header
            g.FileType = new string(br.ReadChars(4));
            g.Version = new string(br.ReadChars(4));
            g.StructOffset = br.ReadInt32();
            g.StructCount = br.ReadInt32();
            g.FieldOffset = br.ReadInt32();
            g.FieldCount = br.ReadInt32();
            g.LabelOffset = br.ReadInt32();
            g.LabelCount = br.ReadInt32();
            g.FieldDataOffset = br.ReadInt32();
            g.FieldDataCount = br.ReadInt32();
            g.FieldIndicesOffset = br.ReadInt32();
            g.FieldIndicesCount = br.ReadInt32();
            g.ListIndicesOffset = br.ReadInt32();
            g.ListIndicesCount = br.ReadInt32();

            br.BaseStream.Seek(g.StructOffset, SeekOrigin.Begin);
            //Struct Array
            for(int i = 0; i < g.StructCount; i++)
            {
                GFF.GFFStruct GS = new GFF.GFFStruct();
                GS.Type = br.ReadInt32();
                GS.DataOrDataOffset = br.ReadInt32();
                GS.FieldCount = br.ReadInt32();
                g.Struct_Array.Add(GS);
            }

            br.BaseStream.Seek(g.FieldOffset, SeekOrigin.Begin);
            //Field Array
            for(int i = 0; i < g.FieldCount; i++)
            {
                GFF.Field GF = new GFF.Field();
                GF.Type = br.ReadInt32();
                GF.LabelIndex = br.ReadInt32();
                GF.DataOrDataOffset = br.ReadInt32();
                GF.Type_Text = Reference_Tables.Field_Types[GF.Type];
                GF.Complex = Reference_Tables.Complex_Field_Types.Contains(GF.Type);
                g.Field_Array.Add(GF);
            }

            br.BaseStream.Seek(g.LabelOffset, SeekOrigin.Begin);
            //Label Array
            for(int i = 0; i < g.LabelCount; i++)
            {
                string l = new string (br.ReadChars(16)).TrimEnd('\0');
                g.Label_Array.Add(l);
            }

            //Attaching Labels to fields
            foreach (GFF.Field GF in g.Field_Array)
            {
                GF.Label = g.Label_Array[GF.LabelIndex];
            }

            br.BaseStream.Seek(g.FieldIndicesOffset, SeekOrigin.Begin);
            //Field Indices
            for(int i = 0; i < (g.FieldIndicesCount / 4); i++)
            {
                g.Field_Indices.Add(br.ReadInt32());
            }

            br.BaseStream.Seek(g.ListIndicesOffset, SeekOrigin.Begin);
            //List Indices
            for(int i = 0; i < g.ListIndicesCount; i++)
            {
                GFF.List_Index LI = new GFF.List_Index();
                LI.Size = br.ReadInt32();
                i += 4;
                for(int k = 0; k < LI.Size; k++)
                {
                    LI.Indices.Add(br.ReadInt32());
                    i += 4;
                }
                g.List_Indices.Add(LI);
            }

            //Field Data / populating each fields Field_data array (except maybe for LIst and structs)
            foreach (GFF.Field GF in g.Field_Array)
            {
                br.BaseStream.Seek(g.FieldDataOffset, SeekOrigin.Begin);

                if (!GF.Complex)
                {
                    #region non-complex switch
                    switch (GF.Type)
                    {
                        case 0:
                            GF.Field_Data = BitConverter.GetBytes(GF.DataOrDataOffset);
                            break;
                        case 1:
                            GF.Field_Data = BitConverter.ToChar(BitConverter.GetBytes(GF.DataOrDataOffset), 0);
                            break;
                        case 2:
                            GF.Field_Data = BitConverter.ToUInt16(BitConverter.GetBytes(GF.DataOrDataOffset), 0);
                            break;
                        case 3:
                            GF.Field_Data = BitConverter.ToInt16(BitConverter.GetBytes(GF.DataOrDataOffset), 0);
                            break;
                        case 4:
                            GF.Field_Data = BitConverter.ToUInt32(BitConverter.GetBytes(GF.DataOrDataOffset), 0);
                            break;
                        case 5:
                            GF.Field_Data = BitConverter.ToInt32(BitConverter.GetBytes(GF.DataOrDataOffset), 0);
                            break;
                        case 8:
                            GF.Field_Data = BitConverter.ToSingle(BitConverter.GetBytes(GF.DataOrDataOffset), 0);
                            break;
                            #endregion
                    }
                }
                else if (GF.Complex)
                {
                    #region complex switch
                    br.BaseStream.Seek(GF.DataOrDataOffset, SeekOrigin.Current);
                    switch (GF.Type)
                    {
                        case 6:
                            GF.Field_Data = br.ReadUInt64();
                            break;
                        case 7:
                            GF.Field_Data = br.ReadInt64();
                            break;
                        case 9:
                            GF.Field_Data = br.ReadDouble();
                            break;
                        case 10:
                            GFF.CExoString CES = new GFF.CExoString();
                            CES.Size = br.ReadInt32();
                            CES.Text = new string(br.ReadChars(CES.Size));
                            GF.Field_Data = CES;
                            break;
                        case 11:
                            GFF.CResRef CRR = new GFF.CResRef();
                            CRR.Size = br.ReadByte();
                            CRR.Text = new string(br.ReadChars(CRR.Size));
                            GF.Field_Data = CRR;
                            break;
                        case 12:
                            GFF.CExoLocString CELS = new GFF.CExoLocString();
                            CELS.Total_Size = br.ReadInt32();
                            CELS.StringRef = br.ReadInt32();
                            CELS.StringCount = br.ReadInt32();
                            for(int i = 0; i < CELS.StringCount; i++)
                            {
                                GFF.CExoLocString.SubString SS = new GFF.CExoLocString.SubString();
                                SS.StringID = br.ReadInt32();
                                SS.StringLength = br.ReadInt32();
                                SS.Text = new string(br.ReadChars(SS.StringLength));
                                CELS.SubStringList.Add(SS);
                            }
                            GF.Field_Data = CELS;
                            break;
                        case 13:
                            GFF.Void_Binary VB = new GFF.Void_Binary();
                            VB.Size = br.ReadInt32();
                            VB.Data = br.ReadBytes(VB.Size);
                            GF.Field_Data = VB;
                            break;
                        case 14:
                            GF.Field_Data = g.Struct_Array[GF.DataOrDataOffset];
                            break;
                        case 15:
                            //List Stuff (maybe separate conditional group?)
                            br.BaseStream.Seek(g.ListIndicesOffset, SeekOrigin.Begin);
                            br.BaseStream.Seek(GF.DataOrDataOffset, SeekOrigin.Current);
                            List<GFF.GFFStruct> LGS = new List<GFF.GFFStruct>();
                            int tempSize = br.ReadInt32();
                            for(int i = 0; i < tempSize; i++)
                            {
                                LGS.Add(g.Struct_Array[br.ReadInt32()]);
                            }
                            GF.Field_Data = LGS;
                            break;
                        #endregion
                    }
                }
            }
            
            //populate StructData
            foreach (GFF.GFFStruct GS in g.Struct_Array)
            {
                if(GS.FieldCount == 1)
                {
                    GS.StructData = g.Field_Array[GS.DataOrDataOffset];
                }
                else if (GS.FieldCount > 1)
                {
                    List<GFF.Field> LGF = new List<GFF.Field>();
                    for (int i = 0; i < GS.FieldCount; i++)
                    {
                        br.BaseStream.Seek(GS.DataOrDataOffset + g.FieldIndicesOffset, SeekOrigin.Begin);
                        LGF.Add(g.Field_Array[br.ReadInt32()]);
                    }
                    GS.StructData = LGF;
                }
                else
                {
                    GS.StructData = null;
                }
            }

            br.Close();
            return g;
        }

        /// <summary>
        /// Reads Bioware Key Files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        static public KEY ReadKEY(Stream s)
        {
            KEY k = new KEY();

            BinaryReader br = new BinaryReader(s);

            //header
            k.FileType = new string(br.ReadChars(4));
            k.Version = new string(br.ReadChars(4));
            k.BIFCount = br.ReadInt32();
            k.KeyCount = br.ReadInt32();
            k.OffsetToFileTable = br.ReadInt32();
            k.OffsetToKeyTable = br.ReadInt32();
            k.Build_Year = br.ReadInt32();
            k.Build_Day = br.ReadInt32();
            k.reserved = br.ReadBytes(32);

            //File Table
            br.BaseStream.Seek(k.OffsetToFileTable, SeekOrigin.Begin);
            for(int i = 0; i < k.BIFCount; i++)
            {
                KEY.File_Entry FE = new KEY.File_Entry();
                FE.FileSize = br.ReadInt32();
                FE.FilenameOffset = br.ReadInt32();
                FE.FilenameSize = br.ReadInt16();
                FE.Drives = br.ReadInt16();
                k.File_Table.Add(FE);
            }

            //Filenames
            foreach(KEY.File_Entry FE in k.File_Table)
            {
                br.BaseStream.Seek(FE.FilenameOffset, SeekOrigin.Begin);
                FE.Filename = new string(br.ReadChars(FE.FilenameSize));
            }

            //Key Table
            br.BaseStream.Seek(k.OffsetToKeyTable, SeekOrigin.Begin);
            for(int i = 0; i < k.KeyCount; i++)
            {
                KEY.Key_Entry KE = new KEY.Key_Entry();
                KE.ResRef = new string(br.ReadChars(16)).TrimEnd('\0');
                KE.ResourceType = br.ReadInt16();
                KE.Type_Text = Reference_Tables.Res_Types[KE.ResourceType];
                KE.ResID = br.ReadInt32();
                KE.IDx = KE.ResID >> 20;
                KE.IDy = KE.ResID - (KE.IDx << 20);

                k.Key_Table.Add(KE);

            }

            br.Close();
            return k;
        }

        /// <summary>
        /// Reads Bioware "RIM" Files 
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        static public RIM ReadRIM(Stream s)
        {
            RIM r = new RIM();

            BinaryReader br = new BinaryReader(s);

            //header
            r.FileType = new string(br.ReadChars(4));
            r.Version = new string(br.ReadChars(4));
            r.Unknown = br.ReadBytes(4);
            r.FileCount = br.ReadInt32();
            r.File_Table_Offset = br.ReadInt32();
            r.IsExtension = br.ReadBoolean();
            r.Reserved = br.ReadBytes(99);

            br.BaseStream.Seek(r.File_Table_Offset, SeekOrigin.Begin);
            //File Table
            for(int i = 0; i < r.FileCount; i++)
            {
                RIM.rFile RF = new RIM.rFile();
                RF.Label = new string(br.ReadChars(16)).TrimEnd('\0');
                RF.TypeID = br.ReadInt32();
                RF.Index = br.ReadInt32();
                RF.DataOffset = br.ReadInt32();
                RF.DataSize = br.ReadInt32();
                r.File_Table.Add(RF);
            }

            //populate FileData
            foreach (RIM.rFile RF in r.File_Table)
            {
                br.BaseStream.Seek(RF.DataOffset, SeekOrigin.Begin);
                RF.File_Data = br.ReadBytes(RF.DataSize + 4); //Add an extra four bytes of padding into the null separater to eleminate size bound errors
            }

            br.Close();
            return r;
        }

        /// <summary>
        /// Reads Bioware Sound Set Format (v1.1) Files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        static public SSF ReadSSF(Stream s)
        {
            SSF f = new SSF();

            BinaryReader br = new BinaryReader(s);

            //header 
            f.FileType = new string(br.ReadChars(4));
            f.Version = new string(br.ReadChars(4));
            f.UnknownInt = br.ReadInt32();

            //string refs
            
            for (int k = 0; k < 28; k++)
            {
                f.StringTable.Add(Reference_Tables.SSFields[k], br.ReadInt32());
            }

            f.EndPadding = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));

            f.TLKPopulated = false;
            br.Close();
            return f;
        }

        /// <summary>
        /// Reads Bioware Talk Table Files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        static public TLK ReadTLK(Stream s) 
        {
            TLK t = new TLK();

            BinaryReader br = new BinaryReader(s);

            //header
            t.FileType = new string(br.ReadChars(4));
            t.Version = new string(br.ReadChars(4));
            t.LanguageID = br.ReadInt32();
            t.StringCount = br.ReadInt32();
            t.StringEntriesOffset = br.ReadInt32();

            //String Data Table
            for(int i = 0; i < t.StringCount; i++)
            {
                TLK.String_Data SD = new TLK.String_Data();
                SD.Flags = br.ReadInt32();
                int tempFlags = SD.Flags;
                if (tempFlags >= 4) { SD.SNDLENGTH_PRESENT = true; tempFlags -= 4; }
                if (tempFlags >= 2) { SD.SND_PRESENT = true; tempFlags -= 2; }
                if (tempFlags >= 1) { SD.TEXT_PRESENT = true; tempFlags--; }
                SD.SoundResRef = new string(br.ReadChars(16)).TrimEnd('\0');
                SD.VolumeVariance = br.ReadInt32();
                SD.PitchVariance = br.ReadInt32();
                SD.OffsetToString = br.ReadInt32();
                SD.StringSize = br.ReadInt32();
                SD.SoundLength = br.ReadSingle();
                
                t.String_Data_Table.Add(SD);
            }

            //populating String text from string entires
            foreach (TLK.String_Data SD in t.String_Data_Table)
            {
                br.BaseStream.Seek(t.StringEntriesOffset + SD.OffsetToString, SeekOrigin.Begin);
                SD.StringText = new string(br.ReadChars(SD.StringSize));
            }

            br.Close();
            return t;
        }

        /// <summary>
        /// Reads miscellaneous Kotor source Files into a <see cref="MiscType"/> class.
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        /// <returns></returns>
        static public MiscType ReadKFile(Stream s)
        {
            MiscType m = new MiscType();

            BinaryReader br = new BinaryReader(s);

            //header
            m.FileType = new string(br.ReadChars(4));
            m.Version = new string(br.ReadChars(4));

            //Data
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            m.data = br.ReadBytes((int)br.BaseStream.Length);

            br.Close();
            return m;
        }
    }


    /// <summary>
    /// Contains methods for writing common Bioware/SW:KotOR File Formats
    /// </summary>
    public static class kWriter
    {
        /// <summary>
        /// Writes Bioware 2-Dimensional Array (v2.b) data
        /// </summary>
        /// <param name="t">The 2-Dimensional Array File to be written</param>
        /// <param name="s">The Stream to which the File will be written</param>
        public static void Write(TwoDA t, Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);

            //Header
            bw.Write(t.FileType.ToArray());
            bw.Write(t.Version.ToArray());
            
            bw.Write((byte)10);

            //Column Labels
            foreach (string c in t.Columns)
            {
                bw.Write(c.ToArray());
                bw.Write((byte)9);
            }

            bw.Write((byte)0);

            //Row Count
            bw.Write(t.Row_Count);

            //Row Indexs
            for (int i = 0; i < t.Row_Count; i++)
            {
                bw.Write(Convert.ToString(i).ToArray());
                bw.Write((byte)9);
            }

            //Offsets
            foreach (short sh in t.Offsets)
            {
                bw.Write(sh);
            }

            int DataOffset = (int)bw.BaseStream.Position;

            //Data
            List<short> CheckedOffsets = new List<short>();
            int row_index = 0;
            int col_index = 0;
            foreach (short sh in t.Offsets)
            {
                if (!CheckedOffsets.Contains(sh))
                {
                    string tempData = Convert.ToString(t.Data[t.Columns[col_index]][row_index]);
                    bw.Seek(DataOffset + sh, SeekOrigin.Begin);
                    bw.Write(tempData.ToArray());
                    bw.Write((byte)0);
                    CheckedOffsets.Add(sh);
                }
                col_index++;

                if (col_index == t.Columns.Count)
                {
                    col_index = 0;
                    row_index++;
                    if (row_index == t.Row_Count) { break; }
                }
            }

            bw.Close();
        }

        /// <summary>
        /// Writes Bioware Built In File data
        /// </summary>
        /// <param name="b">The Built In File to be written</param>
        /// <param name="s">The Stream to which the File will be written</param>
        public static void Write(BIF b, Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);

            //Header
            bw.Write(b.FileType.ToArray());
            bw.Write(b.Version.ToArray());
            bw.Write(b.Variable_Resource_Count);
            bw.Write(b.Fixed_Resource_Count);
            bw.Write(b.Variable_Table_Offset);

            //Variable Resource Tabale
            bw.Seek(b.Variable_Table_Offset, SeekOrigin.Begin);
            foreach (BIF.Var_Res_Entry VRE in b.Variable_Resource_Table)
            {
                bw.Write(VRE.ID);
                bw.Write(VRE.Offset);
                bw.Write(VRE.File_Size);
                bw.Write(VRE.Resource_Type_code);
            }

            //Fixed Resource Table *NOT USED*
            foreach (BIF.Fixed_Res_Entry FRE in b.Fixed_Resource_Table)
            {
                bw.Write(FRE.ID);
                bw.Write(FRE.Offset);
                bw.Write(FRE.PartCount);
                bw.Write(FRE.File_Size);
                bw.Write(FRE.Resource_Type_code);
            }

            //Variable resource Data
            foreach (BIF.Var_Res_Entry VRE in b.Variable_Resource_Table)
            {
                bw.Write(VRE.Entry_Data);
            }

            //Fixed Resource Data
            foreach (BIF.Fixed_Res_Entry FRE in b.Fixed_Resource_Table)
            {
                bw.Write(FRE.Entry_Data);
            }
            bw.Close();
        }

        /// <summary>
        /// Writes Bioware Encapsulated Resource File data
        /// </summary>
        /// <param name="e">The Encapsulated Resource File to be written</param>
        /// <param name="s">The Stream to which the File will be written</param>
        public static void Write(ERF e, Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);

            //Header
            bw.Write(e.FileType.ToArray());
            bw.Write(e.Version.ToArray());
            bw.Write(e.LanguageCount);
            bw.Write(e.LocalizedStringSize);
            bw.Write(e.EntryCount);
            bw.Write(e.OffsetToLocalizedStrings);
            bw.Write(e.OffsetToKeyList);
            bw.Write(e.OffsetToResourceList);
            bw.Write(e.BuildYear);
            bw.Write(e.BuildDay);
            bw.Write(e.DescriptionStrRef);
            bw.Write(e.Reserved_block);

            //Localized String List
            bw.Seek(e.OffsetToLocalizedStrings, SeekOrigin.Begin);
            foreach (ERF.String_List_Element SLE in e.Localized_String_List)
            {
                bw.Write(SLE.LanguageID);
                bw.Write(SLE.StringSize);
                bw.Write(SLE.String.ToArray());
            }

            //Key LIst
            bw.Seek(e.OffsetToKeyList, SeekOrigin.Begin);
            foreach (ERF.Key EK in e.Key_List)
            {
                bw.Write(EK.ResRef.PadRight(16, '\0').ToArray());
                bw.Write(EK.ResID);
                bw.Write(EK.ResType);
                bw.Write(EK.Unused);
            }

            //Resource List
            bw.Seek(e.OffsetToResourceList, SeekOrigin.Begin);
            foreach (ERF.Resource ER in e.Resource_List)
            {
                bw.Write(ER.OffsetToResource);
                bw.Write(ER.ResourceSize);
            }

            //Resource Data
            foreach (ERF.Resource ER in e.Resource_List)
            {
                bw.Seek(ER.OffsetToResource, SeekOrigin.Begin);
                bw.Write(ER.Resource_data);
            }

            bw.Close();
        }

        /// <summary>
        /// Writes Bioware General File Format data
        /// </summary>
        /// <param name="g">The General File Format File to be written</param>
        /// <param name="s">The Stream to which the File will be written</param>
        public static void Write(GFF g, Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);

            //Header
            bw.Write(g.FileType.ToArray());
            bw.Write(g.Version.ToArray());
            bw.Write(g.StructOffset);
            bw.Write(g.StructCount);
            bw.Write(g.FieldOffset);
            bw.Write(g.FieldCount);
            bw.Write(g.LabelOffset);
            bw.Write(g.LabelCount);
            bw.Write(g.FieldDataOffset);
            bw.Write(g.FieldDataCount);
            bw.Write(g.FieldIndicesOffset);
            bw.Write(g.FieldIndicesCount);
            bw.Write(g.ListIndicesOffset);
            bw.Write(g.ListIndicesCount);

            //Struct Array
            bw.Seek(g.StructOffset, SeekOrigin.Begin);
            foreach (GFF.GFFStruct GS in g.Struct_Array)
            {
                bw.Write(GS.Type);
                bw.Write(GS.DataOrDataOffset);
                bw.Write(GS.FieldCount);
            }

            //Field Array
            bw.Seek(g.FieldOffset, SeekOrigin.Begin);
            foreach (GFF.Field GF in g.Field_Array)
            {
                bw.Write(GF.Type);
                bw.Write(GF.LabelIndex);
                bw.Write(GF.DataOrDataOffset);
            }

            //Label Array
            bw.Seek(g.LabelOffset, SeekOrigin.Begin);
            foreach (string l in g.Label_Array)
            {
                bw.Write(l.PadRight(16, '\0').ToArray());
            }

            //Field Data Block
            foreach (GFF.Field GF in g.Field_Array)
            {
                if (GF.Complex)
                {
                    bw.Seek(g.FieldDataOffset + GF.DataOrDataOffset, SeekOrigin.Begin);
                    switch (GF.Type)
                    {
                        case 6:
                            bw.Write((ulong)GF.Field_Data);
                            break;
                        case 7:
                            bw.Write((long)GF.Field_Data);
                            break;
                        case 9:
                            bw.Write((double)GF.Field_Data);
                            break;
                        case 10:
                            bw.Write((GF.Field_Data as GFF.CExoString).Size);
                            bw.Write((GF.Field_Data as GFF.CExoString).Text.ToArray());
                            break;
                        case 11:
                            bw.Write((GF.Field_Data as GFF.CResRef).Size);
                            bw.Write((GF.Field_Data as GFF.CResRef).Text.ToArray());
                            break;
                        case 12:
                            bw.Write((GF.Field_Data as GFF.CExoLocString).Total_Size);
                            bw.Write((GF.Field_Data as GFF.CExoLocString).StringRef);
                            bw.Write((GF.Field_Data as GFF.CExoLocString).StringCount);
                            foreach (GFF.CExoLocString.SubString SS in (GF.Field_Data as GFF.CExoLocString).SubStringList)
                            {
                                bw.Write(SS.StringID);
                                bw.Write(SS.StringLength);
                                bw.Write(SS.Text.ToArray());
                            }
                            break;
                        case 13:
                            bw.Write((GF.Field_Data as GFF.Void_Binary).Size);
                            bw.Write((GF.Field_Data as GFF.Void_Binary).Data);
                            break;
                        default:
                            break;
                    }    
                }
            }

            //Field Indices Array
            // I did this the hard way initially, SAVE THIS CODE FOR FUTURE CONSTRUCTOR
            //IEnumerable<GFF.GFFStruct> IndexingStructs = from strc in g.Struct_Array where strc.FieldCount > 1 select strc;
            //foreach (GFF.GFFStruct GS in IndexingStructs)
            //{
            //    bw.Seek(g.FieldIndicesOffset + GS.DataOrDataOffset, SeekOrigin.Begin);
            //    for (int i = 0; i < GS.FieldCount; i++) { bw.Write(g.Field_Array.IndexOf((GS.StructData as List<GFF.Field>)[i])); }
            //}
            bw.Seek(g.FieldIndicesOffset, SeekOrigin.Begin);
            foreach (int i in g.Field_Indices) { bw.Write(i); }

            //List Indices Array
            bw.Seek(g.ListIndicesOffset, SeekOrigin.Begin);
            foreach (GFF.List_Index LI in g.List_Indices)
            {
                bw.Write(LI.Size);
                foreach (int i in LI.Indices) { bw.Write(i); }
            }

            bw.Close();
        }

        /// <summary>
        /// Writes Bioware Key File data
        /// </summary>
        /// <param name="k">The Key File to be written</param>
        /// <param name="s">The Stream to which the File will be written</param>
        public static void Write(KEY k, Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);

            //Header
            bw.Write(k.FileType.ToArray());
            bw.Write(k.Version.ToArray());
            bw.Write(k.BIFCount);
            bw.Write(k.KeyCount);
            bw.Write(k.OffsetToFileTable);
            bw.Write(k.OffsetToKeyTable);
            bw.Write(k.Build_Year);
            bw.Write(k.Build_Day);
            bw.Write(k.reserved);

            //File Table
            bw.Seek(k.OffsetToFileTable, SeekOrigin.Begin);
            foreach (KEY.File_Entry FE in k.File_Table)
            {
                bw.Write(FE.FileSize);
                bw.Write(FE.FilenameOffset);
                bw.Write(FE.FilenameSize);
                bw.Write(FE.Drives);
            }

            //Filenames
            foreach (KEY.File_Entry FE in k.File_Table)
            {
                bw.Seek(FE.FilenameOffset, SeekOrigin.Begin);
                bw.Write(FE.Filename.ToArray());
            }

            //Key Table
            bw.Seek(k.OffsetToKeyTable, SeekOrigin.Begin);
            foreach (KEY.Key_Entry KE in k.Key_Table)
            {
                bw.Write(KE.ResRef.PadRight(16, '\0').ToArray());
                bw.Write(KE.ResourceType);
                bw.Write(KE.ResID);
            }

            bw.Close();
        }

        /// <summary>
        /// Writes Bioware 'RIM' data
        /// </summary>
        /// <param name="r">The 'RIM' file to be written</param>
        /// <param name="s">The Stream to which the File will be written</param>
        public static void Write(RIM r, Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);

            //Header
            bw.Write(r.FileType.ToArray());
            bw.Write(r.Version.ToArray());
            bw.Write(r.Unknown);
            bw.Write(r.FileCount);
            bw.Write(r.File_Table_Offset);
            bw.Write(r.IsExtension);

            bw.Write(r.Reserved);

            //File Table
            bw.Seek(r.File_Table_Offset, SeekOrigin.Begin);
            foreach (RIM.rFile RF in r.File_Table)
            {
                bw.Write(RF.Label.PadRight(16, '\0').ToArray());
                bw.Write(RF.TypeID);
                bw.Write(RF.Index);
                bw.Write(RF.DataOffset);
                bw.Write(RF.DataSize);
            }

            //File Data
            foreach (RIM.rFile RF in r.File_Table)
            {
                bw.Seek(RF.DataOffset, SeekOrigin.Begin);
                bw.Write(RF.File_Data);
            }

            //Padding the end with 6 bytes because kotor likes that for some reason.
            bw.Seek(0, SeekOrigin.End);
            byte[] padend = new byte[6] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
            bw.Write(padend);

            bw.Close();
        }

        /// <summary>
        /// Writes Bioware Soundset file (v1.1) data
        /// </summary>
        /// <param name="f">The Soundset file to be written</param>
        /// <param name="s">The Stream to which the File will be written</param>
        public static void Write(SSF f, Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);

            //Header
            bw.Write(f.FileType.ToArray());
            bw.Write(f.Version.ToArray());
            bw.Write(f.UnknownInt);

            //string refs
            if (f.TLKPopulated)
            {
                foreach (object o in f.StringTable.Values)
                {
                    bw.Write((o as SSF.Sound).SRef);
                }
            }
            else
            {
                foreach (object o in f.StringTable.Values)
                {
                    bw.Write((int)o);
                }
            }

            //EndPadding
            bw.Write(f.EndPadding);

            bw.Close();
        }

        /// <summary>
        /// Writes Bioware Talk Table data
        /// </summary>
        /// <param name="t">The Talk Table to be written</param>
        /// <param name="s">The Stream to which the File will be written</param>
        public static void Write(TLK t, Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);

            //Header
            bw.Write(t.FileType.ToArray());
            bw.Write(t.Version.ToArray());
            bw.Write(t.LanguageID);
            bw.Write(t.StringCount);
            bw.Write(t.StringEntriesOffset);

            //string data table
            foreach (TLK.String_Data SD in t.String_Data_Table)
            {
                bw.Write(SD.Flags);
                bw.Write(SD.SoundResRef.PadRight(16, '\0').ToArray());
                bw.Write(SD.VolumeVariance);
                bw.Write(SD.PitchVariance);
                bw.Write(SD.OffsetToString);
                bw.Write(SD.StringSize);
                bw.Write(SD.SoundLength);
            }

            //string text
            foreach(TLK.String_Data SD in t.String_Data_Table)
            {
                bw.Seek(t.StringEntriesOffset + SD.OffsetToString, SeekOrigin.Begin);

                bw.Write(SD.StringText.ToArray());
            }

            bw.Close();
        }

        /// <summary>
        /// Writes a miscellaneous KotOR source file data
        /// </summary>
        /// <param name="m">The Miscellaneous KotOR source file to be written</param>
        /// <param name="s">The Stream to which the File will be written</param>
        public static void Write(MiscType m, Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);

            //Data
            bw.Write(m.data);

            bw.Close();
        }
    }


    /// <summary>
    /// Generic Form of a KotOR source file.
    /// <para/>Serves as a super class to:
    /// <see cref="TwoDA"/>, 
    /// <see cref="BIF"/>, 
    /// <see cref="ERF"/>, 
    /// <see cref="GFF"/>, 
    /// <see cref="KEY"/>, 
    /// <see cref="RIM"/>, 
    /// <see cref="SSF"/>, 
    /// <see cref="TLK"/>, and
    /// <see cref="MiscType"/>
    /// </summary>
    public abstract class KFile
    {
        /// <summary>The 4 char file type</summary>
        public string FileType;
        /// <summary>The 4 char file version</summary>
        public string Version;
    }


    /// <summary>
    /// <para>Bioware 2-Dimensional Array Data</para>
    /// See: 
    /// <see cref="KReader.Read2DA(Stream)"/>
    /// <seealso cref="kWriter.Write(TwoDA, Stream)"/>
    /// <para/>
    /// <remarks>
    /// 2DA data is generally presented in a spreadsheet format. It is used by the game engine to reference various values and constants for task ranging from name generation, to item properties.
    /// *NOTE: This program is currently only compatible with 2DA version 2.b files.
    /// </remarks>
    /// </summary>
    public class TwoDA: KFile
    {
        #region class definition
        //header
        //FileType & Version in superclass
        /// <summary>List of Column Headers. Generally used as the keys for Data</summary>
        public List<string> Columns = new List<string>();

        /// <summary>The Number of rows in the array</summary>
        public int Row_Count;

        /// <summary>A list of data offsets, one for each cell of the array</summary>
        public List<short> Offsets = new List<short>();

        /// <summary>The Full 2D-Array with collumns for keys, rows for values that each indexe from 0 to row_count - 1. 
        /// The first collumn with <c>"row_indexs"</c> is an index of each row.</summary>
        public Dictionary<string, List<object>> Data = new Dictionary<string, List<object>>();

        /// <summary>Denotes rather or not the default string data has been parsed to numerical data where appropriate.</summary>
        public bool IsParsed;

        /// <summary>Parses the default string data into either <see cref="int"/>, <see cref="float"/>, hex data, or <see cref="string"/> depending on each column's contents.</summary>
        public void ParseData()
        {
            if (!IsParsed)
            {
                int IScrap = 0;
                float IFcrap = 0;
                foreach (List<object> column in Data.Values)
                {
                    int i = 0;
                    while (column[i] as string == "") { i++; if (i >= column.Count) { break; } } //iterate to the first non null value
                    if (i >= column.Count) { for (int k = 0; k < column.Count; k++) { if (column[k] as string == "") { column[k] = null; } } continue; }
                    bool IntColumn = Int32.TryParse(column[i] as string, out IScrap);
                    bool FloatColumn = Single.TryParse(column[i] as string, out IFcrap);
                    bool HexColumn = Int32.TryParse((column[i] as string).TrimStart('0', 'x'), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out IScrap);

                    if (IntColumn) { for (int k = 0; k < column.Count; k++) { if (column[k] as string != "") { column[k] = Convert.ToInt32(column[k]); } else { column[k] = null; } } }
                    else if (FloatColumn) { for (int k = 0; k < column.Count; k++) { if (column[k] as string != "") { column[k] = Convert.ToSingle(column[k]); } else { column[k] = null; } } }
                    else if (HexColumn) { for (int k = 0; k < column.Count; k++) { if (column[k] as string != "") { column[k] = Int32.Parse((column[k] as string).TrimStart('0', 'x'), NumberStyles.HexNumber); } else { column[k] = null; } } }
                    else { for (int k = 0; k < column.Count; k++) { if (column[k] as string == "") { column[k] = null; } } }
                }
            }
            IsParsed = true;
        }
        #endregion

        #region Construction
        ///<summary>Initiates a new instance of the <see cref="TwoDA"/> class.</summary>
        public TwoDA() { }

        /// <summary>
        /// Initiates a new instance of the <see cref="TwoDA"/> class, given column names and 2-deminsional data.
        /// </summary>
        /// <param name="_Columns">A string array containing the names of the columns</param>
        /// <param name="_Data">The 2-dimensional data</param>
        /// <param name="_IsParsed">Whether the data has been parsed into different formats. If False, then all data is in string format.</param>
        public TwoDA(string[] _Columns, object[,] _Data, bool _IsParsed)
        {
            if (_Columns.Length != _Data.GetLength(1)) { throw new IndexOutOfRangeException("Length of columns should be equal to the number of columns in data.");  }

            //header info
            FileType = "2DA ";
            Version = "V2.b";

            // Columns
            Columns.AddRange(_Columns);

            //row count
            Row_Count = _Data.GetLength(0);

            //Offsets
            List<object> UniqueValues = new List<object>();
            List<int> IndexOffsets = new List<int>();

            int totaloffset = 0;
            foreach (object o in _Data)
            {
                if (!UniqueValues.Contains(o))
                {
                    UniqueValues.Add(o);
                    IndexOffsets.Add(totaloffset);
                    totaloffset += GetDataSize(o) + 1;
                }

                Offsets.Add((short)(IndexOffsets[UniqueValues.IndexOf(o)]));
            }
            Offsets.Add((short)totaloffset);
            //Data

            //generate index column
            List<object> index_list = new List<object>();
            for (int i = 0; i < Row_Count; i++) { index_list.Add(Convert.ToString(i)); }
            Data.Add("row_index", index_list);

            foreach (string c in _Columns)
            {
                List<object> tempCol = new List<object>();
                int colIndex = Array.IndexOf(_Columns, c);

                for (int i = 0; i < Row_Count; i++)
                {
                    tempCol.Add(_Data[i, colIndex]);
                }

                Data.Add(c, tempCol);
            }

            //Parsing
            IsParsed = _IsParsed;

        }
        
        int GetDataSize(object o)
        {
            if (o is string)
            {
                return (o as string).Length;
            }
            else if (o is int)
            {
                return Convert.ToString((int)o).Length;
            }
            else if (o is float)
            {
                return Convert.ToString((float)o).Length;
            }
            else
            {
                return Convert.ToString(o).Length;
            }
        }

        /// <summary>
        /// Indexer for 2DA data
        /// </summary>
        /// <param name="Column_Label">The label of the column in the <see cref="TwoDA"/>.</param>
        /// <param name="Row_Index">The index of the row in <see cref="Data"/>.</param>
        public object this[string Column_Label, int Row_Index] //maybe switch first vaule to column
        {
            get
            {
                if (Data.Keys.Contains(Column_Label) && Row_Index < Row_Count)
                {
                    return Data[Column_Label][Row_Index];
                }
                else
                {
                    throw new IndexOutOfRangeException("Column Label and row index must exist in the 2DA."); 
                }
            }
            set
            {
                if (Data.Keys.Contains(Column_Label) && Row_Index < Row_Count)
                {
                    short _Offset = Offsets[Columns.IndexOf(Column_Label) + Row_Index * Columns.Count];
                    object oldvalue = Data[Column_Label][Row_Index];
                    Data[Column_Label][Row_Index] = value;
                    int offset_difference = Convert.ToString(value).Length - Convert.ToString(oldvalue).Length;

                    for(int i = 0; i < Offsets.Count; i++)
                    {
                        if (Offsets[i] > _Offset)
                        {
                            Offsets[i] += (short)offset_difference;
                        }
                    }

                }
                else
                {
                    throw new IndexOutOfRangeException("Column Label and row index must exist in the 2DA.");
                }
            }
        }

        /// <summary>
        /// Adds a new collumn onto <see cref="Data"/>
        /// </summary>
        /// <param name="_Label">The Label or Header for the collumn</param>
        /// <param name="_Data">The list of objects to be seeded in the collumn</param>
        public void Add_Column(string _Label, object[] _Data)
        {
            if (_Data.Length > Row_Count) { throw new IndexOutOfRangeException("_Data extends beyond Row_Count"); }
            List<object> tmpCol = new List<object>(_Data);
            Data.Add(_Label, tmpCol);
            Columns.Add(_Label);
            Offsets.Clear();

            //Offsets
            List<object> UniqueValues = new List<object>();
            List<int> IndexOffsets = new List<int>();

            int totaloffset = 0;
            for (int row = 0; row < Data["row_index"].Count; row++)
            {
                bool indexColSkipped = false;
                foreach (List<object> col in Data.Values)
                {
                    if (!indexColSkipped) { indexColSkipped = true; continue; }
                    if (!UniqueValues.Contains(col[row]))
                    {
                        UniqueValues.Add(col[row]);
                        IndexOffsets.Add(totaloffset);
                        totaloffset += GetDataSize(col[row]) + 1;
                    }

                    Offsets.Add((short)(IndexOffsets[UniqueValues.IndexOf(col[row])]));
                }
            }
            Offsets.Add((short)totaloffset);


        }

        /// <summary>
        /// Adds a row to the 2DA
        /// </summary>
        /// <param name="_Data">The Data to be seeded to that row (from left to right).</param>
        public void Add_Row(object[] _Data)
        {
            if (_Data.Length > Columns.Count) { throw new IndexOutOfRangeException("_Data contains more collumns than are present in the 2DA."); }
            Data["row_index"].Add(Row_Count);
            Row_Count++;
            int colIndex = 0;
            foreach (object o in _Data)
            {
                Data[Columns[colIndex]].Add(o);
                colIndex++;
            }
            Offsets.Clear();

            //Offsets
            List<object> UniqueValues = new List<object>();
            List<int> IndexOffsets = new List<int>();

            int totaloffset = 0;
            for (int row = 0; row < Data["row_index"].Count; row++)
            {
                bool indexColSkipped = false;
                foreach (List<object> col in Data.Values)
                {
                    if (!indexColSkipped) { indexColSkipped = true; continue; }
                    if (!UniqueValues.Contains(col[row]))
                    {
                        UniqueValues.Add(col[row]);
                        IndexOffsets.Add(totaloffset);
                        totaloffset += GetDataSize(col[row]) + 1;
                    }

                    Offsets.Add((short)(IndexOffsets[UniqueValues.IndexOf(col[row])]));
                }
            }
            Offsets.Add((short)totaloffset);

        }
        #endregion

    }

    /// <summary>
    /// Bioware Built In File Data. <para/>
    /// See: 
    /// <see cref="KReader.ReadBIF(Stream)"/>, 
    /// <seealso cref="kWriter.Write(BIF, Stream)"/>, 
    /// <seealso cref="attachKey(KEY, string)"/>, 
    /// <remarks>
    /// <para/>BIF data stores the bulk of the template files for the game. Items, characters, models, sound data, scripts and many other things that will appear in multiple different location in game find there place in BIFs. 
    /// <para/>BIF data, though, is rather useless without a Key file to reference all of the data stored within. Which is the purpose of the AttachKey Method.
    /// </remarks>
    /// </summary>
    public class BIF: KFile
    {
        ///<summary>The name of the of the BIF file in the form of a path from the key directory. Used exclusively for Key attachment.</summary>
        public string name;

        //Header Data
        //FileType & Version in superclass
        ///<summary>The Number of Variable resources contained within the BIF</summary>
        public int Variable_Resource_Count;
        ///<summary>The Number of Fixed Resources in the BIF *NOTE: This is not used in KotOR, though may be present in other Aurora Games</summary>
        public int Fixed_Resource_Count;
        ///<summary>The Offset to the variable resource table as bytes from the beginning of teh file. (This value is usually 20)</summary>
        public int Variable_Table_Offset;

        //Variable Resource Table
        ///<summary>One of the elements that makes up the Variable Resource Table. Contains basic meta data for each resource a resource in the BIF.</summary>
        public class Var_Res_Entry
        {
            ///<summary>
            ///<para>A unique ID number for this resource.</para>
            ///<para>It Contains two parts (x and y) which denote Resource index, as well as combatibility with Key files.</para>
            /// </summary>
            public int ID;
            ///<summary>Byte offset from the start of teh BIF to the data for this resource.</summary>
            public int Offset;
            ///<summary>The size in bytes of this resource</summary>
            public int File_Size;
            ///<summary>An integer representing the file type of this resource. See: 
            ///<see cref="Reference_Tables.Res_Types"/></summary>
            public int Resource_Type_code;
            ///<summary>A string representing the file extension coresponding to the resource file type. 
            ///This value is obtianed from <see cref="Reference_Tables.Res_Types"/>.</summary>
            public string Resource_Type_text;

            ///<summary>
            ///The X component of the ID. This is rather useless in BIF files, however is included for consitancy with Key files.
            ///<para>It is calculated by bit shifting ID right by 20. (ID &gt;&gt; 20)</para>
            /// </summary>
            public int IDx;
            ///<summary>
            ///The Y component of the ID. This denotes an index of this resource entry in the BIF. This value will match the repective ID in this BIF's Key file.
            ///<para>This is calculated by subtracting the X component bit shifted left 20 from the ID. (ID - (x &lt;&lt; 20))</para>
            ///</summary>
            public int IDy;

            ///<summary>The raw byte data of this resource.</summary>
            public byte[] Entry_Data;

            ///<summary>Resource Reference string (aka filename) of this resource. This is populated when Attaching Key data. </summary>
            public string ResRef;
        }
        ///<summary>The Table Containing all of the Variable Resource Data for this BIF. This is the main object of importance in a BIF, containing the actual functional content of the file.</summary>
        public List<Var_Res_Entry> Variable_Resource_Table = new List<Var_Res_Entry>();

        //Fixed Resource Table
        ///<summary>Fixed Variable Resource. *NOTE: Fixed Resources are not used by KotOR, therefore no further documentation will be provided.</summary>
        public class Fixed_Res_Entry
        {
            ///<summary></summary>
            public int ID;
            ///<summary></summary>
            public int Offset;
            ///<summary></summary>
            public int PartCount;
            ///<summary></summary>
            public int File_Size;
            ///<summary></summary>
            public int Resource_Type_code;
            ///<summary></summary>
            public string Resource_Type_text;
            ///<summary></summary>

            ///<summary></summary>
            public int IDx;
            ///<summary></summary>
            public int IDy;

            ///<summary></summary>
            public byte[] Entry_Data;
        }
        ///<summary>The Table Containing all of the Fixed Variable Resources. *NOTE: Fixed Resources are not used by KotOR</summary>
        public List<Fixed_Res_Entry> Fixed_Resource_Table = new List<Fixed_Res_Entry>();

        /// <summary>
        /// Takes data from a given KEY class and uses it to populate the data in the BIF file. 
        /// </summary>
        /// <param name="k">The KEY to be attached. (Usually chitin.key for KotOR)</param>
        /// <param name="Filename">
        /// The filename for this BIF file which will be used to index its ID from the KEY. 
        /// <para>This is given in the form of a path from the KEY's directory.</para>
        /// <para>For Example: if the BIF file 'file' is located in a directory called 'data' (like KotOR BIFs), then the Filename would be "data\\file.bif"</para>
        /// </param>
        public void attachKey(KEY k, string Filename)
        {
            name = Filename;

            //Get index the bif file
            int xIndex = 0;
            foreach (KEY.File_Entry FE in k.File_Table)
            {
                if (FE.Filename.Trim('\0') == Filename)
                {
                    break;
                }
                else
                {
                    xIndex++;
                }
            }

            //parse the list down to just the section the bif cares about
            List<KEY.Key_Entry> BiffSection = new List<KEY.Key_Entry>();
            foreach (KEY.Key_Entry KE in k.Key_Table)
            {
                if (KE.IDx == xIndex)
                {
                    BiffSection.Add(KE);
                }
            }

            //Compare each Var_res_entry to each Key_entry to match up IDy and assign its ResRef
            foreach(Var_Res_Entry VRE in Variable_Resource_Table)
            {
                foreach (KEY.Key_Entry KE in BiffSection)
                {
                    if (VRE.IDy == KE.IDy)
                    {
                        VRE.ResRef = KE.ResRef;
                    }
                }
            }
        }

        ///<summary>Initiates a new instance of the <see cref="BIF"/> class.</summary>
        public BIF()
        {

        }
    }

    /// <summary>
    /// Bioware Encapsulated Resource File Data. <para/> See: 
    /// <see cref="KReader.ReadERF(Stream)"/>
    /// <seealso cref="kWriter.Write(ERF, Stream)"/>
    /// <remarks>
    /// <para/>ERF files often come in the form of Save games (.SAV), active modules (.MOD), texture packs (.ERF), as well as hack-pack (.HAK)
    /// <para/>They simply store data with Key references for quick reading and writing by the game.
    /// </remarks>
    /// </summary>
    public class ERF: KFile
    {
        #region Class Definition
        //Header
        //FileType & Version in superclass
        ///<summary>The number of strings in the Localized String List</summary>
        public int LanguageCount;
        ///<summary>The Total size (bytes) of Localized String List</summary>
        public int LocalizedStringSize;
        ///<summary>The number of files packed into the ERF</summary>
        public int EntryCount;
        ///<summary>The byte offset from the start of the file to the Localized String List</summary>
        public int OffsetToLocalizedStrings;
        ///<summary>The byte offset from the start of the file to the Key List</summary>
        public int OffsetToKeyList;
        ///<summary>The byte offset from the start of the file to the Resource List</summary>
        public int OffsetToResourceList;
        ///<summary>The number of years after 1900 that the ERF file was built. (i.e. 2019 == 119)</summary>
        public int BuildYear;
        ///<summary>The number of days after January 1st the ERF file was built. (i.e. October 5th == 277)</summary>
        public int BuildDay;
        ///<summary>A numerical string reference to a talk table (<see cref="TLK"/>) for the file description if one exist.</summary>
        public int DescriptionStrRef;
        ///<summary>A block of 116 (usually null) bytes that are reserved for future backwards compatibility.</summary>
        public byte[] Reserved_block = new byte[116];

        //Localized String List
        ///<summary>A localized string element in the Localized String list.</summary>
        public class String_List_Element
        {
            ///<summary>The ID that represents what language this entry is in.</summary>
            public int LanguageID;
            ///<summary>The language that LanguageID references according to <see cref="Reference_Tables.Language_IDs"/></summary>
            public string Language;
            ///<summary>The size of the string in chars/bytes</summary>
            public int StringSize;
            ///<summary>The localized string. Populated by a char array of size StringSize</summary>
            public string String;
        }
        ///<summary>List of localized strings. Used of descriptive content for the ERF file. (Not always present)</summary>
        public List<String_List_Element> Localized_String_List = new List<String_List_Element>();

        // Key List
        ///<summary>The Key that contains the filename, index, and file type of a resource.</summary>
        public class Key
        {
            ///<summary>The Resource Reference (aka Filename). Max of 16 chars long.</summary>
            public string ResRef;
            ///<summary>The Resource ID. This is a 0-based index into the Resource List</summary>
            public int ResID;
            ///<summary>The File type ID. <para/>See: 
            ///<see cref="Reference_Tables.Res_Types"/></summary>
            public short ResType;
            ///<summary>The File Type extension populated from <see cref="Reference_Tables.Res_Types"/></summary>
            public string Type_string;
            ///<summary>An unused 16-bit integer (usually null) present in every ERF Key.</summary>
            public short Unused;
        }
        ///<summary>The List containing all of the Resource Reference Keys for the files in this ERF. (Used for populated Filenames and Types)</summary>
        public List<Key> Key_List = new List<Key>();

        //Resource List
        ///<summary>Contains the byte offset and size of each file in the ERF</summary>
        public class Resource
        {
            ///<summary>The offset from begining of ERF file (usually into Resource data section) to the data for this resource.</summary>
            public int OffsetToResource;
            ///<summary>The size of the resource (file) in bytes</summary>
            public int ResourceSize;

            ///<summary>The raw byte data for the resource (file)</summary>
            public byte[] Resource_data;
        }
        ///<summary>The list containing all of the resources. This contians all of the data for the files within this ERF</summary>
        public List<Resource> Resource_List = new List<Resource>();
        #endregion

        #region Construction
        ///<summary>Initializes a new instance of the <see cref="ERF"/> class.</summary>
        public ERF() { }

        /// <summary>The 4 different ERF types</summary>
        public enum ERF_Type
        {
            ///<summary>The Generical ERF type (commonly used for texture packs)</summary>
            ERF,
            ///<summary>The Dynamic Module Form. (usually nested inside of save files, Sometimes given the extension '.sav' in the game instance) </summary>
            MOD,
            ///<summary>The Save File Form. (Used by save files, and current game instances)</summary>
            SAV,
            ///<summary>Hack-Pack Form. (Used by Hack Packs, or patches. Not present in the PC version, though used in the mobile version to path a few interface changes)</summary>
            HAK
        }
        string[] etypes = new string[4] { "ERF ", "MOD ", "SAV ", "HAK " };

        /// <summary>
        /// Constructs an instance of teh <see cref="ERF"/> class containing the given files.
        /// </summary>
        /// <param name="type">The <see cref="ERF_Type"/> being built.</param>
        /// <param name="files">The KotOR source files to be packed into the ERF</param>
        /// <param name="filenames">The Names of the files. Indices must match that of 'files'. These filenames are case sensitive and should match the resource references of any included gff files.</param>
        public ERF(ERF_Type type, KFile[] files, string[] filenames)
        {
            //header
            FileType = etypes[(int)type];
            Version = "V1.0";
            LanguageCount = 0;
            LocalizedStringSize = 0;
            EntryCount = files.Count();
            OffsetToLocalizedStrings = 160;
            OffsetToKeyList = OffsetToLocalizedStrings + LocalizedStringSize; //future proofing
            OffsetToResourceList = OffsetToKeyList + (EntryCount * 24); //Every key is 24 bytes 
            BuildYear = DateTime.Now.Year - 1900;
            BuildDay = DateTime.Now.DayOfYear - 1;
            DescriptionStrRef = 0;

            //Key List
            int rid = 0; //Resource ID iterable
            foreach (KFile f in files)
            {
                Key k = new Key();

                k.ResRef = filenames[Array.IndexOf(files, f)];
                k.ResID = rid;
                k.ResType = (short)Reference_Tables.TypeCodes[f.FileType];
                k.Unused = 0;
                k.Type_string = Reference_Tables.Res_Types[k.ResType];
                Key_List.Add(k);

                rid++;
            }

            //Res List
            int TotalOffset = OffsetToResourceList + EntryCount * 8;
            foreach (KFile f in files)
            {
                Resource r = new Resource();
                MemoryStream ms = new MemoryStream();

                if (f is TwoDA) { kWriter.Write(f as TwoDA, ms); }
                else if (f is BIF) { kWriter.Write(f as BIF, ms); }
                else if (f is ERF) { kWriter.Write(f as ERF, ms); }
                else if (f is GFF) { kWriter.Write(f as GFF, ms); }
                else if (f is KEY) { kWriter.Write(f as KEY, ms); }
                else if (f is RIM) { kWriter.Write(f as RIM, ms); }
                else if (f is SSF) { kWriter.Write(f as SSF, ms); }
                else if (f is TLK) { kWriter.Write(f as TLK, ms); }
                else { kWriter.Write(f as MiscType, ms); }

                r.Resource_data = ms.ToArray();
                r.ResourceSize = r.Resource_data.Length;
                r.OffsetToResource = TotalOffset;

                Resource_List.Add(r);

                TotalOffset += r.ResourceSize;
            }
        }

        /// <summary>
        /// Adds another resource to the <see cref="ERF"/>.
        /// </summary>
        /// <param name="res_ref">The name of the resource/file. *Maximum of 16 Characters</param>
        /// <param name="file_data">A byte array containing the data for the resource</param>
        public void Append_File(string res_ref, byte[] file_data)
        {
            //header
            EntryCount++;
            OffsetToResourceList += 24;

            //key
            Key k = new Key();
            k.ResRef = res_ref;
            k.ResID = Key_List.Last().ResID + 1;
            StringBuilder sb = new StringBuilder(4);
            sb.Append(new char[4] { (char)file_data[0], (char)file_data[1], (char)file_data[2], (char)file_data[3], });
            k.ResType = (short)Reference_Tables.TypeCodes[sb.ToString()];
            k.Type_string = k.Type_string = Reference_Tables.Res_Types[k.ResType];
            k.Unused = 0;
            Key_List.Add(k);

            //Offset Correction
            int TotalOffset = OffsetToResourceList + EntryCount * 8;
            foreach (Resource res in Resource_List)
            {
                res.OffsetToResource = TotalOffset;
                TotalOffset += res.ResourceSize;
            }

            //resource
            Resource r = new Resource();
            r.Resource_data = file_data;
            r.ResourceSize = file_data.Length;
            r.OffsetToResource = Resource_List.Last().OffsetToResource + Resource_List.Last().ResourceSize;
            Resource_List.Add(r);

        }

        /// <summary>
        /// Adds another resource to the <see cref="ERF"/>.
        /// </summary>
        /// <param name="file">A Kotor Source File</param>
        /// <param name="filename">The name of the <see cref="KFile"/></param>
        public void Append_File(KFile file, string filename)
        {
            MemoryStream ms = new MemoryStream();

            if (file is TwoDA) { kWriter.Write(file as TwoDA, ms); }
            else if (file is BIF) { kWriter.Write(file as BIF, ms); }
            else if (file is ERF) { kWriter.Write(file as ERF, ms); }
            else if (file is GFF) { kWriter.Write(file as GFF, ms); }
            else if (file is KEY) { kWriter.Write(file as KEY, ms); }
            else if (file is RIM) { kWriter.Write(file as RIM, ms); }
            else if (file is SSF) { kWriter.Write(file as SSF, ms); }
            else if (file is TLK) { kWriter.Write(file as TLK, ms); }
            else { kWriter.Write(file as MiscType, ms); }

            Append_File(filename, ms.ToArray());
        }

        /// <summary>
        /// Gets the <see cref="byte"/> data for the specified resource, given the resource reference (filename)
        /// </summary>
        /// <param name="filename">The string resource reference of the file.</param>
        /// <returns></returns>
        public byte[] this[string filename]
        {
            get
            {
                return Resource_List[(from k in Key_List where k.ResRef == filename select k.ResID).First()].Resource_data;
            }
        }

        /// <summary>
        /// Gets the <see cref="byte"/> data for the specified resource, given the index of the resource
        /// </summary>
        /// <param name="index">The index of the resource in the resource array.</param>
        /// <returns></returns>
        public byte[] this[int index]
        {
            get
            {
                return Resource_List[index].Resource_data;
            }
        }

        #endregion
    }

    /// <summary>
    /// Bioware General File Format Data. <para/>See: 
    /// <see cref="KReader.ReadGFF(Stream)"/>, 
    /// <seealso cref="kWriter.Write(GFF, Stream)"/>
    /// <remarks>
    /// <para/>The GFF file is the sort of 'Catch-all' format in the Aurora Engine. It is used to represent nearly every object in the game, from items (.UTI) to static area info (.GIT).
    /// </remarks>
    /// </summary>
    public class GFF : KFile
    {
        //header
        //FileType & Version in superclass
        ///<summary>Offset of Struct array as bytes from the beginning of the file</summary>
        public int StructOffset;
        ///<summary>Number of elements in Struct array</summary>
        public int StructCount;
        ///<summary>Offset of Field array as bytes from the beginning of the file</summary>
        public int FieldOffset;
        ///<summary>Number of elements in Field array</summary>
        public int FieldCount;
        ///<summary>Offset of Label array as bytes from the beginning of the file</summary>
        public int LabelOffset;
        ///<summary>Number of elements in Label array</summary>
        public int LabelCount;
        ///<summary>Offset of Field Data as bytes from the beginning of the file</summary>
        public int FieldDataOffset;
        ///<summary>Number of bytes in Field Data block</summary>
        public int FieldDataCount;
        ///<summary>Offset of Field Indices array as bytes from the beginning of the file</summary>
        public int FieldIndicesOffset;
        ///<summary>Number of bytes in Field Indices array</summary>
        public int FieldIndicesCount;
        ///<summary>Offset of List Indices array as bytes from the beginning of the file</summary>
        public int ListIndicesOffset;
        ///<summary>Number of bytes in List Indices array</summary>
        public int ListIndicesCount;

        //Struct Array
        ///<summary>A GFF object that holds a set of <see cref="Field"/>s, each having there own type and data.</summary>
        public class GFFStruct
        {
            ///<summary>Programmer-defined integer ID for the struct type. Varies from from File to file, though the Top-level struct (0) always has a type equal to 0xFFFFFFFF</summary>
            public int Type;
            ///<summary>
            ///<para>If FieldCount = 1, this is an index into the Field Array.</para>
            ///<para>If FieldCount > 1, this is a byte offset into the Field Indices array</para>
            /// </summary>
            public int DataOrDataOffset;
            ///<summary>Number of fields in this Struct</summary>
            public int FieldCount;
            ///<summary>The data stored in this Struct, populated from DataOrDataOffset. Usually takes the form of a <see cref="Field"/> or <see cref="List{Field}"/></summary>
            public object StructData;
        }
        /// <summary>The array of all of teh GFF Structs stored in this file, this is where the bulk of the data will be stored</summary>
        public List<GFFStruct> Struct_Array = new List<GFFStruct>();

        //Field Array
        /// <summary>A field is essentially a property or variable value stored within structs.</summary>
        public class Field
        {
            /// <summary>Data type. See: 
            /// <see cref="Reference_Tables.Field_Types"/>
            /// </summary>
            public int Type;
            /// <summary>Index into the Label Array</summary>
            public int LabelIndex;
            /// <summary>
            /// <para>If the field data type is not listed as complex, this is the actual value of the field</para>
            /// <para>If the field data type is listed as complex, this is an offset into another data block.</para>
            /// See:
            /// <see cref="Reference_Tables.Complex_Field_Types"/>,
            /// <see cref="Reference_Tables.Field_Types"/>
            /// </summary>
            public int DataOrDataOffset;

            /// <summary>
            /// This text is taken from <see cref="LabelIndex"/> in the <seealso cref="Label_Array"/>.
            /// </summary>
            public string Label;

            /// <summary>The actual data in the field of the type associated with <see cref="Type"/>, and populated from <see cref="DataOrDataOffset"/></summary>
            public object Field_Data;

            /// <summary>The string representation of <see cref="Type"/> from <see cref="Reference_Tables.Field_Types"/></summary>
            public string Type_Text;
            /// <summary>Whether or not the <see cref="Type"/> is complex according to <see cref="Reference_Tables.Complex_Field_Types"/> </summary>
            public bool Complex;
        }
        /// <summary>The array of all fields contained withing the GFF</summary>
        public List<Field> Field_Array = new List<Field>();

        //Label Array
        /// <summary>The array of all Field labels (aka variable names) in the GFF</summary>
        public List<string> Label_Array = new List<string>();

        //Field_Indices
        /// <summary>A list of Index references used to assign fields to structs.</summary>
        public List<int> Field_Indices = new List<int>();

        //List Indices
        /// <summary>Describes a 'List' field as struct indexs prefixed with a 'size'</summary>
        public class List_Index
        {
            /// <summary>The number of <see cref="GFFStruct"/>s in the list</summary>
            public int Size;
            /// <summary>Index vaules representing which <see cref="GFFStruct"/>s from <see cref="Struct_Array"/> are present in the list.</summary>
            public List<int> Indices = new List<int>();
        }
        /// <summary>The array contianing all of the <see cref="List_Index"/> elements.</summary>
        public List<List_Index> List_Indices = new List<List_Index>();

        //Complex Data Types
        /// <summary>A chacrter string prefixed with a size.</summary>
        public class CExoString
        {
            /// <summary>The length of the string.</summary>
            public int Size;
            /// <summary>The string, obatined from a <see cref="char"/> array of length <see cref="Size"/></summary>
            public string Text; 
        }

        /// <summary>Virtually identical to <see cref="CExoString"/>, however it is capped at a size of 16.</summary>
        public class CResRef
        {
            /// <summary>The length of the string. *This can be no larger than 16</summary>
            public byte Size;
            /// <summary>The string, obatined from a <see cref="char"/> array of length <see cref="Size"/></summary>
            public string Text; //Obtained from a char[] of size 'Size'
        }

        /// <summary>A set of localized string that contains language data in addition to the content of <see cref="CExoString"/></summary>
        public class CExoLocString
        {
            /// <summary>Total number of bytes in the object, not including these.</summary>
            public int Total_Size;
            /// <summary>The reference of the string into a relevant Talk table (<see cref="TLK"/>). If this is -1 it does not reference a string.</summary>
            public int StringRef;
            /// <summary>The number of <see cref="SubString"/>s contained.</summary>
            public int StringCount;
            /// <summary>Nearly Identical to a <see cref="CExoString"/> be is prefixed by an ID.</summary>
            public class SubString
            {
                /// <summary>An identify that is calulated by multiplying the <see cref="Reference_Tables.Language_IDs"/> ID by 2, and adding 1 if the speaker if feminine.
                /// <para/>For example, a line spoken by an Italien Male would have an ID of 6
                /// </summary>
                public int StringID;
                /// <summary>The length of the string in characters</summary>
                public int StringLength;
                /// <summary>The string contained</summary>
                public string Text; //Obtained from a char[] of size 'StringLength'
            }
            /// <summary>The list containing the <see cref="SubString"/>s contained.</summary>
            public List<SubString> SubStringList = new List<SubString>();
        }

        /// <summary>A byte array prefixed with size.</summary>
        public class Void_Binary
        {
            /// <summary>The sive of teh void object in bytes</summary>
            public int Size;
            /// <summary>The raw byte data of the object, with a length of <see cref="Size"/></summary>
            public byte[] Data;
        }

        ///<summary>Initiates a new instance of the <see cref="GFF"/> class.</summary>
        public GFF() { }

        public GFF(byte[] raw_data)
        {
            MemoryStream ms = new MemoryStream(raw_data);
            GFF g = KReader.ReadGFF(ms);

            this.FieldCount = g.FieldCount;
            this.FieldDataCount = g.FieldDataCount;
            this.FieldDataOffset = g.FieldDataOffset;
            this.FieldIndicesCount = g.FieldIndicesCount;
            this.FieldIndicesOffset = g.FieldIndicesOffset;
            this.FieldOffset = g.FieldOffset;
            this.Field_Array = g.Field_Array;
            this.Field_Indices = g.Field_Indices;
            this.FileType = g.FileType;
            this.LabelCount = g.LabelCount;
            this.LabelOffset = g.LabelOffset;
            this.Label_Array = g.Label_Array;
            this.ListIndicesCount = g.ListIndicesCount;
            this.ListIndicesOffset = g.ListIndicesOffset;
            this.List_Indices = g.List_Indices;
            this.StructCount = g.StructCount;
            this.StructOffset = g.StructOffset;
            this.Struct_Array = g.Struct_Array;
            this.Version = g.Version;
        }
    }

    /// <summary>
    /// Bioware Key Data.<para/>See: 
    /// <see cref="KReader.ReadKEY(Stream)"/>, 
    /// <seealso cref="kWriter.Write(KEY, Stream)"/>
    /// <remarks>
    /// <para/>Key files contain a large array of references with IDs that refer to existiong BIF files. They serve as a sort of catalog for whenever the game needs to find a specific file. 
    /// <para/>The only, and most important, Key file in SW:KotOR is 'chitin.key' which is used to reference every single BIF file, and their contents.
    /// </remarks>
    /// </summary>
    public class KEY : KFile
    {
        //Header
        //FileType & Version in superclass
        /// <summary>The number of <see cref="BIF"/> files this key controls.</summary>
        public int BIFCount;
        /// <summary>The Number of resources in all of the <see cref="BIF"/>s linked to this key.</summary>
        public int KeyCount;
        /// <summary>Byte offset to the <see cref="File_Table"/> from beginning of the file</summary>
        public int OffsetToFileTable;
        /// <summary>Byte offset to the <see cref="Key_Table"/> from beginning of the file</summary>
        public int OffsetToKeyTable;
        ///<summary>The number of years after 1900 that the KEY file was built. (i.e. 2019 == 119)</summary>
        public int Build_Year;
        ///<summary>The number of days after January 1st the ERF file was built. (i.e. October 5th == 277)</summary>
        public int Build_Day;
        /// <summary>32 (usually) empty bytes reserved for future use.</summary>
        public byte[] reserved;

        //File Table
        /// <summary>An Entry that describes basic info about a <see cref="BIF"/> file</summary>
        public class File_Entry
        {
            ///<summary>The size of the <see cref="BIF"/> file in bytes.</summary>
            public int FileSize;
            ///<summary>The byte offset from the start of the file to the <see cref="BIF"/>'s filename.</summary>
            public int FilenameOffset;
            ///<summary>The size of the filename in <see cref="char"/>s</summary>
            public short FilenameSize;
            ///<summary>A 16-bit number representing which drive the <see cref="BIF"/> is installed on.</summary>
            public short Drives;

            ///<summary>The Filename of the <see cref="BIF"/> as a path from the <see cref="KEY"/>'s root directory</summary>
            public string Filename;
        }
        /// <summary>A List containing all of the <see cref="File_Entry"/>s associated with the linked <see cref="BIF"/> files.</summary>
        public List<File_Entry> File_Table = new List<File_Entry>();

        //Key Table
        /// <summary>An entry containing every resources string reference, Type, and ID.</summary>
        public class Key_Entry
        {
            ///<summary>The name of the resource. (16 <see cref="char"/>s)</summary>
            public string ResRef;
            ///<summary>The Resource Type ID of this resource. See: <see cref="Reference_Tables.Res_Types"/> </summary>
            public short ResourceType;
            ///<summary>The file extension representation of this resource. Obtained from <see cref="Reference_Tables.Res_Types"/> </summary>
            public string Type_Text; //Populated from Reference_Tables.Res_types[ResourceType]
            ///<summary>
            ///A unique ID number that denotes both which BIF this resource refers to, and the index of this resource in the <see cref="BIF.Variable_Resource_Table"/>
            ///<para/> ResID = (x &lt;&lt; 20) + y
            ///<para/> Where y is an index into <see cref="File_Table"/> to specify a <see cref="BIF"/>, and x is an index into that <see cref="BIF"/>'s <see cref="BIF.Variable_Resource_Table"/>.
            /// </summary>
            public int ResID;

            ///<summary>The x component of <see cref="ResID"/> which references an index in the <see cref="File_Table"/></summary>
            public int IDx;
            ///<summary>The y component of <see cref="ResID"/> which is an index into the <see cref="BIF.Variable_Resource_Table"/></summary>
            public int IDy; 
        }
        /// <summary>A list of all the <see cref="Key_Entry"/>s associted with the linked <see cref="BIF"/> files.</summary>
        public List<Key_Entry> Key_Table = new List<Key_Entry>();

        ///<summary>Initiates a new instance of the <see cref="KEY"/> class.</summary>
        public KEY() { }
    }

    /// <summary>
    /// Bioware 'RIM' Data.<para/>See: 
    /// <see cref="KReader.ReadRIM(Stream)"/>, 
    /// <seealso cref="kWriter.Write(RIM, Stream)"/>
    /// <remarks>
    /// <para/>RIM data stores bulk data that is not active, nor mutable. In is primarily used as the base template for each module upon load-in, where the data is then exported to an ERF file by the engine for faster futre reading.
    /// <para/>*NOTE: RIM files are not well documented, so support for issues with this class may be limited.
    /// </remarks>
    /// </summary>
    public class RIM : KFile
    {
        #region Class Definition
        //Header
        //FileType & Version in superclass
        ///<summary>4 bytes that appear to be null in every <see cref="RIM"/> I've come across so far.</summary>
        public byte[] Unknown;
        ///<summary>The number of files contained within the <see cref="RIM"/></summary>
        public int FileCount;
        ///<summary>Byte offset from start of the file to the <see cref="File_Table"/></summary>
        public int File_Table_Offset;
        ///<summary>Denotes a <see cref="RIM"/> that is an extension to another (marked by a filename ending in 'x')</summary>
        public bool IsExtension;

        ///<summary>99 empty bytes, probably reserved for backwards compatibility</summary>
        public byte[] Reserved;

        //File Table
        ///<summary>A File contained within the <see cref="RIM"/></summary>
        public class rFile
        {
            ///<summary>The file's name. (max 16 <see cref="char"/>s)</summary>
            public string Label;
            ///<summary>The type ID from <see cref="Reference_Tables.Res_Types"/></summary>
            public int TypeID;
            ///<summary>The Index of this file in <see cref="File_Table"/></summary>
            public int Index;
            ///<summary>Byte offset of <see cref="File_Data"/> from start of the <see cref="RIM"/></summary>
            public int DataOffset;
            ///<summary>The size of <see cref="File_Data"/> in bytes</summary>
            public int DataSize;

            ///<summary>The data contained within this file</summary>
            public byte[] File_Data; //populated from the FileData block
        }
        ///<summary>All of the <see cref="rFile"/>s contained within the <see cref="RIM"/>. This is the primary property of the <see cref="RIM"/></summary>
        public List<rFile> File_Table = new List<rFile>();

        #endregion

        #region Construction

        ///<summary>Initiates a new instance of the <see cref="RIM"/> class.</summary>
        public RIM() { }

        /// <summary>
        /// Initiates a new instance of the <see cref="RIM"/> class, given a set of Kotor source files, and filenames.
        /// </summary>
        /// <param name="files">An array of <see cref="KFile"/>s containing the resources to be added to be put in the <see cref="RIM"/></param>
        /// <param name="filenames">The names of the <see cref="KFile"/>s being added.</param>
        /// <param name="_IsExtension">Indicates whether this <see cref="RIM"/> is an extension of another. (Usually indicated by a RIM filename ending in 'x', leave "false" if unsure.) </param>
        public RIM(KFile[] files, string[] filenames, bool _IsExtension)
        {
            FileType = "RIM ";
            Version = "V1.0";

            Unknown = new byte[] { 0x0, 0x0, 0x0, 0x0 };
            FileCount = files.Count();
            File_Table_Offset = 120;
            IsExtension = _IsExtension;

            Reserved = new byte[99];
            for (int i = 0; i < 99; i++) { Reserved[i] = 0x0; }

            int totalOffset = File_Table_Offset + (32 * files.Count()) + 8;
            for (int i = 0; i < files.Count(); i++)
            {
                rFile rf = new rFile();
                rf.Label = filenames[i];
                rf.TypeID = Reference_Tables.TypeCodes[files[i].FileType];
                rf.Index = i;

                MemoryStream ms = new MemoryStream();

                if (files[i] is TwoDA) { kWriter.Write(files[i] as TwoDA, ms); }
                else if (files[i] is BIF) { kWriter.Write(files[i] as BIF, ms); }
                else if (files[i] is ERF) { kWriter.Write(files[i] as ERF, ms); }
                else if (files[i] is GFF) { kWriter.Write(files[i] as GFF, ms); }
                else if (files[i] is KEY) { kWriter.Write(files[i] as KEY, ms); }
                else if (files[i] is RIM) { kWriter.Write(files[i] as RIM, ms); }
                else if (files[i] is SSF) { kWriter.Write(files[i] as SSF, ms); }
                else if (files[i] is TLK) { kWriter.Write(files[i] as TLK, ms); }
                else { kWriter.Write(files[i] as MiscType, ms); }

                rf.File_Data = ms.ToArray();
                rf.DataSize = rf.File_Data.Count();
                rf.DataOffset = totalOffset;

                File_Table.Add(rf);

                totalOffset += rf.DataSize + 16;
            }

        }

        /// <summary>
        /// Appends a new KFile to the end of the <see cref="RIM"/>
        /// </summary>
        /// <param name="file">The <see cref="KFile"/> to be added.</param>
        /// <param name="filename">The name of the file.</param>
        public void Append_File(KFile file, string filename)
        {
            FileCount++;

            foreach (rFile r in File_Table)
            {
                r.DataOffset += 32;
            }

            rFile rf = new rFile();
            rf.Label = filename;
            rf.TypeID = Reference_Tables.TypeCodes[file.FileType];
            rf.Index = File_Table.Count();

            MemoryStream ms = new MemoryStream();

            if (file is TwoDA) { kWriter.Write(file as TwoDA, ms); }
            else if (file is BIF) { kWriter.Write(file as BIF, ms); }
            else if (file is ERF) { kWriter.Write(file as ERF, ms); }
            else if (file is GFF) { kWriter.Write(file as GFF, ms); }
            else if (file is KEY) { kWriter.Write(file as KEY, ms); }
            else if (file is RIM) { kWriter.Write(file as RIM, ms); }
            else if (file is SSF) { kWriter.Write(file as SSF, ms); }
            else if (file is TLK) { kWriter.Write(file as TLK, ms); }
            else { kWriter.Write(file as MiscType, ms); }

            rf.File_Data = ms.ToArray();
            rf.DataSize = rf.File_Data.Count();
            rf.DataOffset = File_Table.Last().DataOffset + File_Table.Last().DataSize + 16;

            File_Table.Add(rf);
        }

        /// <summary>
        /// Gets byte data from the RIM from the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte[] this[int index]
        {
            get
            {
                return File_Table[index].File_Data;
            }
        }

        /// <summary>
        /// Gets byte data from the RIM with the given filename
        /// </summary>
        /// <param name="filename">The Name (res_ref) of the file being referenced</param>
        /// <returns></returns>
        public byte[] this[string filename]
        {
            get
            {
                return File_Table.Where(rf => rf.Label == filename).FirstOrDefault().File_Data;
            }
        }

        /// <summary>
        /// Returns a formatted <see cref="KFile"/> read form the resource at the given index.
        /// </summary>
        /// <param name="index">The index of the file to be returned</param>
        /// <returns></returns>
        public KFile GetKFile (int index)
        {
            MemoryStream ms = new MemoryStream(File_Table[index].File_Data);

            KFile k;
            if (File_Table[index].TypeID == 2017)
            {
                k = KReader.Read2DA(ms);
            }
            else if (File_Table[index].TypeID == 9998)
            {
                k = KReader.ReadBIF(ms);
            }
            else if (Reference_Tables.ERFResTypes.Contains(File_Table[index].TypeID)) 
            {
                k = KReader.ReadERF(ms);
            }
            else if (Reference_Tables.GFFResTypes.Contains(File_Table[index].TypeID))
            {
                k = KReader.ReadGFF(ms);
            }
            else if (File_Table[index].TypeID == 3002)
            {
                k = KReader.ReadRIM(ms);
            }
            else if (File_Table[index].TypeID == 2060)
            {
                k = KReader.ReadSSF(ms);
            }
            else if (File_Table[index].TypeID == 2018)
            {
                k = KReader.ReadTLK(ms);
            }
            else
            {
                k = KReader.ReadKFile(ms);
            }
            return k;

        }
        #endregion

    }

    /// <summary>
    /// Bioware Soundset Data (v1.1).<para/>See: 
    /// <see cref="KReader.ReadSSF(Stream)"/>, 
    /// <seealso cref="kWriter.Write(SSF, Stream)"/>
    /// <remarks>
    /// <para/>Soundsets contain numerical references to the various sound effects different creatures make in the game.
    /// <para/>*NOTE: This program is currently only compatible with SSF version 1.1 files
    /// </remarks>
    /// </summary>
    public class SSF : KFile
    {
        //header
        //FileType & Version in superclass
        /// <summary>A 32-bit int that I'm not entirely sure the purpose of, but is present in all <see cref="SSF"/>s</summary>
        public int UnknownInt;

        //string refs or Sound class if populated
        /// <summary>A table containing all the sounds in the Sound Set. The <see cref="object"/> value will be an <see cref="int"/> index into a talk table if <see cref="TLKPopulated"/> is false, and <see cref="Sound"/> if true.</summary>
        public Dictionary<string, object> StringTable = new Dictionary<string, object>();

        /// <summary>Represents a sound in the <see cref="SSF"/>. This class is used when <see cref="TLKPopulated"/> is true.</summary>
        public class Sound
        {
            /// <summary>An <see cref="int"/> index into a <see cref="TLK"/> representing a particular sound and text.</summary>
            public int SRef;
            /// <summary>The Filename of the audio file linked to this sound</summary>
            public string SoundFile;
            /// <summary>The text representation of this sound</summary>
            public string SoundText;
        }

        /// <summary>A set 0xF bytes that pad until the end of the <see cref="SSF"/>, representing unused sounds</summary>
        public byte[] EndPadding;

        /// <summary>Whether the <see cref="SSF"/> has been populated by a <see cref="TLK"/> (usually dialog.tlk). If True then the string references have been converted into <see cref="Sound"/>s.</summary>
        public bool TLKPopulated;

        /// <summary>
        /// Populates the <see cref="StringTable"/> objects with <see cref="Sound"/>s based on the <see cref="int"/> string references.
        /// </summary>
        /// <param name="t">The talk table used to populate the Soundset. (usually dialog.tlk)</param>
        public void PopulateTLK(TLK t) 
        {
            if (!TLKPopulated)
            {
                int x = 0;
                object[] soundObjects = new object[28];
                StringTable.Values.CopyTo(soundObjects, 0);
                foreach (object o in soundObjects)
                {
                    Sound s = new Sound();
                    s.SRef = (int)o;
                    if ((int)o != -1)
                    {
                        s.SoundText = t.String_Data_Table[(int)o].StringText;
                        s.SoundFile = t.String_Data_Table[(int)o].SoundResRef;
                    }
                    else
                    {
                        s.SoundText = "No Sound";
                        s.SoundFile = "No Sound";
                    }
                    StringTable[Reference_Tables.SSFields[x]] = s;
                    x++;
                }
            }
            TLKPopulated = true;
        }

        ///<summary>Initiates a new instance of the <see cref="SSF"/> class.</summary>
        public SSF() { }

        /// <summary>
        /// Gets and Sets SoundSet values. The type will be <see cref="int"/> if <see cref="TLKPopulated"/> is false, and <see cref="Sound"/> if <see cref="TLKPopulated"/> is true.
        /// </summary>
        /// <param name="SSField">The string representation of the soundeffect from <see cref="Reference_Tables.SSFields"/></param>
        /// <returns></returns>
        public object this[string SSField] //value from referene table
        {
            get
            {
                return StringTable[SSField];
            }
            set
            {
                StringTable[SSField] = value;
            }
        }
    }

    /// <summary>
    /// Bioware Talk Table Data.<para/>See: 
    /// <see cref="KReader.ReadTLK(Stream)"/>, 
    /// <seealso cref="kWriter.Write(TLK, Stream)"/>
    /// <remarks>
    /// <para/>Talk Tables contain literally all of the text, spoken or written, for nearly the entire game. Their purpose is to make an easily swappable file (in KotOR's case 'dialog.tlk') to integrate different languages.
    /// <para/>In addition to string references, they also reference sound files.
    /// </remarks>
    /// </summary>
    public class TLK: KFile
    {
        //header
        //FileType & Version in superclass
        ///<summary>The numerical ID for the Language that the string entries in this Talk Table will be in. <para/>See: 
        ///<see cref="Reference_Tables.Language_IDs"/></summary>
        public int LanguageID;
        ///<summary>The number of strings in this Talk Table</summary>
        public int StringCount;
        ///<summary>The byte offset from the start of the file to the String Entries</summary>
        public int StringEntriesOffset;

        //String Data Table
        /// <summary>An element on the <see cref="String_Data_Table"/>.</summary>
        public class String_Data
        {
            /// <summary>Bitwise Flags about this string reference. See: <see cref="TEXT_PRESENT"/>, <see cref="SND_PRESENT"/>, and <see cref="SNDLENGTH_PRESENT"/></summary>
            public int Flags;
            /// <summary>16 character resource reference for the wave file associated with this sound</summary>
            public string SoundResRef; //char[16]
            /// <summary>Marked as not used by the documentation, but presumably adjust volume.</summary>
            public int VolumeVariance;
            /// <summary>Marked as not used by the documentation, but presumably adjust pitch.</summary>
            public int PitchVariance;
            /// <summary>Byte offset from <see cref="StringEntriesOffset"/> to the string's text.</summary>
            public int OffsetToString;
            /// <summary>The size of the string in <see cref="char"/>s.</summary>
            public int StringSize;
            /// <summary>The duration of the sound in seconds</summary>
            public float SoundLength;

            /// <summary>Whether or not text exist for the string. *Note: these flags don't see much use in KotOR</summary>
            public bool TEXT_PRESENT = false;
            /// <summary>Whether or not a Sound exist for the string. *Note: these flags don't see much use in KotOR</summary>
            public bool SND_PRESENT = false;
            /// <summary>Whether or not a sound length is present for this string. *Note: these flags don't see much use in KotOR</summary>
            public bool SNDLENGTH_PRESENT = false;

            /// <summary>The text associated with the string.</summary>
            public string StringText;
        }
        /// <summary>The tabel containing all of the <see cref="String_Data"/> elements. This is the primary property of the <see cref="TLK"/>.</summary>
        public List<String_Data> String_Data_Table = new List<String_Data>();

        ///<summary>Initiates a new instance of the <see cref="TLK"/> class.</summary>
        public TLK() { }

        /// <summary>
        /// Gets a particular string from a string reference
        /// </summary>
        /// <param name="str_ref">The reference number (index) for this particular string.</param>
        /// <returns></returns>
        public string this[int str_ref]
        {
            get
            {
                return String_Data_Table[str_ref].StringText;
            }
            set
            {
                int delta_offset = value.Length - String_Data_Table[str_ref].StringSize;
                String_Data_Table[str_ref].StringSize = value.Length;
                String_Data_Table[str_ref].StringText = value;
                for (int i = str_ref + 1; i < StringCount; i++)
                {
                    String_Data_Table[i].OffsetToString += delta_offset;
                }
            }
        }
    }

    /// <summary>
    /// Represents other Kotor source files that aren't yet implemented. Contains only raw byte data.
    /// </summary>
    /// <remarks>
    /// This is usually used for kotor source files such as Waves, textures, models, and other formats that don't have easily interpretable data.
    /// </remarks>
    public class MiscType: KFile
    {
        ///<summary>The raw byte data for this source fil.</summary>
        public byte[] data;

        ///<summary>Initiates a new instance of the <see cref="MiscType"/> class.</summary>
        public MiscType() { }

        /// <summary>
        /// Initiates a new instance of the <see cref="MiscType"/> class from byte data
        /// </summary>
        /// <param name="data">A byte array containing the file data.</param>
        public MiscType(byte[] data)
        {
            char[] ft = new char[4] { (char)data[0], (char)data[1], (char)data[2], (char)data[3] };
            FileType = new string(ft);

            char[] fv = new char[4] { (char)data[5], (char)data[6], (char)data[7], (char)data[8] };
            Version = new string(fv);

            this.data = data;
        }
    }

   
    /// <summary>
    /// A set of reference data used by the readers and constructors to generate user friendly data. 
    /// </summary>
    static public class Reference_Tables
    {
        /// <summary> Dictionary for conversion of Resource Type IDs to resource file extension. </summary>
        public static Dictionary<int, string> Res_Types = new Dictionary<int, string>() { { 0, "null" }, { 1, "bmp" }, { 3, "tga" }, { 4, "wav" }, { 6, "plt" }, { 7, "ini" }, { 10, "txt" }, { 2002, "mdl" }, { 2009, "nss" }, { 2011, "mod" }, { 2010, "ncs" }, { 2012, "are" }, { 2013, "set" }, { 2014, "ifo" }, { 2015, "bic" }, { 2016, "wok" }, { 2017, "2da" }, { 2018, "tlk" }, { 2022, "txi" }, { 2023, "git" }, { 2024, "bti" }, { 2025, "uti" }, { 2026, "btc" }, { 2027, "utc" }, { 2029, "dlg" }, { 2030, "itp" }, { 2032, "utt" }, { 2033, "dds" }, { 2035, "uts" }, { 2036, "ltr" }, { 2037, "gff" }, { 2038, "fac" }, { 2040, "ute" }, { 2042, "utd" }, { 2044, "utp" }, { 2045, "dft" }, { 2046, "gic" }, { 2047, "gui" }, { 2051, "utm" }, { 2052, "dwk" }, { 2053, "pwk" }, { 2056, "jrl" }, { 2057, "mod" }, { 2058, "utw" }, { 2060, "ssf" }, { 2061, "hak" }, { 2064, "ndb" }, { 2065, "ptm" }, { 2066, "ptt" }, { 3000, "lyt" }, { 3001, "vis" }, { 3002, "rim" }, { 3007, "tpc" }, { 3008, "mdx" }, { 9999, "key" }, { 9998, "bif" }, { 9997, "erf" } };
        /// <summary> Dictionary for conversion of Language IDs to Language name. </summary>
        public static Dictionary<int, string> Language_IDs = new Dictionary<int, string>() { { 0, "English" }, { 1, "French" }, { 2, "German" }, { 3, "Italian" }, { 4, "Spanish" }, { 5, "Polish" }, { 128, "Korean" }, { 129, "Chinese Traditional" }, { 130, "Chinese Simplified" }, { 131, "Japanese" } };
        /// <summary> Dictionary for conversion of GFF Field Type IDs to GFF Field data type text. </summary>
        public static Dictionary<int, string> Field_Types = new Dictionary<int, string>() { { 0, "BYTE" }, { 1, "CHAR" }, { 2, "WORD" }, { 3, "SHORT" }, { 4, "DWORD" }, { 5, "INT" }, { 6, "DWORD64" }, { 7, "INT64" }, { 8, "FLOAT" }, { 9, "DOUBLE" }, { 10, "CExoString" }, { 11, "ResRef" }, { 12, "CExoLocString" }, { 13, "VOID" }, { 14, "Struct" }, { 15, "List" }, { 16, "Orientation"}, { 17, "Vector"}, { 18, "StrRef"} };
        /// <summary> List of GFF Field Types that are marked by the interpreter as "complex". </summary>
        public static List<int> Complex_Field_Types = new List<int>() { 6, 7, 9, 10, 11, 12, 13, 14, 15 };
        /// <summary> List of Default Fields in Sound set files. </summary>
        public static List<string> SSFields = new List<string>() { "Battlecry 1", "Battlecry 2", "Battlecry 3", "Battlecry 4", "Battlecry 5", "Battlecry 6", "Select 1", "Select 2", "Select 3", "Attack Grunt 1", "Attack Grunt 2", "Attack Grunt 3","Pain Grunt 1", "Pain Grunt 2", "Low Health", "Dead", "Critical Hit", "Target Immune to Assault", "Lay Mine", "Disarm Mine", "Begin Stealth", "Begin Search", "Begin Unlock", "Unlock Failed", "Unlock Success", "Separate from Party", "Rejoin Party", "Poisoned"};
        /// <summary> Dictionary for conversion of 4 char FileTypes into resource IDs</summary>
        public static Dictionary<string, int> TypeCodes = new Dictionary<string, int>() { { "NULL", 0 }, { "BMP ", 1 }, { "TGA ", 3 }, { "WAV ", 4 }, { "PLT ", 6 }, { "INI ", 7 }, { "TXT ", 10 }, { "MDL ", 2002 }, { "NSS ", 2009 }, { "MOD ", 2011 }, { "NCS ", 2010 }, { "ARE ", 2012 }, { "SET ", 2013 }, { "IFO ", 2014 }, { "BIC ", 2015 }, { "WOK ", 2016 }, { "2DA ", 2017 }, {"TLK ", 2018 }, { "TXI ", 2022 }, { "GIT ", 2023 }, { "BTI ", 2024 }, { "UTI ", 2025 }, { "BTC ", 2026 }, { "UTC ", 2027 }, { "DLG ", 2029 }, { "ITP ", 2030 }, { "UTT ", 2032 }, { "DDS ", 2033 }, { "UTS ", 2035 }, { "LTR ", 2036 }, { "GFF ", 2037 }, { "FAC ", 2038 }, { "UTE ", 2040 }, { "UTD ", 2042 }, { "UTP ", 2044 }, { "DFT ", 2045 }, { "GIC ", 2046 }, { "GUI ", 2047 }, { "UTM ", 2051 }, { "DWK ", 2052 }, { "PWK ", 2053 }, { "JRL ", 2056 }, { "SAV ", 2057 }, { "UTW ", 2058 }, { "SSF ", 2060 }, { "HAK ", 2061 }, { "NDB ", 2064 }, { "PTM ", 2065 }, { "PTT ", 2066 }, { "LYT ", 3000 }, { "VIS ", 3001 }, { "RIM ", 3002 }, { "TPC ", 3007 }, { "MDX ", 3008 }, { "KEY ", 9999 }, { "BIF ", 9998 }, { "ERF ", 9997 } };
        /// <summary> List of resource type codes that are of GFF type.</summary>
        public static List<int> GFFResTypes = new List<int>() { 2012, 2014, 2015, 2023, 2025, 2027, 2029, 2030, 2032, 2035, 2037, 2038, 2040, 2042, 2044, 2046, 2047, 2051, 2056, 2058, 2065, 2066 };
        /// <summary> List of resource type codes that are of GFF type.</summary>
        public static List<int> ERFResTypes = new List<int>() { 2011, 2057, 9997 };
    }
}
