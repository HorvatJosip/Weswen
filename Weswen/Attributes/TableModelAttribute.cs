using System;

using Novacode;

namespace Weswen
{
    /// <summary>
    /// Defines that this class or struct represents a Word Document table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class TableModelAttribute : Attribute
    {
        /// <summary>
        /// Defines a table in the Word Document.
        /// </summary>
        /// <param name="numRows">Number of rows that the table will have. Minimum: 1.</param>
        /// <param name="numColumns">Number of columns that the table will have. Minimum: 1.</param>
        public TableModelAttribute(int numRows, int numColumns)
        {
            NumRows = Math.Max(1, numRows);
            NumColumns = Math.Max(1, numColumns);
        }

        /// <summary>
        /// Number of rows that the table will have.
        /// </summary>
        public int NumRows { get; }

        /// <summary>
        /// Number of columns that the table will have.
        /// </summary>
        public int NumColumns { get; }

        /// <summary>
        /// Widths of the columns.
        /// </summary>
        public double[] ColumnWidths { get; set; }

        /// <summary>
        /// Defines the alignment of the table.
        /// </summary>
        public Alignment Alignment { get; set; } = Alignment.center;

        /// <summary>
        /// How many paragraphs should be added after the table.
        /// </summary>
        public int NumParagraphsAfter { get; set; }

        /// <summary>
        /// If there are multiple tables, after how many tables should there be a page break?
        /// Needs to be at least 1 for this feature to be included.
        /// </summary>
        public int PageBreakAfter { get; set; }
    }
}
