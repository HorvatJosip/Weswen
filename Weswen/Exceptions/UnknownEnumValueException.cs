using System;

namespace Weswen
{
    /// <summary>
    /// Exception that should be thrown when an unknown enum value was specified.
    /// </summary>
    public class UnknownEnumValueException : Exception
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UnknownEnumValueException() : base("The specified enum value doesn't exist.")
        {
        }

        /// <summary>
        /// Generates an instance with a specified message.
        /// </summary>
        public UnknownEnumValueException(string message) : base(message)
        {
        }

        #endregion
    }

	/// <summary>
	/// Generic version of <see cref="UnknownEnumValueException"/>.
	/// </summary>
	/// <typeparam name="T">Type of enum.</typeparam>
	public class UnknownEnumValueException<T> : UnknownEnumValueException
	{
		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		public UnknownEnumValueException() : this($"The enum value wasn't found as part of {typeof(T).Name}...")
		{

		}

		/// <summary>
		/// Generates an instance with a specified message.
		/// </summary>
		public UnknownEnumValueException(string message) : base(message)
		{

		}

		#endregion
	}
}
