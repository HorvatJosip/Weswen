using System;

namespace Weswen
{
	/// <summary>
	/// Exception that is thrown when the document can't be loaded.
	/// </summary>
	public class DocumentNotAvailableException : Exception
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="innerException">Specific exception that caused this exception.</param>
		public DocumentNotAvailableException(Exception innerException) : this(innerException, null)
		{

		}

		/// <summary>
		/// Creates an instance of exception aware of the location
		/// of the document.
		/// </summary>
		/// <param name="innerException">Specific exception that caused this exception.</param>
		/// <param name="location">Location where the document resides.</param>
		public DocumentNotAvailableException(Exception innerException, string location) 
			: base($"Couldn't load the given document{(location == null ? "" : $" at {location}")}", innerException)
		{

		}
	}
}
