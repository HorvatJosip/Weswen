using System;

namespace Weswen
{
    /// <summary>
    /// Extension methods used for throwing exceptions.
    /// </summary>
    public static class ExceptionThrowsingExtensions
    {
        /// <summary>
        /// Extension for throwing <see cref="NullReferenceException"/> or
        /// <see cref="ArgumentNullException"/> if <paramref name="paramName"/> is provided.
        /// </summary>
        /// <param name="obj">Object to test if it is null.</param>
        /// <param name="paramName">Name of the parameter to test (if the provided object is a parameter / method argument).</param>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="ArgumentNullException"/>
        public static void ThrowIfNull(this object obj, string paramName = null)
        {
            // If the passed in object isn't null...
            if (obj != null)
                // Return
                return;

            // If the passed in parameter name is null...
            if (paramName == null)
                // Throw null reference exception
                throw new NullReferenceException();

            // Otherwise...
            else
                // Throw argument null exception and pass in name of the parameter
                throw new ArgumentNullException(paramName);
        }

        /// <summary>
        /// Throws an exception if the <paramref name="validityCheck"/> returns false.
        /// </summary>
        /// <typeparam name="TObj">Type of object.</typeparam>
        /// <typeparam name="TException">Type of exception.</typeparam>
        /// <param name="obj">Object to test.</param>
        /// <param name="validityCheck">Validation for the object.</param>
        /// <param name="exceptionParameters">Optional parameters for the exception.</param>
        /// <exception cref="Exception"/>
        public static void ThrowIfInvalid<TObj, TException>(this TObj obj, Func<TObj, bool> validityCheck, params object[] exceptionParameters) where TException : Exception
        {
            // Don't allow the check to be null
            validityCheck.ThrowIfNull(nameof(validityCheck));

            // If validity check fails...
            if(validityCheck(obj) == false)
            {
                // Create an instance of the exception
                var exception = Activator.CreateInstance(typeof(TException), exceptionParameters);

                // Throw it
                throw (Exception)exception;
            }
        }
    }
}
