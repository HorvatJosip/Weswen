using System;
using System.Linq;
using System.Reflection;

using Novacode;

namespace Weswen
{
    /// <summary>
    /// Extension methods for <see cref="Cell"/>.
    /// </summary>
    public static class CellExtensions
    {
        /// <summary>
        /// Inserts a paragraph at the given index. If one exists at the index, it is replaced,
        /// otherwise, a new one is inserted.
        /// </summary>
        /// <param name="tableCell">Cell of the table.</param>
        /// <param name="index">Index to insert to.</param>
        /// <param name="text">Paragraph content.</param>
        /// <param name="formatting">Cell formatting.</param>
        /// <param name="trackChanges">Should the changes be tracked?</param>
        /// <exception cref="IndexOutOfRangeException"/>
        public static void InsertParagraphAt(this Cell tableCell, int index, string text, Formatting formatting, bool trackChanges = false)
        {
            // Don't allow the index to be below 0
            index.ThrowIfInvalid<int, IndexOutOfRangeException>(num => num >= 0);

            // If there is already a paragraph at that index...
            if (index < tableCell.Paragraphs.Count)
                // Edit it
                tableCell.Paragraphs[index].ReplaceText(
                    oldValue: tableCell.Paragraphs[index].Text,
                    newValue: text,
                    trackChanges: trackChanges,
                    newFormatting: formatting
                );

            // Otherwise...
            else
                // Add a new paragraph
                tableCell.InsertParagraph(text, trackChanges, formatting);
        }

        /// <summary>
        /// Gets the text that is inside the cell (from the paragraphs).
        /// </summary>
        /// <param name="tableCell">Cell of the table to get the text from.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"/>
        public static string Text(this Cell tableCell)
        {
            // Don't allow null reference
            tableCell.ThrowIfNull();

            // Return a new string from all the paragraphs
            return new string(
                tableCell.Paragraphs.SelectMany(paragraph => paragraph.Text).ToArray()
            );
        }

        /// <summary>
        /// Sets the content of the cell.
        /// </summary>
        /// <param name="tableCell">Cell to set the content of.</param>
        /// <param name="tableModel">Table model to get the value from.</param>
        /// <param name="cell">Information about the table cell.</param>
        /// <param name="prop">Property of the table model this cell belongs to.</param>
        /// <param name="formatting">Formatting for the table cell.</param>
        /// <param name="wordWrapper">String wrapping helper.</param>
        public static void SetContent(
            this Cell tableCell,
            object tableModel,
            TableCellAttribute cell,
            PropertyInfo prop,
            Formatting formatting,
            IWordWrapper wordWrapper
        )
        {
            // If there aren't paragraphs already defined...
            if (cell.Paragraphs.IsNullOrEmpty())
            {
                // Get the table cell content from the model
                var content = prop.GetTableCellContent(tableModel);

                // If a format for the content is provided...
                if (!cell.Format.IsNullOrWhiteSpace() && cell.Format.Contains("{0}"))
                    // Use the format
                    content = string.Format(cell.Format, content);

                // If it should be split into multiple paragraphs...
                if (cell.ShouldWrap(content.Length))
                {
                    // Get the wrapped content using the word wrapper
                    var wrappedContent = wordWrapper.Wrap(content, cell.MaxRowCharacters);

                    // Loop throught the wrapped content
                    for (int i = 0; i < wrappedContent.Count(); i++)
                        // Insert the paragraph at current index
                        tableCell.InsertParagraphAt(i, wrappedContent.ElementAt(i), formatting);
                }

                // Otherwise...
                else
                    // Add it as the only paragraph
                    tableCell.InsertParagraphAt(0, content, formatting);
            }

            // Otherwise...
            else
                // Loop through the defined paragraphs
                for (int i = 0; i < cell.Paragraphs.Length; i++)
                    // Insert the paragraph at current index
                    tableCell.InsertParagraphAt(i, cell.Paragraphs[i], formatting);
        }

        /// <summary>
        /// Configures the number of paragraphs of the cell if the 
        /// <see cref="TableCellAttribute.NumParagraphs"/> is at least 1.
        /// </summary>
        /// <param name="tableCell">Table cell to configure the number of paragraphs for.</param>
        /// <param name="cell">Information about the table cell.</param>
        public static void ConfigureNumParagraphs(this Cell tableCell, TableCellAttribute cell)
        {
            // If there should be fixed number of paragraphs...
            if (cell.NumParagraphs > 0)
            {
                // Defines how many paragraphs are already based on exceptions
                int paragraphsAlready = 0;

                // If there are some exceptions
                if (cell.NumParagraphsExceptions.IsNullOrEmpty() == false)
                    // Find out how many "paragraphs" there are already by counting number of
                    // occurences of each of the exceptions
                    paragraphsAlready = cell.NumParagraphsExceptions.Sum(ex => tableCell.Text().SubstringCount(ex));

                // Keep adjusting the number of paragraphs until they match the requirement...
                while (paragraphsAlready + tableCell.Paragraphs.Count != cell.NumParagraphs)
                {
                    // If there are excess paragraphs...
                    if (paragraphsAlready + tableCell.Paragraphs.Count > cell.NumParagraphs)
                    {
                        // If this isn't the last paragraph...
                        if (tableCell.Paragraphs.Count > 1)
                            // Remove the last one
                            tableCell.Paragraphs.Last().Remove(false);

                        // Otherwise...
                        else
                            // Break out of the loop, don't remove content
                            break;
                    }

                    // Otherwise...
                    else
                        // Insert an empty one
                        tableCell.InsertParagraph("");
                }
            }
        }
    }
}
