using System;

namespace Weswen
{
    /// <summary>
    /// Defines that this property is a cell in a Word Document table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TableCellAttribute : Attribute
    {
        private int rowSpan, colSpan;

        /// <summary>
        /// Defines a cell in the table.
        /// </summary>
        /// <param name="row">Row position in the table. Minimum: 1.</param>
        /// <param name="column">Column position in the table. Minimum: 1.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public TableCellAttribute(int row, int column)
        {
            Row = Math.Max(1, row);
            Column = Math.Max(1, column);
        }

        #region Properties

        #region Content

        /// <summary>
        /// Once the paragraphs are being read, should the <see cref="Environment.NewLine"/>
        /// be appended at the end of the paragraph?
        /// </summary>
        public bool AddNewLinesPerParagraph { get; set; }

        /// <summary>
        /// Defines how many characters can this cell take before splitting into multiple paragraphs.
        /// Needs to be at least 1 for this feature to be included.
        /// </summary>
        public int MaxRowCharacters { get; set; }

        /// <summary>
        /// Optional property for setting the already known content.
        /// If this is null, property's value is used.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Collection of strings in case the content needs to be split into
        /// multiple paragraphs.
        /// </summary>
        public string[] Paragraphs { get; set; }

        /// <summary>
        /// Number of paragraphs to have in this cell no mather what.
        /// Needs to be at least 1 for this feature to be included.
        /// </summary>
        public int NumParagraphs { get; set; }

        /// <summary>
        /// Strings that will affect the <see cref="NumParagraphs"/>. For example,
        /// by default, "\n" will be in here, so if <see cref="NumParagraphs"/> is
        /// 5 and there are 2 "\n"'s found, only 3 blank paragraphs will be added.
        /// </summary>
        public string[] NumParagraphsExceptions { get; set; } = new[] { "\n" };

        /// <summary>
        /// Allows insertion of the content at the desired location.
        /// Example: "${0}", {0} will be replaced by the content (of <see cref="Content"/> or property value).
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Should the value of the property be used if it is available even
        /// if the <see cref="Content"/> is defined?
        /// </summary>
        public bool PropertyValueOverAttributeValue { get; set; }

        #endregion

        #region Position

        /// <summary>
        /// Row position in the table.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Row position in the table by index.
        /// </summary>
        public int RowIndex => Row - 1;

        /// <summary>
        /// Column position in the table.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Column position in the table by index.
        /// </summary>
        public int ColumnIndex => Column - 1;

        /// <summary>
        /// How many rows this cell takes (merges with). Minimum: 1.
        /// </summary>
        public int RowSpan { get => rowSpan; set => rowSpan = Math.Max(1, value); }

        /// <summary>
        /// Gets the end position for the row span.
        /// </summary>
        public int RowSpanEnd => RowIndex + RowSpan - 1;

        /// <summary>
        /// How many columns this cell takes (merges with). Minimum: 1.
        /// </summary>
        public int ColumnSpan { get => colSpan; set => colSpan = Math.Max(1, value); }

        /// <summary>
        /// Gets the end position for the column span.
        /// </summary>
        public int ColumnSpanEnd => ColumnIndex + ColumnSpan - 1;

        #endregion

        #region Style

        ///// <summary>
        ///// Defines how should the cell be styled.
        ///// example: "Size: 15; Bold: True; Position: 8"
        ///// </summary>
        //public string Style { get; set; }

        /// <summary>
        /// Size of the content inside the cell.
        /// </summary>
        public double TextSize { get; set; } = 12;

        /// <summary>
        /// Should the text be bold?
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Spacing between the characters of the cell content.
        /// </summary>
        public int TextSpacing { get; set; } = 8;

        #endregion 

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the content in the cell should wrap.
        /// </summary>
        /// <param name="contentLength">Length of the cell content.</param>
        /// <returns></returns>
        public bool ShouldWrap(int contentLength)
            => MaxRowCharacters > 0 && contentLength > MaxRowCharacters;

        /// <summary>
        /// Checks if the given object equals the current one.
        /// </summary>
        /// <param name="obj">Object to check if it is equal to the current one.</param>
        /// <returns></returns>
        public override bool Equals(object obj) => obj is TableCellAttribute cell && cell.Row == Row && cell.Column == Column;

        /// <summary>
        /// Default hash function.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => new { Row, Column }.GetHashCode();

        #endregion
    }
}
