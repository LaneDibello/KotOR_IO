using System;
using System.Collections.Generic;

namespace KotOR_IO
{
    public partial class GFF
    {
        /// <summary>
        /// Abstract class that all the fields that make the GFF are based off of.
        /// </summary>
        public abstract class FIELD
        {
            /// <summary>
            /// Maximum length of the string Label.
            /// </summary>
            public const int MAX_LABEL_LENGTH = 16;

            /// <summary>
            /// Type of this GFF field.
            /// </summary>
            public GffFieldType Type { get; set; }

            /// <summary>
            /// String label.
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// Collect fields recursively.
            /// </summary>
            /// <param name="Field_Array"></param>
            /// <param name="Raw_Field_Data_Block"></param>
            /// <param name="Label_Array"></param>
            /// <param name="Struct_Indexer"></param>
            /// <param name="List_Indices_Counter"></param>
            abstract internal void collect_fields(
                ref List<Tuple<FIELD, int, int>> Field_Array,
                ref List<byte> Raw_Field_Data_Block,
                ref List<string> Label_Array,
                ref int Struct_Indexer,
                ref int List_Indices_Counter);

            internal FIELD(GffFieldType type, string label = "")
            {
                if (label.Length > MAX_LABEL_LENGTH)
                    throw new Exception($"Label \"{label}\" is longer than {MAX_LABEL_LENGTH} characters, and is invalid.");

                Type = type;
                Label = label;
            }

            /// <summary>
            /// Write FIELD information to string.
            /// </summary>
            /// <returns>[Type] "Label", {specific field info}</returns>
            public override string ToString()
            {
                return $"[{Type}] \"{Label}\"";
            }

            /// <summary>
            /// Test equality between two FIELD objects.
            /// </summary>
            /// <returns>True if the FIELDs contain the same data.</returns>
            public override bool Equals(object right)
            {
                // Check null
                if (right is null)
                    return false;

                // Check self
                if (object.ReferenceEquals(this, right))
                    return true;

                // Check type
                if (this.GetType() != right.GetType())
                    return false;

                // Check GFF Type
                var other = right as FIELD;
                if (Type != other.Type)
                    return false;

                // Check Label
                if (Label != other.Label)
                    return false;

                // All checks passed
                return true;
            }

            /// <summary>
            /// Get default reference hash code as base.
            /// </summary>
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}