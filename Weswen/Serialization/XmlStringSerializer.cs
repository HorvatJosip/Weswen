using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Weswen
{
    /// <summary>
    /// Used for serializing an object into an XML string or
    /// deserializing an XML string into an object.
    /// </summary>
    public static class XmlStringSerializer
    {
        /// <summary>
        /// Deserializes an XML string into an object using <see cref="Encoding.Unicode"/>.
        /// </summary>
        /// <typeparam name="T">Object to turn the XML into.</typeparam>
        /// <param name="data">XML string.</param>
        /// <returns></returns>
        public static T Deserialize<T>(string data)
            => Deserialize<T>(data, Encoding.Unicode);

        /// <summary>
        /// Deserializes an XML string into an object using the specified <paramref name="encoding"/>.
        /// </summary>
        /// <typeparam name="T">Object to turn the XML into.</typeparam>
        /// <param name="data">XML string.</param>
        /// <param name="encoding">Encoding of the string.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="EncoderFallbackException"/>
        public static T Deserialize<T>(string data, Encoding encoding)
        {
            // Don't allow nulls
            Utils.ThrowIfParameterNull((data, nameof(data)), (encoding, nameof(encoding)));

            // Create XML serializer
            var xmlSerializer = new XmlSerializer(typeof(T));

            // Create a memory stream for the raw string data
            using (var memoryStream = new MemoryStream(encoding.GetBytes(data)))
                // Deserialize and return the XML string
                return (T)xmlSerializer.Deserialize(memoryStream);
        }

        /// <summary>
        /// Serializes an item into an XML string.
        /// </summary>
        /// <param name="item">The item to serialize into the XML.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        public static string Serialize(object item)
        {
            // Don't allow nulls
            item.ThrowIfNull(nameof(item));

            // Create XML serializer
            var xmlSerializer = new XmlSerializer(item.GetType());

            // Create a string writer to rececive the serialized string
            using (var stringWriter = new StringWriter())
            {
                // Serialize the object to a string
                xmlSerializer.Serialize(stringWriter, item);

                // Extract the string from the writer and return it
                return stringWriter.ToString();
            }
        }
    }
}
