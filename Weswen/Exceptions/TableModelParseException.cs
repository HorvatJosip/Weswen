using System;

namespace Weswen
{
	/// <summary>
	/// Exception that is thrown when the table model
	/// can't be parsed.
	/// </summary>
	public class TableModelParseException : Exception
	{
		/// <summary>
		/// Table model that couldn't be parsed.
		/// </summary>
		public object TableModel { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tableModel">Table model that couldn't be parsed.</param>
		/// <param name="innerException">Specific exception that caused this exception.</param>
		public TableModelParseException(object tableModel, Exception innerException) : base($"Couldn't parse the given table model ({tableModel})...", innerException)
		{
			TableModel = tableModel;
		}
	}
}
