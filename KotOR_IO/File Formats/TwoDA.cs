using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO.File_Formats
{
    /// <summary>
    /// <para>BioWare 2-Dimensional Array Data</para>
    /// See: <see cref="TwoDA(Stream)"/>, 
    /// <seealso  cref="Write(Stream)"/>
    /// <remarks>
    /// <para>2DA data is generally presented in a spreadsheet format. It is used by the game engine to reference various values and constants for task ranging from name generation, to item properties.</para>
    /// <para>*NOTE: This program is currently only compatible with 2DA version 2.b files.</para>
    /// </remarks>
    /// </summary>
    public class TwoDA : KFile
    {
        #region Constants
        private readonly string ROW_INDEX_KEY = "row_index";
        private const byte BYTE_0 = 0;   // Should use a name that represents its usage.
        private const byte BYTE_9 = 9;   // Should use a name that represents its usage.
        private const byte BYTE_10 = 10; // Should use a name that represents its usage.
        #endregion

        #region Constructors

        /// <summary>
        /// Initiates a new instance of the <see cref="TwoDA"/> class.
        /// </summary>
        public TwoDA() { }

        /// <summary>
        /// Initiates a new instance of the <see cref="TwoDA"/> class, given column names and 2-deminsional data.
        /// </summary>
        /// <param name="columns">A string array containing the names of the columns</param>
        /// <param name="data">The 2-dimensional data</param>
        /// <param name="isParsed">Whether the data has been parsed into different formats. If False, then all data is in string format.</param>
        public TwoDA(string[] columns, object[,] data, bool isParsed)
        {
            if (columns.Length != data.GetLength(1))
            {
                throw new IndexOutOfRangeException("Length of columns should be equal to the number of columns in data.");
            }

            // Header info
            FileType = "2DA ";
            Version = "V2.b";

            // Columns
            Columns.AddRange(columns);

            // Row count
            RowCount = data.GetLength(0);

            // Offsets
            List<object> UniqueValues = new List<object>();
            List<int> IndexOffsets = new List<int>();

            int totaloffset = 0;
            foreach (object o in data)
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

            // Data
            // Generate index column
            List<object> index_list = new List<object>();
            for (int i = 0; i < RowCount; i++) { index_list.Add(Convert.ToString(i)); }
            Data.Add("row_index", index_list);

            foreach (string c in columns)
            {
                List<object> tempCol = new List<object>();
                int colIndex = Array.IndexOf(columns, c);

                for (int i = 0; i < RowCount; i++)
                {
                    tempCol.Add(data[i, colIndex]);
                }

                Data.Add(c, tempCol);
            }

            // Parsing
            IsParsed = isParsed;
        }

        /// <summary>
        /// Reads Bioware 2-Dimensional Array (v2.b) files
        /// </summary>
        /// <param name="s">The Stream from which the File will be Read</param>
        public TwoDA(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                // Get header info
                FileType = new string(br.ReadChars(4));
                Version = new string(br.ReadChars(4));

                br.ReadByte();

                // Get Column Labels
                StringBuilder sb = new StringBuilder();

                while (br.PeekChar() != 0)
                {
                    sb.Clear();
                    while (br.PeekChar() != 9) // May have to make this go one past the current limit
                    {
                        sb.Append(br.ReadChar());
                    }
                    Columns.Add(sb.ToString());
                    br.ReadByte();
                }

                br.ReadByte();

                // Get row count
                RowCount = br.ReadInt32();

                // Skip row indexes (maybe a bad idea, but who cares)
                for (int i = 0; i < RowCount; i++)
                {
                    while (br.PeekChar() != 9)
                    {
                        br.ReadByte();
                    }
                    br.ReadByte();
                }

                // Generate index column
                List<object> index_list = new List<object>();
                for (int i = 0; i < RowCount; i++) { index_list.Add(Convert.ToString(i)); }
                Data.Add(ROW_INDEX_KEY, index_list);

                // Populate column keys
                foreach (string c in Columns) { List<object> tempColumn = new List<object>(); Data.Add(c, tempColumn); }

                // Get offsets
                for (int i = 0; i < (1 + (RowCount * Columns.Count())); i++) // iterates through the number of cells
                {
                    Offsets.Add(br.ReadInt16());
                }
                int DataOffset = (int)br.BaseStream.Position;

                // Populate data
                int OffsetIndex = 0;
                for (int i = 0; i < RowCount; i++)
                {
                    for (int k = 0; k < Columns.Count(); k++)
                    {
                        br.BaseStream.Seek(DataOffset + Offsets[OffsetIndex], SeekOrigin.Begin);
                        sb.Clear();
                        while (br.PeekChar() != 0)
                        {
                            sb.Append(br.ReadChar());
                        }
                        Data[Columns[k]].Add(sb.ToString());
                        br.ReadByte();
                        OffsetIndex++;
                    }
                }
                IsParsed = false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the size of the object as it would be stored in a file.
        /// </summary>
        /// <returns>Length of the object in string format</returns>
        private static int GetDataSize(object o)
        {
            return Convert.ToString(o).Length;
        }

        /// <summary>
        /// Parses the default string data into either <see cref="int"/>, <see cref="float"/>, hex data, or <see cref="string"/> depending on each column's contents.
        /// </summary>
        public void ParseData()
        {
            if (!IsParsed)
            {
                int IScrap = 0;
                float IFcrap = 0;
                foreach (List<object> column in Data.Values)
                {
                    int i = 0;
                    while (column[i] as string == "") { i++; if (i >= column.Count) { break; } } // iterate to the first non null value
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

        /// <summary>
        /// Indexer for 2DA data
        /// </summary>
        /// <param name="columnLabel">The label of the column in the <see cref="TwoDA"/>.</param>
        /// <param name="rowIndex">The index of the row in <see cref="Data"/>.</param>
        public object this[string columnLabel, int rowIndex] //maybe switch first vaule to column
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
            set
            {
                if (Data.Keys.Contains(columnLabel) && rowIndex < RowCount)
                {
                    short offset = Offsets[Columns.IndexOf(columnLabel) + rowIndex * Columns.Count];
                    object oldValue = Data[columnLabel][rowIndex];
                    Data[columnLabel][rowIndex] = value;
                    //int offsetDifference = Convert.ToString(value).Length - Convert.ToString(oldValue).Length;
                    int offsetDifference = GetDataSize(value) - GetDataSize(oldValue);

                    // Could simplify using a query.
                    for (int i = 0; i < Offsets.Count; i++)
                    {
                        if (Offsets[i] > offset)
                        {
                            Offsets[i] += (short)offsetDifference;
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
        /// <param name="label">The Label or Header for the collumn</param>
        /// <param name="data">The list of objects to be seeded in the collumn</param>
        public void AddColumn(string label, object[] data)
        {
            if (data.Length > RowCount) { throw new IndexOutOfRangeException("Data extends beyond Row_Count"); }
            List<object> tmpCol = new List<object>(data);
            Data.Add(label, tmpCol);
            Columns.Add(label);
            Offsets.Clear();

            //Offsets
            List<object> UniqueValues = new List<object>();
            List<int> IndexOffsets = new List<int>();

            int totaloffset = 0;
            for (int row = 0; row < Data[ROW_INDEX_KEY].Count; row++)
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
        /// Writes BioWare 2-Dimensional Array (v2.b) data
        /// </summary>
        /// <param name="s">The Stream to which the File will be written</param>
        public override void Write(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s))
            {
                //Header
                bw.Write(FileType.ToArray());
                bw.Write(Version.ToArray());

                bw.Write((byte)10);

                //Column Labels
                foreach (string c in Columns)
                {
                    bw.Write(c.ToArray());
                    bw.Write((byte)9);
                }

                bw.Write((byte)0);

                //Row Count
                bw.Write(RowCount);

                //Row Indexs
                for (int i = 0; i < RowCount; i++)
                {
                    bw.Write(Convert.ToString(i).ToArray());
                    bw.Write((byte)9);
                }

                //Offsets
                foreach (short sh in Offsets)
                {
                    bw.Write(sh);
                }

                int DataOffset = (int)bw.BaseStream.Position;

                //Data
                List<short> CheckedOffsets = new List<short>();
                int row_index = 0;
                int col_index = 0;
                foreach (short sh in Offsets)
                {
                    if (!CheckedOffsets.Contains(sh))
                    {
                        string tempData = Convert.ToString(Data[Columns[col_index]][row_index]);
                        bw.Seek(DataOffset + sh, SeekOrigin.Begin);
                        bw.Write(tempData.ToArray());
                        bw.Write((byte)0);
                        CheckedOffsets.Add(sh);
                    }
                    col_index++;

                    if (col_index == Columns.Count)
                    {
                        col_index = 0;
                        row_index++;
                        if (row_index == RowCount) { break; }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a row to the 2DA
        /// </summary>
        /// <param name="data">The Data to be seeded to that row (from left to right).</param>
        public void AddRow(object[] data)
        {
            if (data.Length > Columns.Count) { throw new IndexOutOfRangeException("Data contains more columns than are present in the 2DA."); }
            Data[ROW_INDEX_KEY].Add(RowCount);
            RowCount++;
            int colIndex = 0;
            foreach (object o in data)
            {
                Data[Columns[colIndex]].Add(o);
                colIndex++;
            }
            Offsets.Clear();

            //Offsets
            List<object> UniqueValues = new List<object>();
            List<int> IndexOffsets = new List<int>();

            int totaloffset = 0;
            for (int row = 0; row < Data[ROW_INDEX_KEY].Count; row++)
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

        #region Properties

        // FileType & Version in superclass

        /// <summary>List of Column Headers. Generally used as the keys for Data</summary>
        public List<string> Columns { get; set; } = new List<string>();

        /// <summary>The Number of rows in the array</summary>
        public int RowCount { get; set; } = 0;

        /// <summary>A list of data offsets, one for each cell of the array</summary>
        public List<short> Offsets { get; set; } = new List<short>();

        /// <summary>The Full 2D-Array with columns for keys, rows for values that each indexe from 0 to RowCount - 1.
        /// The first column with <c>"row_indexs"</c> is an index of each row.</summary>
        public Dictionary<string, List<object>> Data { get; set; } = new Dictionary<string, List<object>>();

        /// <summary>Denotes rather or not the default string data has been parsed to numerical data where appropriate.</summary>
        public bool IsParsed { get; set; } = false;

        #endregion
    }
}
