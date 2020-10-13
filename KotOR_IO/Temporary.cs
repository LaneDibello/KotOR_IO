using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KotOR_IO;

// Consideration of making GFF more friendly (Would require a total re-code)
//namespace KotOR_IO_Refracter
//{
//   public static class KReader
//    {
//        GFF.Field ReadField(int FIndex)
//        {
//            BinaryReader br = new BinaryReader(s)

//            br.BaseStream.Seek(FieldOffset + FIndex * 12, SeekOrigin.Begin);

//            int tempFtype = br.ReadInt32(); //Temporary storage of field type
//            int tempLabelIndex = br.ReadInt32();
//            byte[] tempData = br.ReadBytes(4);

//            switch (tempFtype)
//            {
//                case 0:
//                    GFF.FByte FB = new GFF.FByte(tempData[0], Label_Array[tempLabelIndex]);
//                    return FB;
//                case 1:
//                    GFF.FChar FC = new GFF.FChar((char)tempData[0], Label_Array[tempLabelIndex]);
//                    return FC;
//                case 2:
//                    GFF.FWord FW = new GFF.FWord(BitConverter.ToUInt16(tempData, 0), Label_Array[tempLabelIndex]);
//                    return FW;
//                case 3:
//                    GFF.FShort FS = new GFF.FShort(BitConverter.ToInt16(tempData, 0), Label_Array[tempLabelIndex]);
//                    return FS;
//                case 4:
//                    GFF.FDWord FDW = new GFF.FDWord(BitConverter.ToUInt32(tempData, 0), Label_Array[tempLabelIndex]);
//                    return FDW;
//                case 5:
//                    GFF.FInt FI = new GFF.FInt(BitConverter.ToInt32(tempData, 0), Label_Array[tempLabelIndex]);
//                    return FI;
//                case 6:
//                    br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//                    tempData = br.ReadBytes(8);
//                    GFF.FDWord64 FDW64 = new GFF.FDWord64(BitConverter.ToUInt64(tempData, 0), Label_Array[tempLabelIndex]);
//                    return FDW64;
//                case 7:
//                    br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//                    tempData = br.ReadBytes(8);
//                    GFF.FInt64 FI64 = new GFF.FInt64(BitConverter.ToInt64(tempData, 0), Label_Array[tempLabelIndex]);
//                    return FI64;
//                case 8:
//                    GFF.FFloat FF = new GFF.FFloat(BitConverter.ToSingle(tempData, 0), Label_Array[tempLabelIndex]);
//                    return FF;
//                case 9:
//                    br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//                    tempData = br.ReadBytes(8);
//                    GFF.FDouble FD = new GFF.FDouble(BitConverter.ToDouble(tempData, 0), Label_Array[tempLabelIndex]);
//                    return FD;
//                case 10:
//                    br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//                    int tempSize = br.ReadInt32();
//                    GFF.FCExoString FCES = new GFF.FCExoString(new string(br.ReadChars(tempSize)), Label_Array[tempLabelIndex]);
//                    return FCES;
//                case 11:
//                    br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//                    tempData = br.ReadBytes(1);
//                    GFF.FResRef FRR = new GFF.FResRef(new string(br.ReadChars(tempData[0])), Label_Array[tempLabelIndex]);
//                    return FRR;
//                case 12:
//                    br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//                    tempSize = br.ReadInt32();
//                    int tempRef = br.ReadInt32();
//                    int tempCount = br.ReadInt32();
//                    List<GFF.FCexoLocString.SubString> tempStringList = new List<GFF.FCexoLocString.SubString>();
//                    for (int k = 0; k < tempCount; k++)
//                    {
//                        int tempID = br.ReadInt32();
//                        int tempLength = br.ReadInt32();
//                        GFF.FCexoLocString.SubString SS = new GFF.FCexoLocString.SubString(tempID, new string(br.ReadChars(tempLength)));
//                        tempStringList.Add(SS);
//                    }
//                    GFF.FCexoLocString FCLS = new GFF.FCexoLocString(tempRef, Label_Array[tempLabelIndex], tempStringList.ToArray());
//                    return FCLS;
//                case 13:
//                    br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//                    tempSize = br.ReadInt32();
//                    GFF.FVoid FV = new GFF.FVoid(br.ReadBytes(tempSize), Label_Array[tempLabelIndex]);
//                    return FV;
//                case 14:
//                    br.BaseStream.Seek(StructOffset + BitConverter.ToInt32(tempData, 0) * 12, SeekOrigin.Begin);
//                    GFF.FStruct FSt = new GFF.FStruct(br.ReadInt32(), Label_Array[tempLabelIndex]);
//                    return FSt;
//                case 15:


