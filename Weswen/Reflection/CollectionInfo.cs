using System;
using System.Collections;

namespace Weswen
{
    /// <summary>
    /// Information about a type - is it a collection and if it is, which
    /// types does that collection work with?
    /// </summary>
    public class CollectionInfo
    {
        #region Properties

        /// <summary>
        /// Defines if the passed in type is IEnumerable (if it
        /// implements <see cref="IEnumerable"/>).
        /// <para>(Arrays also implement <see cref="IEnumerable"/>).</para>
        /// </summary>
        public bool IsCollection { get; }

        /// <summary>
        /// Defines if the passed in type is an array.
        /// </summary>
        public bool IsArray { get; }

        /// <summary>
        /// If the passed in type is an array, this will
        /// be the type of that array.
        /// <para>For example, if the passed in type is of type int[],
        /// this collection will contain the type int.</para>
        /// </summary>
        public Type ArrayType { get; }

        /// <summary>
        /// If the passed in type is an IEnumerable, this will be the
        /// collection of types for that IEnumerable. <para>For example,
        /// if the passed in type is of type Dictionary&lt;int, string&gt;,
        /// this collection will contain types int and string.</para>
        /// </summary>
        public Type[] CollectionTypes { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Provides information about a type.
        /// </summary>
        /// <param name="typeToTest">Type to test if it is a collection.</param>
        /// <exception cref="ArgumentNullException"/>
        public CollectionInfo(Type typeToTest)
        {
            // Don't allow the type to be null
            typeToTest.ThrowIfNull(nameof(typeToTest));

            // Check if the type is an array
            if (typeof(Array).IsAssignableFrom(typeToTest))
            {
                // Set IsArray to true
                IsArray = true;
                // Get the array type from the type
                ArrayType = typeToTest.GetElementType();
            }

            // Check if the type implements IEnumerable
            if (typeof(IEnumerable).IsAssignableFrom(typeToTest))
            {
                // Set IsIEnumerable to true
                IsCollection = true;
                // Get the type arguments for the collection
                CollectionTypes = typeToTest.GetGenericArguments();
            }
        } 

        #endregion
    }
}
