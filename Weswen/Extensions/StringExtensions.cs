using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Weswen
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Indicates whether the specified string is null or a <see cref="string.Empty"/> string.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string value)
            => string.IsNullOrEmpty(value);

        /// <summary>
        /// Indicates whether a specified string is null, empty,
        /// or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string value)
            => string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// If the string is null, <paramref name="newValue"/> is returned.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="newValue">Value to return if the <paramref name="value"/> is null.</param>
        /// <returns></returns>
        public static string ReplaceIfNull(this string value, string newValue)
            => value ?? newValue;

        /// <summary>
        /// If the string is null or empty, <paramref name="newValue"/> is returned.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="newValue">Value to return if the <paramref name="value"/> is null or empty.</param>
        /// <returns></returns>
        public static string ReplaceIfNullOrEmpty(this string value, string newValue)
            => value.IsNullOrEmpty() ? newValue : value;

        /// <summary>
        /// If the string is null or whiteSpace, <paramref name="newValue"/> is returned.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="newValue">Value to return if the <paramref name="value"/> is null or whiteSpace.</param>
        /// <returns></returns>
        public static string ReplaceIfNullOrWhiteSpace(this string value, string newValue)
            => value.IsNullOrWhiteSpace() ? newValue : value;

        /// <summary>
        /// Takes in a string and surrounds it with two other strings.
        /// </summary>
        /// <param name="value">Current string.</param>
        /// <param name="before">String to prepend to the current string.</param>
        /// <param name="after">String to append to the current string.</param>
        /// <returns></returns>
        public static string SurroundWith(this string value, string before, string after)
            => $"{before}{value}{after}";

        /// <summary>
        /// Removes all of the whitespace from a string.
        /// </summary>
        /// <param name="value">String to remove the whitespace from.</param>
        /// <returns></returns>
        public static string WithoutWhiteSpace(this string value) => new string(
            value?.Where(@char => !char.IsWhiteSpace(@char)).ToArray()
        );

        /// <summary>
        /// Gets the index of a first whitespace character in the string.
        /// </summary>
        /// <param name="value">String to find the whitespace in.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"/>
        public static int IndexOfWhiteSpace(this string value)
        {
            // Don't allow null reference
            value.ThrowIfNull();

            // Loop through the section
            for (int i = 0; i < value.Length; i++)
                // If the current character is whitespace...
                if (char.IsWhiteSpace(value[i]))
                    // Return its index
                    return i;

            // Whitespace wasn't found, return -1
            return -1;
        }

        /// <summary>
        /// Gets the index of a last whitespace character in the string.
        /// </summary>
        /// <param name="value">String to find the whitespace in.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"/>
        public static int LastIndexOfWhiteSpace(this string value)
        {
            // Don't allow null reference
            value.ThrowIfNull();

            // Loop through the section from the end
            for (int i = value.Length - 1; i >= 0; i--)
                // If the current character is whitespace...
                if (char.IsWhiteSpace(value[i]))
                    // Return its index
                    return i;

            // Whitespace wasn't found, return -1
            return -1;
        }


        /// <summary>
        /// Finds how many times does a given "target" substring occurr
        /// in the current string.
        /// </summary>
        /// <param name="value">String to search.</param>
        /// <param name="target">Substring to find the count of.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="ArgumentNullException"/>
        public static int SubstringCount(this string value, string target)
        {
            // Don't allow nulls
            value.ThrowIfNull();
            target.ThrowIfNull(nameof(target));

            // Initialize the counter
            var count = 0;
            // Setup the current index
            int currentIndex = -target.Length;

            // Loop until the target value is no longer found
            while (true)
            {
                // Try to find the next occurence of the target string
                currentIndex = value.IndexOf(target, currentIndex + target.Length);

                // If the substring was found...
                if (currentIndex != -1)
                    // Increase the counter
                    count++;

                // Otherwise...
                else
                    // Return the substring count
                    return count;
            }
        }

        /// <summary>
        /// Converts a string into an object of the specified type.
        /// </summary>
        /// <param name="value">String to convert into an object.</param>
        /// <param name="type">Type of object to convert the string into.</param>
        /// <param name="culture">Culture used for converting to string.</param>
        /// <param name="context">Context info about the <see cref="TypeDescriptor"/> for the type.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ArgumentNullException"/>
        public static object ConvertFromString(this string value, Type type, CultureInfo culture = null, ITypeDescriptorContext context = null)
        {
            // Get the type converter
            var typeConverter = TypeDescriptor.GetConverter(type);
            
            // Try to convert from string to the wanted type
            return typeConverter.ConvertFromString(context, culture, value);
        }

        /// <summary>
        /// Converts a string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of object to convert the string into.</typeparam>
        /// <param name="value">String to convert into an object.</param>
        /// <param name="culture">Culture used for converting to string.</param>
        /// <param name="context">Context info about the <see cref="TypeDescriptor"/> for the type <typeparamref name="T"/>.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ArgumentNullException"/>
        public static T ConvertFromString<T>(this string value, CultureInfo culture = null, ITypeDescriptorContext context = null)
            => (T)ConvertFromString(value, typeof(T), culture, context);

        /// <summary>
        /// Replaces whitespace inside a string with a given string.
        /// </summary>
        /// <param name="value">String to replace the whitespace in.</param>
        /// <param name="with">Replacement for whitespace.</param>
        /// <returns></returns>
        public static string ReplaceWhiteSpace(this string value, string with = " ")
            => with.IsNullOrEmpty()
                ? value
                :Regex.Replace(value, "\\s+", with);

        /// <summary>
        /// Takes the formatted string in with its format and replaces all
        /// of the fixed parts that are defined by the format.
        /// </summary>
        /// <param name="value">String to unformat.</param>
        /// <param name="format">Format that the string was formatted by.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        public static string Unformat(this string value, string format)
        {
            // Don't allow nulls
            value.ThrowIfNull();
            format.ThrowIfNull(nameof(format));

            // Create a collection that will store constant strings - the ones to remove
            var strings = new List<string>();
            // Declare the current working index and left brace index
            int index = 0, leftBrace = -1;

            void AddString(bool full = false)
            {
                // If the string should be substringed until the end..
                string substring = full
                    // Substring from the index
                    ? format.Substring(index)
                    // Otherwise substring from the index and until the left brace
                    : format.Substring(index, leftBrace - index);

                // If it exists...
                if (!substring.IsNullOrEmpty())
                    // Add it to the replacables
                    strings.Add(substring.Replace("}}", "}"));
            }

            // Loop until the format's length is hit
            while (index < format.Length)
            {
                // Get the index of the '{'
                leftBrace = format.IndexOf('{', index);

                // If the left brace is found...
                if (leftBrace != -1)
                {
                    // If the opening brace is the end of the format 
                    // or immediately followed by the closing brace...
                    if (leftBrace + 1 == format.Length || format[leftBrace + 1] == '}')
                        // Throw the format exception
                        throw new FormatException();

                    // Else if the next character is another opening brace...
                    else if (format[leftBrace + 1] == '{')
                    {
                        // Include the first brace
                        leftBrace += 1;

                        // Add string before the second brace
                        AddString();

                        // Skip the second brace
                        index = leftBrace + 1;
                    }

                    // Otherwise...
                    else
                    {
                        // Loop from that point to the end
                        for (int i = leftBrace + 1; i < format.Length; i++)
                        {
                            // If we've hit the closing brace...
                            if (format[i] == '}')
                            {
                                // Add the string until the left brace
                                AddString();

                                // Set the next starting point right after the closing brace
                                index = i + 1;

                                // Break out of for loop
                                break;
                            }

                            // Else if the character isn't a digit...
                            else if (!char.IsDigit(format[i]))
                                // Throw the format exception
                                throw new FormatException();
                        } // ENDOF: for
                    } // ENDOF: else
                } // ENDOF: if

                // Otherwise...
                else
                {
                    // Add the remainder of the string
                    AddString(true);

                    // Finish with the algorithm
                    break;
                }
            }

            // Foreach string that needs to be removed...
            foreach (var str in strings)
            {
                // Find its location
                var startIndex = value.IndexOf(str);

                // Remove it
                value = value.Remove(startIndex, str.Length);
            }

            // Remove the unformatted string
            return value;
        }
    }
}