//                    break;
//                case 16:

//                    break;
//                case 17:

//                    break;
//                case 18:

//                    break;
//                default:
//                    break;
//            }
//        }

//        public static GFF ReadGFF(Stream s)
//        {
//            GFF g = new GFF();

//            BinaryReader br = new BinaryReader(s);

//            //header
//            g.FileType = new string(br.ReadChars(4));
//            g.Version = new string(br.ReadChars(4));
//            int StructOffset = br.ReadInt32();
//            int StructCount = br.ReadInt32();
//            int FieldOffset = br.ReadInt32();
//            int FieldCount = br.ReadInt32();
//            int LabelOffset = br.ReadInt32();
//            int LabelCount = br.ReadInt32();
//            int FieldDataOffset = br.ReadInt32();
//            int FieldDataCount = br.ReadInt32();
//            int FieldIndicesOffset = br.ReadInt32();
//            int FieldIndicesCount = br.ReadInt32();
//            int ListIndicesOffset = br.ReadInt32();
//            int ListIndicesCount = br.ReadInt32();

//            br.BaseStream.Seek(LabelOffset, SeekOrigin.Begin);
//            //Label Array
//            List<string> Label_Array = new List<string>();
//            for (int i = 0; i < LabelCount; i++)
//            {
//                string l = new string(br.ReadChars(16)).TrimEnd('\0');
//                Label_Array.Add(l);
//            }

//            //ROUTINE START
//            br.BaseStream.Seek(StructOffset, SeekOrigin.Begin);
//            br.ReadBytes(8);
//            uint tempfcount = br.ReadUInt32();
//            for (int i = 0; i < tempfcount; i++)
//            {
//                br.BaseStream.Seek(FieldIndicesOffset + i * 4, SeekOrigin.Begin);
//                int FieldIndex = br.ReadInt32();


//            }

            

//            ////Field Array
//            //List<GFF.Field> Field_Array = new List<GFF.Field>();
//            //for (int i = 0; i < FieldCount; i++)
//            //{
//            //    br.BaseStream.Seek(FieldOffset + i * 12, SeekOrigin.Begin);

//            //    int tempFtype = br.ReadInt32(); //Temporary storage of field type
//            //    int tempLabelIndex = br.ReadInt32();
//            //    byte[] tempData = br.ReadBytes(4);

