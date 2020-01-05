using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Weswen
{
    /// <summary>
    /// Manager used for saving and loading data from files.
    /// </summary>
    /// <typeparam name="TKey">Type used for switching files.</typeparam>
    public class FileManager<TKey>
    {
        #region Properties

        /// <summary>
        /// Working directory - path used for defining the base
        /// for all of the paths provided in the methods.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Paths to the files aliased by the <typeparamref name="TKey"/>.
        /// </summary>
        public IDictionary<TKey, string> Paths { get; set; }

        /// <summary>
        /// Serializer to use during the (de)serialization.
        /// </summary>
        public Serializer Serializer { get; set; }

        #endregion

        #region Methods

        #region Serialization

        /// <summary>
        /// Writes an object to the given path.
        /// </summary>
        /// <param name="path">Path to write the serialized object content to.</param>
        /// <param name="obj">Object to serialize to the file.</param>
        /// <param name="serializer">Serializer to use.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="SerializationException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="UnknownEnumValueException"/>
        public void Serialize(string path, object obj, Serializer serializer)
        {
            // Don't allow the path or the passed in object to be null
            Utils.ThrowIfParameterNull((path, nameof(path)), (obj, nameof(obj)));

            // Get the path that may be affected by the base path
            path = GetPath(path);

            // Switch on the serializer that should be used
            switch (serializer)
            {
                // Binary serialization
                case Serializer.Binary:
                    // Create the binary formatter
                    var formatter = new BinaryFormatter();

                    // Open file stream...
                    using (var fileStream = File.OpenWrite(path))
                        // Serialize the object to the file stream using the binary formatter
                        formatter.Serialize(fileStream, obj);
                    break;

                // XML stream serialization
                case Serializer.XmlStream:
                    if (File.Exists(path))
                        File.Delete(path);

                    // Create the serializer based on the type
                    var xmlSerializer = new XmlSerializer(obj.GetType());

                    // Open the file for writing
                    using (var fileStream = File.OpenWrite(path))
                        // Serialize the object to the file stream using the xml serializer
                        xmlSerializer.Serialize(fileStream, obj);
                    break;

                // XML string serialization
                case Serializer.XmlString:
                    // Convert to XML string and write to the path
                    Write(path, XmlStringSerializer.Serialize(obj));
                    break;

                // JSON serialization
                case Serializer.Json:
                    // Convert to JSON string and write to the path
                    Write(path, JsonConvert.SerializeObject(obj));
                    break;

                // Unknown
                default:
                    throw new UnknownEnumValueException();
            }
        }

        /// <summary>
        /// Reads the serialized content from the path and
        /// deserializes it with the wanted serializer.
        /// </summary>
        /// <typeparam name="T">Type of object that was serialized to the given path.</typeparam>
        /// <param name="path">Path to read the serialized content from.</param>
        /// <param name="serializer">Serializer to use for content deserialization.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="SerializationException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="UnknownEnumValueException"/>
        public T Deserialize<T>(string path, Serializer serializer)
        {
            // Don't allow the path to be null
            path.ThrowIfNull(nameof(path));

            // Get the path that may be affected by the base path
            path = GetPath(path);

            // Switch on the serializer that should be used
            switch (serializer)
            {
                // Binary deserialization
                case Serializer.Binary:
                    // Create the binary formatter
                    var formatter = new BinaryFormatter();

                    // Open file stream...
                    using (var fileStream = File.OpenRead(path))
                        // Deserialize the object from the file stream using the binary formatter and return it
                        return (T)formatter.Deserialize(fileStream);

                // XML stream deserialization
                case Serializer.XmlStream:
                    // Create the serializer based on the type
                    var xmlSerializer = new XmlSerializer(typeof(T));

                    // Open the file for reading
                    using (var fileStream = File.OpenRead(path))
                        // Deserialize the content from file into the object of specified type
                        return (T)xmlSerializer.Deserialize(fileStream);

                // XML string deserialization
                case Serializer.XmlString:
                    // Deserialize the XML from the file and return the result
                    return XmlStringSerializer.Deserialize<T>(ReadAllText(path));

                // JSON deserialization
                case Serializer.Json:
                    // Deserialize the JSON from the file and return the result
                    return JsonConvert.DeserializeObject<T>(ReadAllText(path));

                // Unknown
                default:
                    throw new UnknownEnumValueException();
            }
        }

        #region Overloads
        
        /// <summary>
        /// Writes an object to the given path.
        /// </summary>
        /// <param name="path">Path to write the serialized object content to.</param>
        /// <param name="obj">Object to serialize to the file.</param>
        /// <param name="serializer">Serializer to use.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="SerializationException"/>
        /// <exception cref="UnknownEnumValueException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="NullReferenceException"/>
        public void Serialize(TKey path, object obj, Serializer serializer)
            => Serialize(Paths[path], obj, serializer);

        /// <summary>
        /// Writes an object to the given path using the <see cref="Serializer"/>.
        /// </summary>
        /// <param name="path">Path to write the serialized object content to.</param>
        /// <param name="obj">Object to serialize to the file.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="SerializationException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="UnknownEnumValueException"/>
        public void Serialize(string path, object obj)
            => Serialize(path, obj, Serializer);

        /// <summary>
        /// Writes an object to the given path using the <see cref="Serializer"/>.
        /// </summary>
        /// <param name="path">Path to write the serialized object content to.</param>
        /// <param name="obj">Object to serialize to the file.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="SerializationException"/>
        /// <exception cref="UnknownEnumValueException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="NullReferenceException"/>
        public void Serialize(TKey path, object obj)
            => Serialize(Paths[path], obj, Serializer);

        /// <summary>
        /// Reads the serialized content from the path and
        /// deserializes it with the wanted serializer.
        /// </summary>
        /// <typeparam name="T">Type of object that was serialized to the given path.</typeparam>
        /// <param name="path">Path to read the serialized content from.</param>
        /// <param name="serializer">Serializer to use for content deserialization.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="SerializationException"/>
        /// <exception cref="UnknownEnumValueException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="NullReferenceException"/>
        public T Deserialize<T>(TKey path, Serializer serializer)
            => Deserialize<T>(Paths[path], serializer);

        /// <summary>
        /// Reads the serialized content from the path and
        /// deserializes it with the wanted serializer.
        /// </summary>
        /// <typeparam name="T">Type of object that was serialized to the given path.</typeparam>
        /// <param name="path">Path to read the serialized content from.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="SerializationException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="UnknownEnumValueException"/>
        public T Deserialize<T>(string path)
            => Deserialize<T>(path, Serializer);

        /// <summary>
        /// Reads the serialized content from the path and
        /// deserializes it with the wanted serializer.
        /// </summary>
        /// <typeparam name="T">Type of object that was serialized to the given path.</typeparam>
        /// <param name="path">Path to read the serialized content from.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="SerializationException"/>
        /// <exception cref="UnknownEnumValueException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="NullReferenceException"/>
        public T Deserialize<T>(TKey path)
            => Deserialize<T>(Paths[path], Serializer);

        #endregion

        #endregion

        #region Standard IO

        /// <summary>
        /// Writes all of the content as a single string to the given path.
        /// </summary>
        /// <param name="path">Path of the file to write the content to.</param>
        /// <param name="content">Content to write into the file.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        public void Write(string path, string content)
        {
            // Don't allow the parameters to be null
            Utils.ThrowIfParameterNull((path, nameof(path)), (content, nameof(content)));

            // Write the content to the specified file
            File.WriteAllText(GetPath(path), content);
        }

        /// <summary>
        /// Writes all of the content as a collection of lines to the given path.
        /// </summary>
        /// <param name="path">Path of the file to write the content to.</param>
        /// <param name="content">Content to write into the file line by line.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        public void Write(string path, IEnumerable<string> content)
        {
            // Don't allow the parameters to be null
            Utils.ThrowIfParameterNull((path, nameof(path)), (content, nameof(content)));

            // Write the content to the specified file
            File.WriteAllLines(GetPath(path), content);
        }

        /// <summary>
        /// Writes all of the content as a collection of objects that
        /// can be converted into lines to the given path.
        /// </summary>
        /// <param name="path">Path of the file to write the content to.</param>
        /// <param name="content">Content to write into the file line by line.</param>
        /// <param name="converter">Converter used for converting the object into a line.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        public void Write<T>(string path, IEnumerable<T> content, BaseSeparatorConverter<T> converter)
        {
            // Don't allow the parameters to be null
            Utils.ThrowIfParameterNull((path, nameof(path)), (content, nameof(content)), (converter, nameof(converter)));

            // Convert the objects using the converter
            var fileContent = content.Select(item => converter.ToLine(item));

            // Write the converted content to the specified file
            Write(path, fileContent);
        }

        /// <summary>
        /// Appends the given content to the end of the file at the given path.
        /// </summary>
        /// <param name="path">Path to append the content to.</param>
        /// <param name="content">Content to append to the end of the file.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        public void Append(string path, string content)
        {
            // Append the content to the file
            File.AppendAllText(GetPath(path), content);
        }

        /// <summary>
        /// Appends the given lines to the end of the file at the given path.
        /// </summary>
        /// <param name="path">Path to append the content to.</param>
        /// <param name="content">Lines to append to the end of the file.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        public void Append(string path, IEnumerable<string> content)
        {
            // Don't allow the parameters to be null
            Utils.ThrowIfParameterNull((path, nameof(path)), (content, nameof(content)));

            // Append the content to the file
            File.AppendAllLines(GetPath(path), content);
        }

        /// <summary>
        /// Appends the given objects to the end of the file at the given path by parsing them into lines.
        /// </summary>
        /// <param name="path">Path to append the content to.</param>
        /// <param name="content">Objects to parse and append to the end of the file.</param>
        /// <param name="converter">Converter used for converting the object into a line.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        public void Append<T>(string path, IEnumerable<T> content, BaseSeparatorConverter<T> converter)
        {
            // Don't allow the parameters to be null
            Utils.ThrowIfParameterNull((path, nameof(path)), (content, nameof(content)), (converter, nameof(converter)));

            // Append the content to the file
            Append(path, content.Select(item => converter.ToLine(item)));
        }

        /// <summary>
        /// Reads all of the file content and returns it as a single string.
        /// </summary>
        /// <param name="path">Path to the file whose content needs to be read</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        public string ReadAllText(string path)
        {
            // Don't allow the path to be null
            path.ThrowIfNull(nameof(path));

            // Read all of the content from the file
            return File.ReadAllText(GetPath(path));
        }

        /// <summary>
        /// Reads all of the file content and returns it as a collection of lines.
        /// </summary>
        /// <param name="path">Path to the file whose content needs to be read</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        public IEnumerable<string> ReadAllLines(string path)
        {
            // Don't allow the path to be null
            path.ThrowIfNull(nameof(path));

            // Read all of the content from the file
            return File.ReadAllLines(GetPath(path));
        }

        /// <summary>
        /// Reads all of the file content and returns it as a collection of objects
        /// that can be converted from lines.
        /// </summary>
        /// <param name="path">Path to the file whose content needs to be read</param>
        /// <param name="converter">Converter used for converting the line into an instance of the object.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        public IEnumerable<T> ReadAllObjects<T>(string path, BaseSeparatorConverter<T> converter)
        {
            // Don't allow the parameters to be null
            Utils.ThrowIfParameterNull((path, nameof(path)), (converter, nameof(converter)));

            // Read all of the lines from the file and convert them with the type's converter
            return ReadAllLines(path).Select(line => converter.FromLine(line));
        }

        #region Overloads

        /// <summary>
        /// Writes all of the content as a single string to the given path.
        /// </summary>
        /// <param name="path">Path of the file to write the content to.</param>
        /// <param name="content">Content to write into the file.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="NullReferenceException"/>
        public void Write(TKey path, string content)
            => Write(Paths[path], content);

        /// <summary>
        /// Writes all of the content as a collection of lines to the given path.
        /// </summary>
        /// <param name="path">Path of the file to write the content to.</param>
        /// <param name="content">Content to write into the file line by line.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="NullReferenceException"/>
        public void Write(TKey path, IEnumerable<string> content)
            => Write(Paths[path], content);

        /// <summary>
        /// Writes all of the content as a collection of objects that
        /// can be converted into lines to the given path.
        /// </summary>
        /// <param name="path">Path of the file to write the content to.</param>
        /// <param name="content">Content to write into the file line by line.</param>
        /// <param name="converter">Converter used for converting the line into an instance of the object.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="NullReferenceException"/>
        public void Write<T>(TKey path, IEnumerable<T> content, BaseSeparatorConverter<T> converter)
            => Write(Paths[path], content, converter);

        /// <summary>
        /// Appends the given content to the end of the file at the given path.
        /// </summary>
        /// <param name="path">Path to append the content to.</param>
        /// <param name="content">Content to append to the end of the file.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="NullReferenceException"/>
        public void Append(TKey path, string content)
            => Append(Paths[path], content);

        /// <summary>
        /// Appends the given lines to the end of the file at the given path.
        /// </summary>
        /// <param name="path">Path to append the content to.</param>
        /// <param name="content">Lines to append to the end of the file.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="NullReferenceException"/>
        public void Append(TKey path, IEnumerable<string> content)
            => Append(Paths[path], content);

        /// <summary>
        /// Appends the given objects to the end of the file at the given path by parsing them into lines.
        /// </summary>
        /// <param name="path">Path to append the content to.</param>
        /// <param name="content">Objects to parse and append to the end of the file.</param>
        /// <param name="converter">Converter used for converting the line into an instance of the object.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="NullReferenceException"/>
        public void Append<T>(TKey path, IEnumerable<T> content, BaseSeparatorConverter<T> converter)
            => Append(Paths[path], content, converter);

        /// <summary>
        /// Reads all of the file content and returns it as a single string.
        /// </summary>
        /// <param name="path">Path to the file whose content needs to be read</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="NullReferenceException"/>
        public string ReadAllText(TKey path)
            => ReadAllText(Paths[path]);

        /// <summary>
        /// Reads all of the file content and returns it as a collection of lines.
        /// </summary>
        /// <param name="path">Path to the file whose content needs to be read</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="NullReferenceException"/>
        public IEnumerable<string> ReadAllLines(TKey path)
            => ReadAllLines(Paths[path]);

        /// <summary>
        /// Reads all of the file content and returns it as a collection of objects
        /// that can be converted from lines.
        /// </summary>
        /// <param name="path">Path to the file whose content needs to be read</param>
        /// <param name="converter">Converter used for converting the line into an instance of the object.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="NullReferenceException"/>
        public IEnumerable<T> ReadAllObjects<T>(TKey path, BaseSeparatorConverter<T> converter)
            => ReadAllObjects(Paths[path], converter);

        #endregion

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the absolute path based on <see cref="BasePath"/>.
        /// </summary>
        /// <param name="providedPath">Path without the <see cref="BasePath"/>.</param>
        /// <returns></returns>
        public string GetPath(string providedPath)
        {
            var path = BasePath.IsNullOrEmpty()
                // If there is no base path, just return the provided path
                ? providedPath
                // Otherwise, combine the base path with the provided one
                : Path.Combine(BasePath, providedPath);

            // Return the absolute path
            return Path.GetFullPath(path);
        }

        /// <summary>
        /// Gets the absolute path based on <see cref="BasePath"/>.
        /// </summary>
        /// <param name="pathKey">Path without the <see cref="BasePath"/>.</param>
        /// <returns></returns>
        public string GetPath(TKey pathKey) => GetPath(Paths[pathKey]);

        #endregion 

        #endregion
    }
}
