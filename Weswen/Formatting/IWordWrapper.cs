using System.Collections.Generic;

namespace Weswen
{
    /// <summary>
    /// Used for wrapping and unwrapping strings.
    /// </summary>
    public interface IWordWrapper
    {
        /// <summary>
        /// Wraps the original string based on the max length.
        /// Individual substrings are returned as a collection.
        /// </summary>
        /// <param name="original">String that needs to be wrapped.</param>
        /// <param name="maxLength">Max length per row.</param>
        /// <returns></returns>
        IEnumerable<string> Wrap(string original, int maxLength);

        /// <summary>
        /// Unwraps the wrapped string.
        /// </summary>
        /// <param name="wrapped">Collection of substrings that used to
        /// represent the original string.</param>
        /// <returns></returns>
        string Unwrap(IEnumerable<string> wrapped);
    }
}