//            //    switch (tempFtype)
//            //    {
//            //        case 0:
//            //            GFF.FByte FB = new GFF.FByte(tempData[0], Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FB);
//            //            break;
//            //        case 1:
//            //            GFF.FChar FC = new GFF.FChar((char)tempData[0], Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FC);
//            //            break;
//            //        case 2:
//            //            GFF.FWord FW = new GFF.FWord(BitConverter.ToUInt16(tempData, 0), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FW);
//            //            break;
//            //        case 3:
//            //            GFF.FShort FS = new GFF.FShort(BitConverter.ToInt16(tempData, 0), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FS);
//            //            break;
//            //        case 4:
//            //            GFF.FDWord FDW = new GFF.FDWord(BitConverter.ToUInt32(tempData, 0), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FDW);
//            //            break;
//            //        case 5:
//            //            GFF.FInt FI = new GFF.FInt(BitConverter.ToInt32(tempData, 0), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FI);
//            //            break;
//            //        case 6: 
//            //            br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//            //            tempData = br.ReadBytes(8);
//            //            GFF.FDWord64 FDW64 = new GFF.FDWord64(BitConverter.ToUInt64(tempData, 0), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FDW64);
//            //            break;
//            //        case 7:
//            //            br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//            //            tempData = br.ReadBytes(8);
//            //            GFF.FInt64 FI64 = new GFF.FInt64(BitConverter.ToInt64(tempData, 0), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FI64);
//            //            break;
//            //        case 8:
//            //            GFF.FFloat FF = new GFF.FFloat(BitConverter.ToSingle(tempData, 0), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FF);
//            //            break;
//            //        case 9:
//            //            br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//            //            tempData = br.ReadBytes(8);
//            //            GFF.FDouble FD = new GFF.FDouble(BitConverter.ToDouble(tempData, 0), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FD);
//            //            break;
//            //        case 10:
//            //            br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//            //            int tempSize = br.ReadInt32();
//            //            GFF.FCExoString FCES = new GFF.FCExoString(new string(br.ReadChars(tempSize)), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FCES);
//            //            break;
//            //        case 11:
//            //            br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//            //            tempData = br.ReadBytes(1);
//            //            GFF.FResRef FRR = new GFF.FResRef(new string(br.ReadChars(tempData[0])), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FRR);
//            //            break;
//            //        case 12:
//            //            br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//            //            tempSize = br.ReadInt32();
//            //            int tempRef = br.ReadInt32();
//            //            int tempCount = br.ReadInt32();
//            //            List<GFF.FCexoLocString.SubString> tempStringList = new List<GFF.FCexoLocString.SubString>();
//            //            for (int k = 0; k < tempCount; k++)
//            //            {
//            //                int tempID = br.ReadInt32();
//            //                int tempLength = br.ReadInt32();
//            //                GFF.FCexoLocString.SubString SS = new GFF.FCexoLocString.SubString(tempID, new string(br.ReadChars(tempLength)));
//            //                tempStringList.Add(SS);
//            //            }
//            //            GFF.FCexoLocString FCLS = new GFF.FCexoLocString(tempRef, Label_Array[tempLabelIndex], tempStringList.ToArray());
//            //            Field_Array.Add(FCLS);
//            //            break;
//            //        case 13:
//            //            br.BaseStream.Seek(FieldDataOffset + BitConverter.ToInt32(tempData, 0), SeekOrigin.Begin);
//            //            tempSize = br.ReadInt32();
//            //            GFF.FVoid FV = new GFF.FVoid(br.ReadBytes(tempSize), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FV);
//            //            break;
//            //        case 14:
//            //            br.BaseStream.Seek(StructOffset + BitConverter.ToInt32(tempData, 0) * 12, SeekOrigin.Begin);
//            //            GFF.FStruct FSt = new GFF.FStruct(br.ReadInt32(), Label_Array[tempLabelIndex]);
//            //            Field_Array.Add(FSt);
//            //            break;
//            //        case 15:

//            //            break;
//            //        case 16:

//            //            break;
//            //        case 17:

//            //            break;
//            //        case 18:

//            //            break;
//            //        default:
//            //            break;
//            //    }
//            //}


//            //br.BaseStream.Seek(StructOffset, SeekOrigin.Begin);
//            ////Struct Array
//            //List<int> StructDatas = new List<int>(); //the Data/DataOffset info
//            //List<int> StructFcounts = new List<int>(); //The Field Counts for each struct
//            //for (int i = 0; i < StructCount; i++)
//            //{
//            //    GFF.FStruct FS = new GFF.FStruct();
//            //    FS.struct_type = br.ReadInt32();
//            //    StructDatas.Add(br.ReadInt32());
//            //    StructFcounts.Add(br.ReadInt32());
//            //}


//            return g;
//        }

//        public static class GFFFieldReader
//        {
//            public static ReadByte(Stream s, int index)
//            {
//                BinaryReader br = new BinaryReader(s);

//                br.BaseStream.Seek(FieldOffset + index * 12, SeekOrigin.Begin);

//                GFF.FByte FB = new GFF.FByte(tempData[0], Label_Array[tempLabelIndex]);
//                return FB;
//            }
//        }
//    }

//   public class GFF : KotOR_IO.KFile
//    {
//        public abstract class Field
//        {
//            public string Label;
//            public bool complex;
//            public int size;
//        }

//        public class FByte : Field
//        {
//            public const int type = 0;
//            public const bool complex = false;
//            public const int size = 1;

//            public byte Data;

//            public FByte(byte Data, string label)
//            {
//                this.Data = Data;
//                this.Label = label;
//            }

//            public FByte() { }
//        }
//        public class FChar : Field
//        {
//            public const int type = 1;
//            public const bool complex = false;
//            public const int size = 1;

//            public char Data;

//            public FChar(char Data, string label)
//            {
//                this.Data = Data;
//                this.Label = label;
//            }

//            public FChar() { }
//        }
//        public class FWord : Field
//        {
//            public const int type = 2;
//            public const bool complex = false;
//            public const int size = 2;

//            public ushort Data;

//            public FWord(ushort Data, string label)
//            {
//                this.Data = Data;
//                this.Label = label;
//            }

//            public FWord() { }
//        }
//        public class FShort : Field
//        {
//            public const int type = 3;
//            public const bool complex = false;
//            public const int size = 2;

