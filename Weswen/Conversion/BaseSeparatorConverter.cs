using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Weswen
{
    /// <summary>
    /// Converter used for converting an object into a separator separated line
    /// and vice versa.
    /// </summary>
    /// <typeparam name="T">Type that the converter is supposed to convert.</typeparam>
    public abstract class BaseSeparatorConverter<T>
    {
        /// <summary>
        /// Validator (filter) used while getting the properties.
        /// If this returns false, property is skipped.
        /// If this is null, none of the properties are skipped.
        /// </summary>
        protected Func<PropertyInfo, bool> _propertyValidator;

        /// <summary>
        /// Separator used for separating items in a line.
        /// </summary>
        public string Separator { get; set; } = ",";

        #region Methods

        /// <summary>
        /// Converts the given item into a line separated by <see cref="Separator"/>.
        /// By default, just separates values stored in properties using <see cref="string.Join(string, object[])"/>.
        /// </summary>
        /// <param name="item">Item to convert into a line.</param>
        /// <returns></returns>
        public virtual string ToLine(T item)
        {
            // If there is no separator...
            if (Separator.IsNullOrEmpty())
                // Throw invalid operation excecption
                throw new InvalidOperationException();

            // Get the properties from the type
            var properties = GetProperties();

            // Get the properties' values from the item
            var values = properties.Select(prop => prop.GetValue(item));

            // Separate the values with the separator and return the line
            return string.Join(Separator, values);
        }

        /// <summary>
        /// Converts the given line separated by <see cref="Separator"/> into
        /// the item of type <typeparamref name="T"/>. By default, splits by the
        /// <see cref="Separator"/> and stores values in properties.
        /// </summary>
        /// <param name="line">Line to convert into an item of type <typeparamref name="T"/>.</param>
        /// <returns></returns>
        public virtual T FromLine(string line)
        {
            // Create an instance of the type T
            var instance = Utils.TryGetInstantiatedObject<T>();

            // If there is no separator or the instance couldn't be created...
            if (Separator.IsNullOrEmpty() || instance == null)
                // Throw invalid operation excecption
                throw new InvalidOperationException();

            var lineContent = Separator.Length == 1
                // If the separator is a char, use it for splitting
                ? line.Split(Separator[0])
                // Otherwise, use the whole string
                : line.Split(new[] { Separator }, StringSplitOptions.None);

            // Get the properties
            var properties = GetProperties();

            // If the property count doesn't match the line content count...
            if (properties.Count() != lineContent.Length)
                // Throw the argument exception
                throw new ArgumentException("Number of values in a line doesn't match number of properties", nameof(line));

            // Foreach property on the type...
            for (int i = 0; i < lineContent.Length; i++)
            {
                // Get the property
                var prop = properties.ElementAt(i);

                // Set its value to the current line value converted from string
                prop.SetValue(instance, lineContent[i].ConvertFromString(prop.PropertyType));
            }

            // Return the instance
            return instance;
        }

        private IEnumerable<PropertyInfo> GetProperties()
        {
            // Get the properties from the type
            var properties = typeof(T).GetProperties();

            // If there is a property validator...
            if (_propertyValidator != null)
                // Filter the properties and return them
                return properties.Where(_propertyValidator);

            // Otherwise just return the properties
            return properties;
        } 

        #endregion
    }

    /// <summary>
    /// Converter used for converting an object into a separator separated line
    /// and vice versa. Provides an instance of the separator.
    /// </summary>
    /// <typeparam name="T">Converter's type (the one inheriting this class).</typeparam>
    /// <typeparam name="U">Type that the converter is supposed to convert.</typeparam>
    public abstract class BaseSeparatorConverter<T, U> : BaseSeparatorConverter<U> 
		where T : BaseSeparatorConverter<T, U>, new()
    {
        /// <summary>
        /// Instance of the separator converter.
        /// </summary>
        public static T Instance { get; } = new T();
    }
}


