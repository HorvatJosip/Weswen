using System.Collections.Generic;

namespace Weswen
{
    /// <summary>
    /// Default implementation of <see cref="IWordWrapper"/>.
    /// </summary>
    public class WordWrapper : IWordWrapper
    {
        /// <summary>
        /// Wraps the original string based on the max length.
        /// Individual substrings are returned as a collection.
        /// </summary>
        /// <param name="original">String that needs to be wrapped.</param>
        /// <param name="maxLength">Max length per row.</param>
        /// <returns></returns>
        public IEnumerable<string> Wrap(string original, int maxLength)
        {
            // Create a list for the wrapped content
            var wrappedContent = new List<string>();

            // Loop counter
            var i = 0;
            // Loop flag
            var isDone = false;
            // Next starting point
            var start = 0;

            // Loop until the algorithm is done...
            while (isDone == false)
            {
                // Declare the length of substringing
                var length = maxLength;

                // Check if we've gone over all of the content
                if (start + length >= original.Length)
                {
                    // Adjust the length to the remaining length so that it doesn't cause the overflow
                    length = original.Length - start;

                    // Set the flag to notify that we've reached the end
                    isDone = true;
                }

                // Substring the original
                var substringed = original.Substring(start, length);

                // Add an extra character for the whitespace check
                var extraWhitespaceCheck = isDone ? '\0' : original[start + length];

                // Get the index of last whitespace
                var lastWhitespace = (substringed + extraWhitespaceCheck).LastIndexOfWhiteSpace();

                // If the whitespace exists...
                if (lastWhitespace != -1)
                {
                    // Adjust the new paragraph start to be after the whitespace
                    start += lastWhitespace + 1;

                    // If the whitespace isn't the last character (the extra one)...
                    if (lastWhitespace != length)
                        // Adjust the substringed original to go until the whitespace
                        substringed = substringed.Remove(lastWhitespace);
                }

                // Otherwise...
                else
                {
                    // Increase start by the full length minus one (for the '-')
                    start += length - 1;

                    //If the algorithm isn't done...
                    if (isDone == false)
                        // Replace the last one with the '-' 
                        substringed = substringed.Remove(substringed.Length - 1) + '-';
                }

                // If the substring isn't empty...
                if (!substringed.IsNullOrWhiteSpace())
                    // Add the substring to the wrapped content list
                    wrappedContent.Add(substringed);

                // Increase the counter
                i++;
            } // ENDOF: while

            // Return the wrapped content
            return wrappedContent;
        }

        /// <summary>
        /// Unwraps the wrapped string.
        /// </summary>
        /// <param name="wrapped">Collection of substrings that used to
        /// represent the original string.</param>
        /// <returns></returns>
        public string Unwrap(IEnumerable<string> wrapped)
        {
            // Initialize the string builder for the content
            var content = new System.Text.StringBuilder();

            // Foreach string in the wrapped content...
            foreach (var str in wrapped)
            {
                // If the text is empty or just a character or...
                // If the last character is a '-' and there is no whitespace before it is false...
                if (str.Length < 2 || !(str[str.Length - 1] == '-' && !char.IsWhiteSpace(str[str.Length - 2])))
                {
                    // Add the text to the content
                    content.Append(str);
                    // Separate the paragraphs with a space
                    content.Append(' ');
                }

                // Otherwise...
                else
                    // Get rid of the '-' and don't add space to allow the paragraphs to merge
                    content.Append(str.Remove(str.Length - 1));
            }

            // Get the content as a string
            return content.ToString();
        }
    }
}