//            public short Data;

//            public FShort(short Data, string label)
//            {
//                this.Data = Data;
//                this.Label = label;
//            }

//            public FShort() { }
//        }
//        public class FDWord : Field
//        {
//            public const int type = 4;
//            public const bool complex = false;
//            public const int size = 4;

//            public uint Data;

//            public FDWord(uint Data, string label)
//            {
//                this.Data = Data;
//                this.Label = label;
//            }

//            public FDWord() { }
//        }
//        public class FInt : Field
//        {
//            public const int type = 5;
//            public const bool complex = false;
//            public const int size = 4;

//            public int Data;

//            public FInt(int Data, string label)
//            {
//                this.Data = Data;
//                this.Label = label;
//            }

//            public FInt() { }
//        }
//        public class FDWord64 : Field
//        {
//            public const int type = 6;
//            public const bool complex = true;
//            public const int size = 8;

//            public ulong Data;

//            public FDWord64(ulong Data, string label)
//            {
//                this.Data = Data;
//                this.Label = label;
//            }

//            public FDWord64() { }
//        }
//        public class FInt64 : Field
//        {
//            public const int type = 7;
//            public const bool complex = true;
//            public const int size = 8;

//            public long Data;

//            public FInt64(long Data, string label)
//            {
//                this.Data = Data;
//                this.Label = label;
//            }

//            public FInt64() { }
//        }
//        public class FFloat : Field
//        {
//            public const int type = 8;
//            public const bool complex = false;
//            public const int size = 4;

//            public float Data;

//            public FFloat(float Data, string label)
//            {
//                this.Data = Data;
//                this.Label = label;
//            }

//            public FFloat() { }
//        }
//        public class FDouble : Field
//        {
//            public const int type = 9;
//            public const bool complex = true;
//            public const int size = 8;

//            public double Data;

//            public FDouble(double Data, string label)
//            {
//                this.Data = Data;
//                this.Label = label;
//            }

//            public FDouble() { }
//        }
//        public class FCExoString : Field
//        {
//            public const int type = 10;
//            public const bool complex = true;

//            /// <summary>The length of the string.</summary>
//            public int size;
//            /// <summary>The string, obatined from a <see cref="char"/> array of length <see cref="size"/></summary>
//            public string Data
//            {
//                get
//                {
//                    return Data;
//                }
//                set
//                {
//                    Data = value;
//                    size = value.Length;
//                }
//            }

//            public FCExoString(string Data, string label)
//            {
//                this.Data = Data;
//                size = Data.Length;
//                this.Label = label;
//            }

//            public FCExoString() { }
//        }
//        public class FResRef : Field
//        {
//            public const int type = 11;
//            public const bool complex = true;
//            public const int size = 16;

//            /// <summary>The string, obatined from a <see cref="char"/> array of length <see cref="size"/></summary>
//            public string Data
//            {
//                get
//                {
//                    return Data;
//                }
//                set
//                {
//                    if (value.Length > 16)
//                    {
//                        Data = value.Substring(0, 16);
//                    }
//                    else
//                    {
//                        Data = value;
//                    }
//                }
//            } 

//            public FResRef(string Data, string label)
//            {
//                if (Data.Length > 16)
//                {
//                    this.Data = Data.Substring(0, 16);
//                }
//                else
//                {
//                    this.Data = Data;
//                }
//                this.Label = label;
//            }

//            public FResRef() { }
//        }
//        public class FCexoLocString : Field
//        {
//            public const int type = 12;
//            public const bool complex = true;

//            //NOTE: SIZE NEEDS SPECIAL HANDLING ON WRITE
//            public int size;

//            /// <summary>The reference of the string into a relevant Talk table (<see cref="TLK"/>). If this is -1 it does not reference a string.</summary>
//            public int StringRef;

//            /// <summary>Nearly Identical to a <see cref="CExoString"/> be is prefixed by an ID.</summary>
//            public class SubString
//            {
//                /// <summary>An identify that is calulated by multiplying the <see cref="Reference_Tables.Language_IDs"/> ID by 2, and adding 1 if the speaker if feminine.
//                /// <para/>For example, a line spoken by an Italien Male would have an ID of 6
//                /// </summary>
//                public int StringID;
//                /// <summary>The string contained</summary>
//                public string Text;

//                public SubString(int StringID, string Text)
//                {
//                    this.StringID = StringID;
//                    this.Text = Text;
//                }
//            }

