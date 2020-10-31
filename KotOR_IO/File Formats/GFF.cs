using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// BioWare General File Format Data. <para/>
    /// See: <see cref="GFF(Stream)"/>, 
    /// <seealso  cref="Write(Stream)"/>
    /// <remarks>
    /// <para/>The GFF file is the sort of 'Catch-all' format in the Aurora Engine. It is used to represent nearly every object in the game, from items (.UTI) to static area info (.GIT).
    /// </remarks>
    /// </summary>
    public class GFF : KFile
    {
        #region Constructors
        
        ///<summary>Initiates a new instance of the <see cref="GFF"/> class.</summary>
        public GFF() { }

        /// <summary>
        /// Initiates a new instance of the <see cref="GFF"/> class from raw byte data.
        /// </summary>
        /// <param name="raw_data"></param>
        public GFF(byte[] raw_data)
            : this(new MemoryStream(raw_data))
        { }

        /// <summary>
        /// Reads Bioware General File Format Files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        public GFF(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                //header
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));
                StructOffset = br.ReadInt32();
                StructCount = br.ReadInt32();
                FieldOffset = br.ReadInt32();
                FieldCount = br.ReadInt32();
                LabelOffset = br.ReadInt32();
                LabelCount = br.ReadInt32();
                FieldDataOffset = br.ReadInt32();
                FieldDataCount = br.ReadInt32();
                FieldIndicesOffset = br.ReadInt32();
                FieldIndicesCount = br.ReadInt32();
                ListIndicesOffset = br.ReadInt32();
                ListIndicesCount = br.ReadInt32();

                br.BaseStream.Seek(StructOffset, SeekOrigin.Begin);
                //Struct Array
                for (int i = 0; i < StructCount; i++)
                {
                    GFFStruct GS = new GFFStruct();
                    GS.Type = br.ReadInt32();
                    GS.DataOrDataOffset = br.ReadInt32();
                    GS.FieldCount = br.ReadInt32();
                    Struct_Array.Add(GS);
                }

                br.BaseStream.Seek(FieldOffset, SeekOrigin.Begin);
                //Field Array
                for (int i = 0; i < FieldCount; i++)
                {
                    Field GF = new Field();
                    GF.Type = br.ReadInt32();
                    GF.LabelIndex = br.ReadInt32();
                    GF.DataOrDataOffset = br.ReadInt32();
                    GF.Type_Text = Reference_Tables.Field_Types[GF.Type];
                    GF.Complex = Reference_Tables.Complex_Field_Types.Contains(GF.Type);
                    Field_Array.Add(GF);
                }

                br.BaseStream.Seek(LabelOffset, SeekOrigin.Begin);
                //Label Array
                for (int i = 0; i < LabelCount; i++)
                {
                    string l = new string(br.ReadChars(16)).TrimEnd('\0');
                    Label_Array.Add(l);
                }

                //Attaching Labels to fields
                foreach (Field GF in Field_Array)
                {
                    GF.Label = Label_Array[GF.LabelIndex];
                }

                br.BaseStream.Seek(FieldIndicesOffset, SeekOrigin.Begin);
                //Field Indices
                for (int i = 0; i < (FieldIndicesCount / 4); i++)
                {
                    Field_Indices.Add(br.ReadInt32());
                }

                br.BaseStream.Seek(ListIndicesOffset, SeekOrigin.Begin);
                //List Indices
                for (int i = 0; i < ListIndicesCount; i++)
                {
                    List_Index LI = new List_Index();
                    LI.Size = br.ReadInt32();
                    i += 4;
                    for (int k = 0; k < LI.Size; k++)
                    {
                        LI.Indices.Add(br.ReadInt32());
                        i += 4;
                    }
                    List_Indices.Add(LI);
                }

                //Field Data / populating each fields Field_data array (except maybe for LIst and structs)
                foreach (Field GF in Field_Array)
                {
                    br.BaseStream.Seek(FieldDataOffset, SeekOrigin.Begin);

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
                        }
                        #endregion
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
                                CExoString CES = new CExoString();
                                CES.Size = br.ReadInt32();
                                CES.Text = new string(br.ReadChars(CES.Size));
                                GF.Field_Data = CES;
                                break;
                            case 11:
                                CResRef CRR = new CResRef();
                                CRR.Size = br.ReadByte();
                                CRR.Text = new string(br.ReadChars(CRR.Size));
                                GF.Field_Data = CRR;
                                break;
                            case 12:
                                CExoLocString CELS = new CExoLocString();
                                CELS.Total_Size = br.ReadInt32();
                                CELS.StringRef = br.ReadInt32();
                                CELS.StringCount = br.ReadInt32();
                                for (int i = 0; i < CELS.StringCount; i++)
                                {
                                    CExoLocString.SubString SS = new CExoLocString.SubString();
                                    SS.StringID = br.ReadInt32();
                                    SS.StringLength = br.ReadInt32();
                                    SS.Text = new string(br.ReadChars(SS.StringLength));
                                    CELS.SubStringList.Add(SS);
                                }
                                GF.Field_Data = CELS;
                                break;
                            case 13:
                                Void_Binary VB = new Void_Binary();
                                VB.Size = br.ReadInt32();
                                VB.Data = br.ReadBytes(VB.Size);
                                GF.Field_Data = VB;
                                break;
                            case 14:
                                GF.Field_Data = Struct_Array[GF.DataOrDataOffset];
                                break;
                            case 15:
                                //List Stuff (maybe separate conditional group?)
                                br.BaseStream.Seek(ListIndicesOffset, SeekOrigin.Begin);
                                br.BaseStream.Seek(GF.DataOrDataOffset, SeekOrigin.Current);
                                List<GFFStruct> LGS = new List<GFFStruct>();
                                int tempSize = br.ReadInt32();
                                for (int i = 0; i < tempSize; i++)
                                {
                                    LGS.Add(Struct_Array[br.ReadInt32()]);
                                }
                                GF.Field_Data = LGS;
                                break;
                            case 16:
                                Orientation OR = new Orientation();
                                OR.float1 = br.ReadSingle();
                                OR.float2 = br.ReadSingle();
                                OR.float3 = br.ReadSingle();
                                OR.float4 = br.ReadSingle();
                                GF.Field_Data = OR;
                                break;
                            case 17:
                                Vector VC = new Vector();
                                VC.x_component = br.ReadSingle();
                                VC.y_component = br.ReadSingle();
                                VC.z_component = br.ReadSingle();
                                GF.Field_Data = VC;
                                break;
                            case 18:
                                StrRef SR = new StrRef();
                                SR.leading_value = br.ReadInt32();
                                SR.reference = br.ReadInt32();
                                GF.Field_Data = SR;
                                break;
                        }
                        #endregion
                    }
                }

                //populate StructData
                foreach (GFFStruct GS in Struct_Array)
                {
                    if (GS.FieldCount == 1)
                    {
                        GS.StructData = Field_Array[GS.DataOrDataOffset];
                    }
                    else if (GS.FieldCount > 1)
                    {
                        List<Field> LGF = new List<Field>();
                        int temp_field_index = 0;
                        for (int i = 0; i < GS.FieldCount; i++)
                        {
                            br.BaseStream.Seek(GS.DataOrDataOffset + FieldIndicesOffset, SeekOrigin.Begin);
                            temp_field_index = br.ReadInt32();
                            LGF.Add(Field_Array[temp_field_index]);
                            GS.Field_Indexes.Add(temp_field_index);
                        }
                        GS.StructData = LGF;
                    }
                    else
                    {
                        GS.StructData = null;
                    }
                }
            }
        }

        #endregion

        #region Properties

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

        /// <summary>The array of all of the GFF Structs stored in this file, this is where the bulk of the data will be stored</summary>
        public List<GFFStruct> Struct_Array = new List<GFFStruct>();
        /// <summary>The array of all fields contained withing the GFF</summary>
        public List<Field> Field_Array = new List<Field>();
        //Label Array
        /// <summary>The array of all Field labels (aka variable names) in the GFF</summary>
        public List<string> Label_Array = new List<string>();
        //Field_Indices
        /// <summary>A list of Index references used to assign fields to structs.</summary>
        public List<int> Field_Indices = new List<int>();
        /// <summary>The array contianing all of the <see cref="List_Index"/> elements.</summary>
        public List<List_Index> List_Indices = new List<List_Index>();

        #endregion

        #region Methods

        /// <summary>
        /// Writes Bioware General File Format data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        internal override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                //Header
                bw.Write(FileType.ToArray());
                bw.Write(Version.ToArray());
                bw.Write(StructOffset);
                bw.Write(StructCount);
                bw.Write(FieldOffset);
                bw.Write(FieldCount);
                bw.Write(LabelOffset);
                bw.Write(LabelCount);
                bw.Write(FieldDataOffset);
                bw.Write(FieldDataCount);
                bw.Write(FieldIndicesOffset);
                bw.Write(FieldIndicesCount);
                bw.Write(ListIndicesOffset);
                bw.Write(ListIndicesCount);

                //Struct Array
                bw.Seek(StructOffset, SeekOrigin.Begin);
                foreach (GFFStruct GS in Struct_Array)
                {
                    if (GS.StructData == null) { continue; }
                    bw.Write(GS.Type);
                    bw.Write(GS.DataOrDataOffset);
                    bw.Write(GS.FieldCount);
                }

                //Field Array
                bw.Seek(FieldOffset, SeekOrigin.Begin);
                foreach (Field GF in Field_Array)
                {
                    bw.Write(GF.Type);
                    bw.Write(GF.LabelIndex);
                    bw.Write(GF.DataOrDataOffset);
                }

                //Label Array
                bw.Seek(LabelOffset, SeekOrigin.Begin);
                foreach (string l in Label_Array)
                {
                    bw.Write(l.PadRight(16, '\0').ToArray());
                }

                //Field Data Block
                foreach (Field GF in Field_Array)
                {
                    if (GF.Complex)
                    {
                        bw.Seek(FieldDataOffset + GF.DataOrDataOffset, SeekOrigin.Begin);
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
                                bw.Write((GF.Field_Data as CExoString).Size);
                                bw.Write((GF.Field_Data as CExoString).Text.ToArray());
                                break;
                            case 11:
                                bw.Write((GF.Field_Data as CResRef).Size);
                                bw.Write((GF.Field_Data as CResRef).Text.ToArray());
                                break;
                            case 12:
                                bw.Write((GF.Field_Data as CExoLocString).Total_Size);
                                bw.Write((GF.Field_Data as CExoLocString).StringRef);
                                bw.Write((GF.Field_Data as CExoLocString).StringCount);
                                foreach (CExoLocString.SubString SS in (GF.Field_Data as CExoLocString).SubStringList)
                                {
                                    bw.Write(SS.StringID);
                                    bw.Write(SS.StringLength);
                                    bw.Write(SS.Text.ToArray());
                                }
                                break;
                            case 13:
                                bw.Write((GF.Field_Data as Void_Binary).Size);
                                bw.Write((GF.Field_Data as Void_Binary).Data);
                                break;
                            case 16:
                                bw.Write((GF.Field_Data as Orientation).float1);
                                bw.Write((GF.Field_Data as Orientation).float2);
                                bw.Write((GF.Field_Data as Orientation).float3);
                                bw.Write((GF.Field_Data as Orientation).float4);
                                break;
                            case 17:
                                bw.Write((GF.Field_Data as Vector).x_component);
                                bw.Write((GF.Field_Data as Vector).y_component);
                                bw.Write((GF.Field_Data as Vector).z_component);
                                break;
                            case 18:
                                bw.Write((GF.Field_Data as StrRef).leading_value);
                                bw.Write((GF.Field_Data as StrRef).reference);
                                break;
                            default:
                                break;
                        }
                    }
                }

                //Field Indices Array
                // I did this the hard way initially, SAVE THIS CODE FOR FUTURE CONSTRUCTOR
                //IEnumerable<GFF.GFFStruct> IndexingStructs = from strc in Struct_Array where strc.FieldCount > 1 select strc;
                //foreach (GFF.GFFStruct GS in IndexingStructs)
                //{
                //    bw.Seek(FieldIndicesOffset + GS.DataOrDataOffset, SeekOrigin.Begin);
                //    for (int i = 0; i < GS.FieldCount; i++) { bw.Write(Field_Array.IndexOf((GS.StructData as List<GFF.Field>)[i])); }
                //}
                bw.Seek(FieldIndicesOffset, SeekOrigin.Begin);
                foreach (int i in Field_Indices) { bw.Write(i); }

                //List Indices Array
                bw.Seek(ListIndicesOffset, SeekOrigin.Begin);
                foreach (List_Index LI in List_Indices)
                {
                    bw.Write(LI.Size);
                    foreach (int i in LI.Indices) { bw.Write(i); }
                }
            }
        }

        /// <summary>
        /// Gets field data given a field label. If more than one fields have this label, type will be <see cref="List{object}"/>, otherwise type will be <see cref="object"/>
        /// </summary>
        /// <param name="Field_Label"></param>
        /// <returns></returns>
        public object this[string Field_Label]
        {
            get
            {
                int count = Field_Array.Where(p => p.Label == Field_Label).Count();

                if (count == 0)
                {
                    throw new Exception("Field_Label \"" + Field_Label + "\" does not exist in this GFF");
                }

                else if (count == 1)
                {
                    return Field_Array.Where(p => p.Label == Field_Label).FirstOrDefault().Field_Data;
                }

                else if (count > 1)
                {
                    return Field_Array.Where(p => p.Label == Field_Label);
                }

                else
                {
                    throw new Exception("Something has gone seriously wrong. There should not be a negative quantity of Fields.");
                }
            }
        }

        #region Add and Delete Field Methods
        /// <summary>
        /// Returns the index of the nth occurance of a field with the specified label.
        /// </summary>
        /// <param name="label">The <see cref="Field"/> label.</param>
        /// <param name="occurance">The nth occurance the check</param>
        /// <returns></returns>
        public int get_Field_Index(string label, int occurance)
        {
            return Field_Array.IndexOf(Field_Array.Where(f => f.Label == label.TrimEnd('\0')).ToArray()[occurance - 1]);
        }
        /// <summary>
        /// Returns the index of of the given field.
        /// </summary>
        /// <param name="F"></param>
        /// <returns></returns>
        public int get_Field_Index(Field F)
        {
            return Field_Array.IndexOf(F);
        }

        /// <summary>
        /// Gets the index of the given label
        /// </summary>
        /// <param name="label">The label given. (16 <see cref="char"/> or less)</param>
        /// <returns></returns>
        public int get_Label_Index(string label)
        {
            return Label_Array.IndexOf(label.TrimEnd('\0'));
        }

        /// <summary>
        /// Add a new <see cref="GFFStruct"/> to the end of the <see cref="Struct_Array"/> given the indices of the containing fields.
        /// </summary>
        /// <param name="_Type">Programmer-defined integer ID for the struct type. Varies from from file to file.</param>
        /// <param name="_Field_Indices">An array of integer index values that reference different feilds in the <see cref="Field_Array"/> block.
        /// <para/> SEE: <see cref="get_Field_Index(Field)"/> for finding field indices
        /// </param>
        public void add_struct(int _Type, params int[] _Field_Indices)
        {
            GFFStruct GF = new GFFStruct();
            GF.Type = _Type;
            GF.FieldCount = _Field_Indices.Length;

            FieldOffset += 12;
            LabelOffset += 12;
            FieldDataOffset += 12;

            if (GF.FieldCount == 1)
            {
                GF.DataOrDataOffset = _Field_Indices[0];
                ListIndicesOffset += 12;
            }
            else
            {
                GF.DataOrDataOffset = ListIndicesOffset - FieldIndicesOffset; //Using this before the list offset is set to represent end of FieldIndices Array, where the new indices will be appended.
                Field_Indices.AddRange(_Field_Indices);
                FieldIndicesCount += GF.FieldCount * 4;
                ListIndicesOffset += 12 + (4 * GF.FieldCount);
            }

            FieldIndicesOffset += 12;

            Struct_Array.Add(GF);
        }

        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="byte"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(byte data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 0;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = data;

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = false;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldIndicesOffset += 12;
            ListIndicesOffset += 12;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="char"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(char data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 1;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = data;

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = false;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldIndicesOffset += 12;
            ListIndicesOffset += 12;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="ushort"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(ushort data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 2;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = data;

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = false;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldIndicesOffset += 12;
            ListIndicesOffset += 12;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="short"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(short data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 3;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = data;

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = false;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldIndicesOffset += 12;
            ListIndicesOffset += 12;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="uint"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(uint data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 4;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = (int)data;

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = false;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldIndicesOffset += 12;
            ListIndicesOffset += 12;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="int"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(int data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 5;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = data;

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = false;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldIndicesOffset += 12;
            ListIndicesOffset += 12;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="float"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(float data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 8;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = BitConverter.ToInt32(BitConverter.GetBytes(data), 0);

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = false;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldIndicesOffset += 12;
            ListIndicesOffset += 12;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="ulong"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(ulong data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 6;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = FieldIndicesOffset - FieldDataOffset; //Because the new data will be added at the end of the FieldDataBlock

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldDataCount += 8;
            FieldIndicesOffset += 20;
            ListIndicesOffset += 20;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="long"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(long data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 7;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = FieldIndicesOffset - FieldDataOffset; //Because the new data will be added at the end of the FieldDataBlock

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldDataCount += 8;
            FieldIndicesOffset += 20;
            ListIndicesOffset += 20;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="double"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(double data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 9;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = FieldIndicesOffset - FieldDataOffset; //Because the new data will be added at the end of the FieldDataBlock

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldDataCount += 8;
            FieldIndicesOffset += 20;
            ListIndicesOffset += 20;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="CExoString"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(CExoString data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 10;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = FieldIndicesOffset - FieldDataOffset; //Because the new data will be added at the end of the FieldDataBlock

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            int Off = data.Size + 4;
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldDataCount += Off;
            FieldIndicesOffset += 12 + Off;
            ListIndicesOffset += 12 + Off;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="CResRef"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(CResRef data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 11;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = FieldIndicesOffset - FieldDataOffset; //Because the new data will be added at the end of the FieldDataBlock

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldDataCount += data.Size + 1;
            FieldIndicesOffset += 12 + data.Size + 1;
            ListIndicesOffset += 12 + data.Size + 1;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="CExoLocString"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(CExoLocString data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 12;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = FieldIndicesOffset - FieldDataOffset; //Because the new data will be added at the end of the FieldDataBlock

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            int Off = 4 + data.Total_Size;
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldDataCount += Off;
            FieldIndicesOffset += 12 + Off;
            ListIndicesOffset += 12 + Off;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="Void_Binary"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(Void_Binary data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 13;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = FieldIndicesOffset - FieldDataOffset; //Because the new data will be added at the end of the FieldDataBlock

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldDataCount += data.Size + 4;
            FieldIndicesOffset += 12 + data.Size + 4;
            ListIndicesOffset += 12 + data.Size + 4;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="GFFStruct"/> data stored within this field. <para/> NOTE: <see cref="GFFStruct"/> must exist in the <see cref="Struct_Array"/>.</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(GFFStruct data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 14;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            if (Struct_Array.Contains(data)) { F.DataOrDataOffset = Struct_Array.IndexOf(data); }
            else { throw new Exception("GFF Struct does not yet exist, please see the GFF.add_struct method."); }

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldIndicesOffset += 12;
            ListIndicesOffset += 12;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The list of structs stored within this field. NOTE: The <see cref="GFFStruct"/>s must exist in the <see cref="Struct_Array"/>.</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(List<GFFStruct> data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 15;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = ListIndicesOffset + ListIndicesCount; //End of the listIndices array

            //List Indices
            List_Index LI = new List_Index();
            LI.Size = 0;
            foreach (GFFStruct GS in data)
            {
                LI.Size++;
                LI.Indices.Add(Struct_Array.IndexOf(GS));
            }
            List_Indices.Add(LI);

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldIndicesOffset += 12;
            ListIndicesOffset += 12;
            ListIndicesCount += 4 + (4 * LI.Size);

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="Orientation"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(Orientation data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 16;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = FieldIndicesOffset - FieldDataOffset; //Because the new data will be added at the end of the FieldDataBlock

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldDataCount += 16;
            FieldIndicesOffset += 28;
            ListIndicesOffset += 28;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="Vector"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(Vector data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 17;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = FieldIndicesOffset - FieldDataOffset; //Because the new data will be added at the end of the FieldDataBlock

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldDataCount += 12;
            FieldIndicesOffset += 24;
            ListIndicesOffset += 24;

            FieldCount++;

            Field_Array.Add(F);
        }
        /// <summary>
        /// Adds a new field to the <see cref="Field_Array"/>.
        /// <para/> NOTE: This adds the field to the array, but it will not be initiallized until it is added to a struct. See <see cref="GFFStruct.add_field(int, GFF)"/>
        /// </summary>
        /// <param name="data">The <see cref="StrRef"/> data stored within this field</param>
        /// <param name="label">The Label of the field being added</param>
        public void add_field(StrRef data, string label)
        {
            Field F = new Field();

            //Binary Content
            F.Type = 18;

            if (Label_Array.Contains(label)) { F.LabelIndex = Label_Array.IndexOf(label); }
            else { F.LabelIndex = add_label(label); }

            F.DataOrDataOffset = FieldIndicesOffset - FieldDataOffset; //Because the new data will be added at the end of the FieldDataBlock

            //User Content
            F.Field_Data = data;
            F.Label = label;
            F.Complex = true;
            F.Type_Text = Reference_Tables.Field_Types[F.Type];

            //Offsets
            LabelOffset += 12;
            FieldDataOffset += 12;
            FieldDataCount += 8;
            FieldIndicesOffset += 20;
            ListIndicesOffset += 20;

            FieldCount++;

            Field_Array.Add(F);
        }

        /// <summary>
        /// Deletes a nth occurance of a field with the given label.
        /// </summary>
        /// <param name="label">The label of the field to be deleted</param>
        /// <param name="occurance">The occurance of this label in the field array.</param>
        public void delete_field(string label, int occurance)
        {
            int index = get_Field_Index(label, occurance);

            foreach (GFFStruct GS in Struct_Array)
            {
                if (GS.FieldCount == 1)
                {
                    if (GS.DataOrDataOffset == index)
                    {
                        GS.delete_field(Field_Array[index], index);
                    }
                    else if (GS.DataOrDataOffset > index)
                    {
                        GS.DataOrDataOffset--;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (GS.FieldCount > 1)
                {
                    if ((GS.StructData as List<Field>).Contains(Field_Array[index]))
                    {
                        GS.delete_field(Field_Array[index], index);
                    }
                }
            }

            //Field Indices Adjustment
            int Field_Indices_Removed = 0;
            for (int i = 0; i < Field_Indices.Count(); i++)
            {
                if (Field_Indices[i] == index)
                {
                    Field_Indices.RemoveAt(i);
                    Field_Indices_Removed++;
                }
                else if (Field_Indices[i] > index)
                {
                    Field_Indices[i]--;
                }
            }

            //General Offset adjustments
            LabelOffset -= 12;
            FieldDataOffset -= 12;
            FieldIndicesOffset -= 12;
            ListIndicesOffset -= (12 + 4 * Field_Indices_Removed); //need to subtract out the number of occurances in Field indices deleted

            //Complex Offset Adjustments
            int Off;
            switch (Field_Array[index].Type)
            {
                case 6:
                case 7:
                case 9:
                    FieldDataCount -= 8;
                    FieldIndicesOffset -= 8;
                    ListIndicesOffset -= 8;
                    break;
                case 10:
                    Off = (Field_Array[index].Field_Data as CExoString).Size + 4;
                    FieldDataCount -= Off;
                    FieldIndicesOffset -= Off;
                    ListIndicesOffset -= Off;
                    break;
                case 11:
                    Off = (Field_Array[index].Field_Data as CResRef).Size + 1;
                    FieldDataCount -= Off;
                    FieldIndicesOffset -= Off;
                    ListIndicesOffset -= Off;
                    break;
                case 12:
                    Off = (Field_Array[index].Field_Data as CExoLocString).Total_Size + 4;
                    FieldDataCount -= Off;
                    FieldIndicesOffset -= Off;
                    ListIndicesOffset -= Off;
                    break;
                case 13:
                    Off = (Field_Array[index].Field_Data as Void_Binary).Size + 4;
                    FieldDataCount -= Off;
                    FieldIndicesOffset -= Off;
                    ListIndicesOffset -= Off;
                    break;
                case 14:
                    break; //No offset adjustments for struct
                case 15:
                    int li_index = get_ListIndices_superIndex(Field_Array[index].DataOrDataOffset); //List need to remove indices
                    ListIndicesCount -= 4 + (4 * List_Indices[li_index].Size);
                    List_Indices.RemoveAt(li_index);
                    break;
                case 16:
                    FieldDataCount -= 16;
                    FieldIndicesOffset -= 16;
                    ListIndicesOffset -= 16;
                    break;
                case 17:
                    FieldDataCount -= 12;
                    FieldIndicesOffset -= 12;
                    ListIndicesOffset -= 12;
                    break;
                case 18:
                    FieldDataCount -= 8;
                    FieldIndicesOffset -= 8;
                    ListIndicesOffset -= 8;
                    break;
                default:
                    break;
            }

            //Proper field removal
            FieldCount--;

            Field_Array.RemoveAt(index);

            //Potential Label Removal
            if (Field_Array.Where(x => x.Label == label).Count() == 0)
            {
                delete_label(label);
            }
        }
        #endregion

        /// <summary>
        /// Adds a new label to the <see cref="Label_Array"/>. <para/> Returns the index of this new label.
        /// </summary>
        /// <param name="label">The field label to be added to <see cref="Label_Array"/>. Must be 16 <see cref="char"/>s or less</param>
        /// <returns></returns>
        public int add_label(string label)
        {
            if (label.Length > 16) { throw new Exception("Label length must be less than 16"); }
            LabelCount++;
            Label_Array.Add(label.PadRight(16, '\0'));
            FieldDataOffset += 16;
            FieldIndicesOffset += 16;
            ListIndicesOffset += 16;
            return Label_Array.IndexOf(Label_Array.Last());
        }

        /// <summary>
        /// Deletes a label that is attached to no existing fields.
        /// </summary>
        /// <param name="label">The label to be deleted</param>
        public void delete_label(string label)
        {
            if (Field_Array.Where(x => x.Label == label).Count() == 1)
            {
                new Exception("This label is currently is use by another field!");
            }

            Label_Array.Remove(label);
            FieldDataOffset -= 16;
            FieldIndicesOffset -= 16;
            ListIndicesOffset -= 16;
            LabelCount--;
        }

        private int get_ListIndices_superIndex(int dataOffset)
        {
            int LI_off = dataOffset; //byte offset from start of Listindices block
            int Elapsed_off = 0; //The amount of bytes that have been iterated past.
            int LI_super_index = 0; //The index in the List_Indices Object.
            foreach (List_Index li in List_Indices)
            {
                if (LI_off == Elapsed_off) { break; }

                Elapsed_off += li.Size + 4;
                LI_super_index++;
            }
            return LI_super_index;
        }


        #endregion

        #region Nested Classes

        //Struct Array
        /// <summary>
        /// A GFF object that holds a set of <see cref="Field"/>s, each having there own type and data.
        /// </summary>
        public class GFFStruct
        {
            #region Properties

            ///<summary>Programmer-defined integer ID for the struct type. Varies from from File to file, though the Top-level struct (0) always has a type equal to 0xFFFFFFFF</summary>
            public int Type;
            ///<summary>
            ///<para>If FieldCount = 1, this is an index into the Field Array.</para>
            ///<para>If FieldCount > 1, this is a byte offset into the Field Indices array</para>
            /// </summary>
            public int DataOrDataOffset;
            ///<summary>Number of fields in this Struct</summary>
            public int FieldCount;
            ///<summary>The numeric Indexes of each field.</summary>
            public List<int> Field_Indexes = new List<int>();
            ///<summary>The data stored in this Struct, populated from DataOrDataOffset. Usually takes the form of a <see cref="Field"/> or <see cref="List{Field}"/></summary>
            public object StructData;

            #endregion

            #region Methods

            /// <summary>
            /// Adds the specified Field to the Struct
            /// </summary>
            /// <param name="field_index">The Index of the Field on the <see cref="GFF.Field_Array"/></param>
            /// <param name="g">The GFF that contains this Struct</param>
            public void add_field(int field_index, GFF g)
            {

                if (FieldCount == 0) { DataOrDataOffset = field_index; StructData = g.Field_Array[field_index]; }
                else if (FieldCount == 1)
                {
                    Field old = StructData as Field;
                    StructData = new List<Field>();
                    (StructData as List<Field>).Add(old);
                    (StructData as List<Field>).Add(g.Field_Array[field_index]);
                    Field_Indexes.Add(DataOrDataOffset);
                    Field_Indexes.Add(field_index);
                    DataOrDataOffset = g.ListIndicesOffset - g.FieldIndicesOffset; //Using this before the list offset is set to represent end of FieldIndices Array, where the new indices will be appended.
                    g.Field_Indices.Add(g.Field_Array.IndexOf(old));
                    g.Field_Indices.Add(field_index);
                    g.FieldIndicesCount += 8;
                    g.ListIndicesOffset += 8;
                }
                else
                {
                    (StructData as List<Field>).Add(g.Field_Array[field_index]);
                    Field_Indexes.Add(field_index);
                    int insert_index = (DataOrDataOffset / 4) + FieldCount;
                    g.Field_Indices.Insert(insert_index, field_index);

                    foreach (GFFStruct gs in g.Struct_Array.Where(s => s.DataOrDataOffset > DataOrDataOffset))
                    {
                        gs.DataOrDataOffset += 4;
                    }

                    g.FieldIndicesCount += 4;
                    g.ListIndicesOffset += 4;
                }

                FieldCount++;
            }

            /// <summary>
            /// Adds the specified Field to the Struct
            /// </summary>
            /// <param name="F"></param>
            /// <param name="g"></param>
            public void add_field(Field F, GFF g)
            {
                add_field(g.get_Field_Index(F), g);
            }

            /// <summary>
            /// Deletes a field from the Struct (Should not be called by end user)
            /// </summary>
            /// <param name="F">Field being removed</param>
            /// <param name="field_index">The Index of teh feild in <see cref="GFF.Field_Array"/></param>
            public void delete_field(Field F, int field_index)
            {
                FieldCount--;
                if (FieldCount > 1)
                {
                    (StructData as List<Field>).Remove(F);
                    Field_Indexes.Remove(field_index);
                }
                else if (FieldCount == 1)
                {
                    (StructData as List<Field>).Remove(F);
                    Field temp = (StructData as List<Field>).FirstOrDefault();
                    StructData = temp;
                    Field_Indexes.Remove(field_index);
                    DataOrDataOffset = Field_Indexes.FirstOrDefault();
                }
                else
                {
                    StructData = null;
                    DataOrDataOffset = 0;
                    //Struct should be marked for deletion
                }
            }

            #endregion
        }

        //Field Array
        /// <summary>
        /// A field is essentially a property or variable value stored within structs.
        /// </summary>
        public class Field
        {
            /// <summary> Data type. See: <see cref="Reference_Tables.Field_Types"/> </summary>
            public int Type;
            /// <summary> Index into the Label Array. </summary>
            public int LabelIndex;

            /// <summary>
            /// <para>If the field data type is not listed as complex, this is the actual value of the field</para>
            /// <para>If the field data type is listed as complex, this is an offset into another data block.</para>
            /// See:
            /// <see cref="Reference_Tables.Complex_Field_Types"/>,
            /// <see cref="Reference_Tables.Field_Types"/>
            /// </summary>
            public int DataOrDataOffset;

            /// <summary> This text is taken from <see cref="LabelIndex"/> in the <seealso cref="Label_Array"/>. </summary>
            public string Label;

            /// <summary> The actual data in the field of the type associated with <see cref="Type"/>, and populated from <see cref="DataOrDataOffset"/>. </summary>
            public object Field_Data;

            /// <summary> The string representation of <see cref="Type"/> from <see cref="Reference_Tables.Field_Types"/>. </summary>
            public string Type_Text;

            /// <summary> Whether or not the <see cref="Type"/> is complex according to <see cref="Reference_Tables.Complex_Field_Types"/>. </summary>
            public bool Complex;
        }

        //List Indices
        /// <summary>
        /// Describes a 'List' field as struct indexs prefixed with a 'size'
        /// </summary>
        public class List_Index
        {
            /// <summary> The number of <see cref="GFFStruct"/>s in the list. </summary>
            public int Size;
            /// <summary> Index vaules representing which <see cref="GFFStruct"/>s from <see cref="Struct_Array"/> are present in the list. </summary>
            public List<int> Indices = new List<int>();
        }

        //Complex Data Types
        /// <summary>
        /// A chacrter string prefixed with a size.
        /// </summary>
        public class CExoString
        {
            /// <summary> The length of the string. </summary>
            public int Size;
            /// <summary> The string, obatined from a <see cref="char"/> array of length <see cref="Size"/> </summary>
            public string Text;
        }

        /// <summary>
        /// Virtually identical to <see cref="CExoString"/>, however it is capped at a size of 16.
        /// </summary>
        public class CResRef
        {
            /// <summary> The length of the string. *This can be no larger than 16 </summary>
            public byte Size;
            /// <summary> The string, obatined from a <see cref="char"/> array of length <see cref="Size"/> </summary>
            public string Text; //Obtained from a char[] of size 'Size'
        }

        /// <summary>
        /// A set of localized string that contains language data in addition to the content of <see cref="CExoString"/>.
        /// </summary>
        public class CExoLocString
        {
            /// <summary> Total number of bytes in the object, not including these. </summary>
            public int Total_Size;
            /// <summary> The reference of the string into a relevant Talk table (<see cref="TLK"/>). If this is -1 it does not reference a string. </summary>
            public int StringRef;
            /// <summary> The number of <see cref="SubString"/>s contained. </summary>
            public int StringCount;
            /// <summary> The list containing the <see cref="SubString"/>s contained. </summary>
            public List<SubString> SubStringList = new List<SubString>();

            /// <summary>
            /// Nearly Identical to a <see cref="CExoString"/> be is prefixed by an ID.
            /// </summary>
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
        }

        /// <summary>
        /// A byte array prefixed with size.
        /// </summary>
        public class Void_Binary
        {
            /// <summary> The sive of teh void object in bytes </summary>
            public int Size;
            /// <summary> The raw byte data of the object, with a length of <see cref="Size"/> </summary>
            public byte[] Data;
        }

        /// <summary>
        /// Field Type used in later version of The Aurora Engine Contains four float values of currently unclear purpose.
        /// </summary>
        public class Orientation
        {
            /// <summary> Unknown float 1 </summary>
            public float float1;
            /// <summary> Unknown float 2 </summary>
            public float float2;
            /// <summary> Unknown float 3 </summary>
            public float float3;
            /// <summary> Unknown float 4 </summary>
            public float float4;
        }

        /// <summary>
        /// A 3-dimensional vector made of 3 single precision components
        /// </summary>
        public class Vector
        {
            /// <summary> The 'X' component of this vector </summary>
            public float x_component;
            /// <summary> The 'Y' component of this vector </summary>
            public float y_component;
            /// <summary> The 'Z' component of this vector </summary>
            public float z_component;
        }

        /// <summary>
        /// ???
        /// </summary>
        public class StrRef
        {
            /// <summary> An interger value of unknown purpose that appears to be always '4' </summary>
            public int leading_value;
            /// <summary> The integer reference to the string in question from dialogue.tlk </summary>
            public int reference;
        }

        #endregion
    }
}
