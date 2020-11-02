using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    /// <summary>
    /// <para>BioWare 2-Dimensional Array Data</para>
    /// See: <see cref="TwoDA(Stream, string)"/>, 
    /// <seealso  cref="Write(Stream)"/>
    /// <remarks>
    /// <para>2DA data is generally presented in a spreadsheet format. It is used by the game engine to reference various values and constants for task ranging from name generation, to item properties.</para>
    /// <para>*NOTE: This program is currently only compatible with 2DA version 2.b files.</para>
    /// </remarks>
    /// </summary>
    public class TwoDA : KFile
    {
        /// <summary> String used as the key for the row index. </summary>
        private readonly string ROW_INDEX_KEY = "row_index";

        /// <summary>
        /// Initiates a new instance of the <see cref="TwoDA"/> class.
        /// </summary>
        /// <param name="name">The resource reference string for this 2DA</param>
        private TwoDA(string name = null)
        {
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawData">A byte array containing the file data.</param>
        /// <param name="name">The resource reference string for this 2DA</param>
        public TwoDA(byte[] rawData, string name = null)
            : this(new MemoryStream(rawData), name)
        { }

        /// <summary>
        /// Reads the given BioWare BIF File.
        /// </summary>
        /// <param name="path">File path to read.</param>
        /// <param name="name">The resource reference string for this 2DA</param>
        public TwoDA(string path, string name = null)
            : this(File.OpenRead(path), name)
        { }

        /// <summary>
        /// Reads Bioware 2-Dimensional Array (v2.b) files.
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        /// <param name="name">The resource reference string for this 2DA</param>
        protected TwoDA(Stream s, string name = null)
            : this(name)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                // Get header info.
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));

                br.ReadByte();

                // Get Column Labels.
                StringBuilder sb = new StringBuilder();

                while (br.PeekChar() != ASCII_NULL)
                {
                    sb.Clear();

                    // May have to make this go one past the current limit.
                    while (br.PeekChar() != ASCII_TAB)
                    {
                        sb.Append(br.ReadChar());
                    }
                    Columns.Add(sb.ToString());
                    br.ReadByte();
                }

                br.ReadByte();
                RowCount = br.ReadInt32();  // Get row count.

                // Skip row indexes (maybe a bad idea, but who cares).
                for (int i = 0; i < RowCount; i++)
                {
                    while (br.PeekChar() != ASCII_TAB)
                    {
                        br.ReadByte();
                    }
                    br.ReadByte();
                }

                // Generate index column.
                List<string> index_list = new List<string>();
                for (int i = 0; i < RowCount; i++) { index_list.Add(Convert.ToString(i)); }
                Data.Add(ROW_INDEX_KEY, index_list);

                // Populate column keys.
                foreach (string c in Columns)
                {
                    List<string> tempColumn = new List<string>();
                    Data.Add(c, tempColumn);
                }

                var offsets = new List<short>();

                // Get offsets. Iterates through the number of cells.
                for (int i = 0; i < (1 + (RowCount * Columns.Count())); i++)
                {
                    offsets.Add(br.ReadInt16());
                }
                int DataOffset = (int)br.BaseStream.Position;

                // Populate data.
                int OffsetIndex = 0;
                for (int i = 0; i < RowCount; i++)
                {
                    for (int k = 0; k < Columns.Count(); k++)
                    {
                        br.BaseStream.Seek(DataOffset + offsets[OffsetIndex], SeekOrigin.Begin);
                        sb.Clear();
                        while (br.PeekChar() != ASCII_NULL)
                        {
                            sb.Append(br.ReadChar());
                        }
                        Data[Columns[k]].Add(sb.ToString());
                        br.ReadByte();
                        OffsetIndex++;
                    }
                }
            }
        }

        /// <summary> The resource reference string for this 2DA </summary>
        public string Name { get; }

        /// <summary> List of Column Headers. Generally used as the keys for Data </summary>
        public List<string> Columns { get; set; } = new List<string>();

        /// <summary> The Number of rows in the array </summary>
        public int RowCount { get; set; } = 0;

        ///// <summary> A list of data offsets, one for each cell of the array </summary>
        //public List<short> Offsets { get; set; } = new List<short>();

        /// <summary> The Full 2D-Array with columns for keys, rows for values that each indexe from 0 to RowCount - 1.
        /// The first column with <c>"row_indexs"</c> is an index of each row. </summary>
        public Dictionary<string, List<string>> Data { get; set; } = new Dictionary<string, List<string>>();

        ///// <summary> Denotes rather or not the default string data has been parsed to numerical data where appropriate. </summary>
        //public bool IsParsed { get; set; } = false;

        /// <summary>
        /// Indexer for 2DA data
        /// </summary>
        /// <param name="columnLabel">The label of the column in the <see cref="TwoDA"/>.</param>
        /// <param name="rowIndex">The index of the row in <see cref="Data"/>.</param>
        public string this[string columnLabel, int rowIndex] //maybe switch first vaule to column
        {
            get
            {
                if (Data.Keys.Contains(columnLabel) && rowIndex < RowCount)
                {
                    return Data[columnLabel][rowIndex];
                }
                else
                {
                    throw new IndexOutOfRangeException("Column label and row index must exist in the 2DA.");
                }
            }
            //set
            //{
            //    if (Data.Keys.Contains(columnLabel) && rowIndex < RowCount)
            //    {
            //        short offset = Offsets[Columns.IndexOf(columnLabel) + rowIndex * Columns.Count];
            //        object oldValue = Data[columnLabel][rowIndex];
            //        Data[columnLabel][rowIndex] = value;
            //        //int offsetDifference = Convert.ToString(value).Length - Convert.ToString(oldValue).Length;
            //        int offsetDifference = GetDataSize(value) - GetDataSize(oldValue);

            //        // Could simplify using a query.
            //        for (int i = 0; i < Offsets.Count; i++)
            //        {
            //            if (Offsets[i] > offset)
            //            {
            //                Offsets[i] += (short)offsetDifference;
            //            }
            //        }

            //    }
            //    else
            //    {
            //        throw new IndexOutOfRangeException("Column Label and row index must exist in the 2DA.");
            //    }
            //}
        }

        /// <summary>
        /// Gets the size of the object as it would be stored in a file.
        /// </summary>
        /// <returns>Length of the object in string format</returns>
        private static int GetDataSize(object o)
        {
            return Convert.ToString(o).Length;
        }

        /// <summary>
        /// Writes BioWare 2-Dimensional Array (v2.b) data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        internal override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                // Header
                bw.Write(FileType.ToArray());
                bw.Write(Version.ToArray());
                bw.Write(ASCII_NEWLINE);

                // Column Labels
                foreach (string c in Columns)
                {
                    bw.Write(c.ToArray());
                    bw.Write(ASCII_TAB);
                }
                bw.Write(ASCII_NULL);

                // Row Count
                bw.Write(RowCount);

                // Row Indices
                for (int i = 0; i < RowCount; i++)
                {
                    bw.Write(Convert.ToString(i).ToArray());
                    bw.Write(ASCII_TAB);
                }

                // Offsets
                List<string> allValues = new List<string>();
                foreach (string c in Columns)
                {
                    allValues.AddRange(Data[c].Distinct());
                }
                allValues = allValues.Distinct().ToList();

                Dictionary<string, short> offsetTable = new Dictionary<string, short>();

                short currentOffset = 0;
                foreach (string v in allValues)
                {
                    offsetTable.Add(v, currentOffset);
                    currentOffset += (short)v.Length;
                    currentOffset++;
                }

                for (int i = 0; i < RowCount; i++)
                {
                    foreach (string c in Columns)
                    {
                        bw.Write(offsetTable[Data[c][i]]);
                    }
                }
                bw.Write(currentOffset);

                foreach (string k in offsetTable.Keys)
                {
                    bw.Write(k.ToArray());
                    bw.Write('\0');
                }
            }
        }

        /// <summary>
        /// Writes a file to the given directory using the Name property in this class object.
        /// </summary>
        /// <param name="directory">Directory to write a file to.</param>
        public void WriteToDirectory(string directory)
        {
            var path = Path.Combine(directory, $"{Name}.2da");
            Write(File.OpenWrite(path));
        }
    }
}
