using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Weswen
{
    /// <summary>
    /// Specifies which serializer should be used.
    /// </summary>
    public enum Serializer
    {
        /// <summary>
        /// Use the <see cref="BinaryFormatter"/> for serialization.
        /// </summary>
        Binary,

        /// <summary>
        /// Use the <see cref="XmlSerializer"/> for serialization.
        /// </summary>
        XmlStream,

        /// <summary>
        /// Use the <see cref="XmlStringSerializer"/> for serialization.
        /// </summary>
        XmlString,

        /// <summary>
        /// Use the <see cref="JsonConvert"/> for serialization.
        /// </summary>
        Json
    }
}