//            /// <summary>The list containing the <see cref="SubString"/>s contained.</summary>
//            public List<SubString> Data = new List<SubString>();

//            public FCexoLocString(int StringRef, string label, SubString[] Data)
//            {
//                this.StringRef = StringRef;
//                this.Data.AddRange(Data);
//                this.Label = label;

//                size = 12;
//                foreach (SubString ss in Data)
//                {
//                    size += (ss.Text.Length + 8);
//                }
//            }

//            public FCexoLocString() { }
//        }
//        public class FVoid : Field
//        {
//            public const int type = 13;
//            public const bool complex = true;

//            /// <summary>The sive of the void object in bytes</summary>
//            public int size;
//            /// <summary>The raw byte data of the object, with a length of <see cref="size"/></summary>
//            public byte[] Data
//            {
//                get { return Data; }
//                set
//                {
//                    Data = value;
//                    size = value.Count();
//                }
//            }

//            public FVoid(byte[] data, string label)
//            {
//                size = data.Count();
//                Data = data;
//                this.Label = label;
//            }

//            public FVoid() { }
//        }
//        public class FStruct : Field
//        {
//            public const int type = 14;
//            public const bool complex = true;

//            public int struct_type;
//            //Figure out size

//            private List<Field> Data = new List<Field>();

//            public void add_field(Field f)
//            {
//                Data.Add(f);
//            }

//            public void delete_field(string label, int occurance)
//            {
//                Data.Remove(Data.Where(f => f.Label.TrimEnd('\0') == label.TrimEnd('\0')).ToArray()[occurance]);
//            }

//            public Field this[int index]
//            {
//                get
//                {
//                    return Data[index];
//                }

//                set
//                {
//                    Data[index] = value;
//                }
//            }

//            public IEnumerable<Field> AsEnum()
//            {
//                foreach (Field f in Data)
//                {
//                    yield return f;
//                }
//            }

//            public FStruct(int type, string label, params Field[] fields)
//            {
//                struct_type = type;
//                Data.AddRange(fields);
//                this.Label = label;
//            }

//            public FStruct() { }
//        }
//        public class FList : Field
//        {
//            public const int type = 15;
//            public const bool complex = true;

//            public List<FStruct> Data = new List<FStruct>();

//            public FList(FStruct[] Data, string label)
//            {
//                this.Data.AddRange(Data);
//                this.Label = label;
//            }

//            public FList() { }
//        }
//        public class FOrientation : Field
//        {
//            public const int type = 16;
//            public const bool complex = true;
//            public const int size = 16;

//            /// <summary> Unknown float 1 </summary>
//            public float float1;
//            /// <summary> Unknown float 2 </summary>
//            public float float2;
//            /// <summary> Unknown float 3 </summary>
//            public float float3;
//            /// <summary> Unknown float 4 </summary>
//            public float float4;

//            public FOrientation(float float1, float float2, float float3, float float4, string label)
//            {
//                this.float1 = float1;
//                this.float2 = float2;
//                this.float3 = float3;
//                this.float4 = float4;
//                this.Label = label;
//            }

//            public FOrientation() { }
//        }
//        public class FVector : Field
//        {
//            public const int type = 17;
//            public const bool complex = true;
//            public const int size = 12;

//            /// <summary>The 'X' component of this vector</summary>
//            public float x_component;
//            /// <summary>The 'Y' component of this vector</summary>
//            public float y_component;
//            /// <summary>The 'Z' component of this vector</summary>
//            public float z_component;

//            public FVector(float x, float y, float z, string label)
//            {
//                x_component = x;
//                y_component = y;
//                z_component = z;
//                this.Label = label;
//            }

//            public FVector() { }
//        }
//        public class FStrRef : Field
//        {
//            public const int type = 18;
//            public const bool complex = true;
//            public const int size = 8;

//            /// <summary>An interger value of unknown purpose that appears to be always '4'</summary>
//            public int leading_value;
//            /// <summary>The integer reference to the string in question from dialogue.tlk</summary>
//            public int reference;

//            public FStrRef(int lead, int refe, string label)
//            {
//                leading_value = lead;
//                reference = refe;
//                this.Label = label;
//            }

//            public FStrRef() { }
//        }

//        public FStruct Top = new FStruct(-1, "");

//        public void add_field(Field f)
//        {
//            Top.add_field(f);
//        }

//        public void delete_field(Field f, int occurance)
//        {
//            Top.delete_field(f.Label, occurance);
//        }
//    }
//}
