using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Novacode;

namespace Weswen
{
    /// <summary>
    /// Class used to work with Word Documents (docx files).
    /// </summary>
    public class WordManager
    {
        #region Fields

        private DocX _document;
        private string _documentPath;
        private bool _dispose = true;
        private Formatting _formatting = new Formatting();
        private readonly IWordWrapper _wordWrapper;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the manager with a path to the file.
        /// </summary>
        /// <param name="documentPath">Path to the Word Document.</param>
		/// <param name="overwrite">Should the document be overwritten if it exists?</param>
        /// <exception cref="ArgumentNullException"/>
        public WordManager(string documentPath, bool overwrite) : this(documentPath, overwrite, new WordWrapper())
        {
        }

		/// <summary>
		/// Initializes the manager with a path to the file and an implementation
		/// of a <see cref="IWordWrapper"/>.
		/// </summary>
		/// <param name="documentPath">Path to the Word Document.</param>
		/// <param name="overwrite">Should the document be overwritten if it exists?</param>
		/// <param name="wordWrapper">Wrapper used for </param>
		/// <exception cref="ArgumentNullException"/>
		public WordManager(string documentPath, bool overwrite, IWordWrapper wordWrapper)
        {
            // Set the path
            ChangeDocumentPath(documentPath, overwrite);

            // Initialize the word wrapper
            _wordWrapper = wordWrapper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Changes the current document path to the specified one (unless it is the same as the current one).
        /// <para>If the file exists, it will be loaded, otherwise, a new document will be created.</para>
        /// </summary>
        /// <param name="path">Path to the new file.</param>
		/// <param name="overwrite">Should the document be overwritten if it exists?</param>
        /// <exception cref="ArgumentNullException"/>
        public void ChangeDocumentPath(string path, bool overwrite)
        {
            // Don't allow nulls
            path.ThrowIfNull(nameof(path));

            // If the path is the same...
            if (_documentPath == path)
                // Return
                return;

			// If it should be overwritten...
			if (overwrite)
				// Delete it
				File.Delete(path);

			// If the file doesn't exist at the provided path...
			if (!File.Exists(path))
			{
				// Create a new one
				_document = DocX.Create(path);

				// Save it and dispose of the object
				SaveAndDispose();
			}

            // Remember the document path
            _documentPath = path;
        }

        /// <summary>
        /// Inserts tables for each table model in the list.
        /// </summary>
        /// <typeparam name="T">Type decorated with <see cref="TableModelAttribute"/>.</typeparam>
        /// <param name="tableModels">List of instances of a class decorated with <see cref="TableModelAttribute"/>.</param>
        /// <param name="progress">Used to report the progress while adding tables.</param>
        public void AddTables<T>(IEnumerable<T> tableModels, IProgress<double> progress)
        {
            // Throw an argument exception if there are no table models
            tableModels.ThrowIfNull(nameof(tableModels));

            // Don't dispose the document on adding tables
            _dispose = false;

            // Load the document from the path
            _document = DocX.Load(_documentPath);

            // Get the number of models inside the collection
            var numModels = tableModels.Count();

            // Get metadata about the model to use inside the loop
            var tableAttribute = GetTableModelAttribute<T>();
            var tableCellProperties = GetCells<T>();

            // Determine if there should ever be used a page break
            var usePageBreak = tableAttribute.PageBreakAfter > 0;

            // Setup the progress counter
            var counter = 1;

            // For each model inside the collection...
            foreach (var tableModel in tableModels)
            {
                // Add the table
                AddTable(
                    tableModel: tableModel,
                    tableAttribute: tableAttribute,
                    tableCellProperties: tableCellProperties,
                    pageBreakAfter: usePageBreak && counter % tableAttribute.PageBreakAfter == 0
                );

                // Report the progress
                progress?.Report(100.0 * counter / numModels);

                // Increase the counter
                counter++;
            }

            // Reset the dispose flag
            _dispose = true;

            // Save changes and dispose
            SaveAndDispose();
        }

        /// <summary>
        /// Inserts a table at the end of the document.
        /// </summary>
        /// <typeparam name="T">Type decorated with <see cref="TableModelAttribute"/>.</typeparam>
        /// <param name="tableModel">Instance of class decorated with <see cref="TableModelAttribute"/>.</param>
        /// <param name="pageBreakAfter">Should a page break be inserted after the table?</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        public void AddTable<T>(T tableModel, bool pageBreakAfter = false)
            => AddTable(tableModel, pageBreakAfter);

        private void AddTable<T>(
            T tableModel,
            bool pageBreakAfter,
            TableModelAttribute tableAttribute = null,
            IEnumerable<(PropertyInfo prop, TableCellAttribute cell)> tableCellProperties = null
        )
        {
            // Don't allow the table model to be null
            tableModel.ThrowIfNull(nameof(tableModel));

            // If the document should be disposed after adding the table...
            if (_dispose)
                // Load it from the path
                _document = DocX.Load(_documentPath);

            // Get the table attribute if the provided one was null
            tableAttribute = tableAttribute ?? GetTableModelAttribute<T>();

            // Create the table
            var table = _document.AddTable(tableAttribute.NumRows, tableAttribute.NumColumns);

            // Set its alignment
            table.Alignment = tableAttribute.Alignment;

            // If the cells weren't passed in, fetch them
            tableCellProperties = tableCellProperties ?? GetCells<T>();

            // Setup the columns widths and row spans
            table.SetupColumnWidthsAndRowSpans(tableAttribute.ColumnWidths, tableCellProperties);

            // Foreach property that is defined as a table cell...
            foreach (var (prop, cell) in tableCellProperties)
            {
                // Change the formatting options to the ones defined by the cell
                _formatting.Bold = cell.Bold;
                _formatting.Position = cell.TextSpacing;
                _formatting.Size = cell.TextSize;

                // Get the table cell that the content needs to be inserted in
                var tableCell = table.Rows[cell.RowIndex].Cells[cell.ColumnIndex];

                // Set the content of the cell
                tableCell.SetContent(tableModel, cell, prop, _formatting, _wordWrapper);

                // If the cell spans more than one column...
                if (cell.ColumnSpan > 1)
                    // Apply it
                    table.Rows[cell.RowIndex].MergeCells(cell.ColumnIndex, cell.ColumnSpanEnd);

                // Configure the number of paragraphs (defined on the attribute)
                tableCell.ConfigureNumParagraphs(cell);
            } // ENDOF: foreach

            // Remove all of the empty paragraphs from the table
            // (Except from the cells that define exact number of paragraphs)
            table.RemoveEmptyParagraphs(tableCellProperties);

            // If there should be only page break inserted after the table...
            if (pageBreakAfter)
                // Insert the table and the page break
                _document.InsertTable(table).InsertPageBreakAfterSelf();

            // Otherwise
            else
            {
                // Insert the table
                _document.InsertTable(table);

                // Insert the paragraphs after the table
                for (int i = 1; i <= tableAttribute.NumParagraphsAfter; i++)
                    _document.InsertParagraph();
            }

            // If the document should be disposed after adding the table...
            if (_dispose)
                // Save changes and dispose
                SaveAndDispose();
        }

        /// <summary>
        /// Gets the table models based on the tables in the Word Document.
        /// </summary>
        /// <typeparam name="T">Type decorated with <see cref="TableModelAttribute"/>.</typeparam>
        /// <param name="progress">Used to report the progress while converting tables into models.</param>
        /// <param name="whitespaceReplacement">What should any whitespace found be replaced with?</param>
        /// <param name="trim">Should the whitespace at the beginning and end be trimmed?</param>
        /// <returns></returns>
        public List<T> GetTableModels<T>(
            IProgress<double> progress,
            string whitespaceReplacement = " ",
            bool trim = true
        ) where T : new()
        {
            // Create a new list for the table models
            var data = new List<T>();

			// Try to load the document
			try
			{
				_document = DocX.Load(_documentPath);
			}
			catch(Exception ex)
			{
				throw new DocumentNotAvailableException(ex, _documentPath);
			}

            // Get the property - table cell tuples
            var tableCellProperties = GetCells<T>();

            // Setup the progress counter
            var counter = 1;

            // For each table in the document...
            foreach (var table in _document.Tables)
            {
				// Create an instance of the object
				var instance = new T();

				// Try to parse it...
				try
				{
					// Foreach table cell property in the type...
					foreach (var (prop, cell) in tableCellProperties)
					{
						// Get the exact cell from the Word Document table
						var tableCell = table.Rows[cell.RowIndex].Cells[cell.ColumnIndex];

						// Get the paragraphs into string collection
						var paragraphs = tableCell.Paragraphs.Select(para =>
						{
							// Extract text from the paragraph
							var text = para.Text;

							// If the whitespace should be replaced...
							if (!whitespaceReplacement.IsNullOrEmpty())
								// Replace it
								text = text.ReplaceWhiteSpace(whitespaceReplacement);

							// If a new line should be added after each paragraph...
							if (cell.AddNewLinesPerParagraph)
								// Add it
								text += Environment.NewLine;

							// Return the end result
							return text;
						}).ToList();

						// If the new lines were added and there are paragraphs...
						if (cell.AddNewLinesPerParagraph && paragraphs.Count > 0)
						{
							// Get a reference to the last one
							var lastParagraph = paragraphs[paragraphs.Count - 1];

							// Remove the new line from it
							paragraphs[paragraphs.Count - 1] = lastParagraph.Remove(lastParagraph.LastIndexOf(Environment.NewLine));
						}

						// Get the content by unwrapping all of the paragraphs
						var content = _wordWrapper.Unwrap(paragraphs);

						// If the cell is formatted...
						if (cell.Format != null && cell.Format.Contains("{0}"))
							// Unformat it
							content = content.Unformat(cell.Format);

						// If the end result should be trimmed...
						if (trim)
							// Trim leading and trailing whitespace
							content = content.Trim();

						// Set the value of the instance to the content converted to the type of the property
						prop.SetValue(
							instance,
							content.ConvertFromString(prop.PropertyType)
						);
					}

					// Add the instance to the collection
					data.Add(instance);

					// Report the progress
					progress?.Report(100.0 * counter / _document.Tables.Count);

					// Increase the counter
					counter++;
				}

				// If we couldn't parse the table model...
				catch(Exception ex)
				{
					// Throw the exception
					throw new TableModelParseException(instance, ex);
				}
            }

            // Dispose of the document
            _document.Dispose();

            // Return the table models
            return data;
        }

        #region Helpers

        private void SaveAndDispose()
        {
            // Save changes
            _document.Save();

            // Dispose of the document
            _document.Dispose();
        }

        private IEnumerable<(PropertyInfo prop, TableCellAttribute cell)> GetCells<T>()
        {
            // Declare the comparer for the tuple
            var comparer = new TableCellPropertyComparer();

            return typeof(T)
                // Get the properties
                .GetProperties()
                // Filter to those that have the attribute
                .Where(prop => Attribute.IsDefined(prop, typeof(TableCellAttribute)))
                // Get the property and its attribute into a tuple
                .Select(prop => (prop, cell: prop.GetCustomAttribute<TableCellAttribute>()))
                // Get rid of the duplicates
                .Distinct(comparer)
                // Order by rows first
                .OrderBy(tuple => tuple.cell.Row)
                // And then by columns
                .ThenBy(tuple => tuple.cell.Column);
        }

        private TableModelAttribute GetTableModelAttribute<T>()
        {
            // Get the table model attribute
            var tableAttribute = typeof(T).GetCustomAttribute<TableModelAttribute>();

            // The table model must exist, throw exception if it doesn't
            tableAttribute.ThrowIfInvalid<TableModelAttribute, ArgumentException>(attr => attr != null,
                $"The provided table model type doesn't have the {nameof(TableModelAttribute)}.");

            // Return the valid table model attribute
            return tableAttribute;
        }

        #endregion

        #endregion
    }
}
