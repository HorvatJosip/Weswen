using Novacode;
using System.Collections.Generic;
using System.Reflection;

namespace Weswen
{
    /// <summary>
    /// Extension methods for <see cref="Table"/>.
    /// </summary>
    public static class TableExtensions
    {
        /// <summary>
        /// Removes all of the paragraphs in a table that are empty or just whitespace.
        /// Only exception is if the cell defines exact number of paragraphs.
        /// </summary>
        /// <param name="table">Table to remove empty paragraphs from.</param>
        /// <param name="tableCellProperties">Properties that define table cells.</param>
        public static void RemoveEmptyParagraphs(this Table table,
            IEnumerable<(PropertyInfo prop, TableCellAttribute cell)> tableCellProperties)
        {
            // Foreach row in the table...
            for (int i = 0; i < table.Rows.Count; i++)
                // Foreach cell in the row...
                for (int j = 0; j < table.Rows[i].Cells.Count; j++)
                    // Foreach paragraph in the cell...
                    foreach (var paragraph in table.Rows[i].Cells[j].Paragraphs)
                        // If it is null, empty or whitespace...
                        if (paragraph.Text.IsNullOrWhiteSpace())
                        {
                            // By default remove it
                            bool removeIt = true;

                            // Foreach table cell property...
                            foreach (var (prop, cell) in tableCellProperties)
                                // If a table cell property was defined for current cell...
                                if(cell.RowIndex == i && cell.ColumnIndex == j)
                                {
                                    // If the cell defines number of paragraphs to use...
                                    if (cell.NumParagraphs > 0)
                                        // Don't remove the paragraph
                                        removeIt = false;

                                    // Exit out of the current foreach
                                    break;
                                }

                            // If it should be removed...
                            if (removeIt)
                                // Remove it
                                paragraph.Remove(false);
                        }
        }

        /// <summary>
        /// Sets up the column widths (if present) and row merges (if present).
        /// </summary>
        /// <param name="table">Table to modify.</param>
        /// <param name="columnWidths">Widths of each column of a table.</param>
        /// <param name="tableCellProperties">Properties that define table cells.</param>
        public static void SetupColumnWidthsAndRowSpans(
            this Table table,
            double[] columnWidths,
            IEnumerable<(PropertyInfo prop, TableCellAttribute cell)> tableCellProperties
        )
        {
            // If the column widths were specified...
            if (columnWidths.IsNullOrEmpty() == false)
                // Loop through the columns
                for (int i = 0; i < table.ColumnCount && i < columnWidths.Length; i++)
                    // Set the width of the column
                    table.SetColumnWidth(i, columnWidths[i]);

            // If the table cell properties were defined...
            if (tableCellProperties.IsNullOrEmpty() == false)
                // Foreach table cell property...
                foreach (var (prop, cell) in tableCellProperties)
                    // If the cell defines a row span
                    if (cell.RowSpan > 1)
                        // Span as defined
                        table.MergeCellsInColumn(cell.ColumnIndex, cell.RowIndex, cell.RowSpanEnd);
        }
    }
}
