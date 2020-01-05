using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Weswen
{
    /// <summary>
    /// Collection of extensions for the <see cref="Type"/> class.
    /// </summary>
    public static class TypeExtensions
    {
        #region Constants

        /// <summary>
        /// Default <see cref="BindingFlags"/> used for getting fields and properties.
        /// </summary>
        public const BindingFlags DefaultPrieldFlags =
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Default <see cref="BindingFlags"/> used for getting methods.
        /// </summary>
        public const BindingFlags DefaultMethodFlags =
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static |
            BindingFlags.InvokeMethod | BindingFlags.Instance;

        #endregion

        #region Methods

        /// <summary>
        /// Used to get all the fields and properties defined by a type.
        /// </summary>
        /// <param name="type">Type to get the fields and properties from.</param>
        /// <param name="skipBackingFields">If there are backing fields for the properties,
        /// should they be skipped or not?</param>
        /// <param name="flags"><see cref="BindingFlags"/> used for accessing the fields and properties.</param>
        /// <returns>Collection of fields and properties.</returns>
        /// <exception cref="NullReferenceException"/>
        public static List<Prield> GetPrields(this Type type, bool skipBackingFields = true, BindingFlags flags = DefaultPrieldFlags)
        {
            // Don't allow null reference
            type.ThrowIfNull();

            // Generate prields out of fields and properties
            var fields = Prield.Generate(type.GetFields(flags));
            var properties = Prield.Generate(type.GetProperties(flags));

            // If backing fields need to be skipped...
            if (skipBackingFields)
                // Remove backing fields from the fields collection
                fields = fields.Where(field => !field.Member.Name.Contains("k__BackingField")).ToList();

            // Group them together
            properties.AddRange(fields);

            // Return all of the prields
            return properties;
        }

        /// <summary>
        /// Gets a value from a field or a property on a target object.
        /// </summary>
        /// <param name="type">Type that declares the field or property.</param>
        /// <param name="prieldName">Name of the field or property to get the value from.</param>
        /// <param name="target">Object to get the value from.</param>
        /// <param name="indexerParams">Parameters for the indexer (if the property is an indexer).</param>
        /// <returns>Value from the target object of the specified field or property</returns>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="ArgumentNullException"/>
        public static object GetValue(this Type type, string prieldName, object target, object[] indexerParams = null)
        {
            // Don't allow null reference
            type.ThrowIfNull();

            // Don't allow nulls
            prieldName.ThrowIfNull(nameof(prieldName));

            // Get the prields of the type
            var prields = type.GetPrields();

            // Find the one that matches the name in the parameters
            var targetPrield = prields.Find(prield => prield.Member.Name == prieldName);

            // If it wasn't found, return null, otherwise get value of the prield from the target
            return targetPrield?.GetValue(target, indexerParams);
        }

        /// <summary>
        /// Gets a method based on name and conditions for the parameters.
        /// </summary>
        /// <param name="type">Type to get the method from.</param>
        /// <param name="name">Name of the method.</param>
        /// <param name="parameterConditions">Defines conditions for parameters of the method.</param>
        /// <returns>Method that matches the given name and parameter conditions.</returns>
        /// <param name="flags"><see cref="BindingFlags"/> used for accessing the methods.</param>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="ArgumentNullException"/>
        public static MethodInfo GetAMethod(this Type type, string name, Func<ParameterInfo[], bool> parameterConditions,
            BindingFlags flags = DefaultMethodFlags)
        {
            // Don't allow null reference
            type.ThrowIfNull();

            // Don't allow nulls
            Utils.ThrowIfParameterNull(
                (name, nameof(name)),
                (parameterConditions, nameof(parameterConditions))
            );

            return type
                // Get all of the methods using the binding flags
                .GetMethods(flags)
                // Get all of the overloads by name
                .Where(method => method.Name == name)
                // Return the first one to match the parameter conditions
                .FirstOrDefault(method => parameterConditions(method.GetParameters()));
        }

        /// <summary>
        /// Gets a method based on name and the types it takes in as parameters.
        /// </summary>
        /// <param name="type">Type to get the method from.</param>
        /// <param name="name">Name of the method.</param>
        /// <param name="parameterTypes">Types that the method takes in as parameters.</param>
        /// <returns>Method that matches the given name and parameter types.</returns>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="ArgumentNullException"/>
        public static MethodInfo GetAMethod(this Type type, string name, params Type[] parameterTypes)
            => GetAMethod(type, name, parameters =>
            {
                // If the method should have no parameters...
                if (parameterTypes.IsNullOrEmpty())
                    // Return true if the method has no parameters, false otherwise
                    return parameters.Length == 0;

                // Get the parameter types from the current method
                var methodParameterTypes = parameters.Select(parameter => parameter.ParameterType);

                // Return whether or not all of the types match
                return parameterTypes.HasSameElementsAs(methodParameterTypes);
            });

        /// <summary>
        /// Gets collection information about a type.
        /// </summary>
        /// <param name="type">Type to test if it is a collection.</param>
        /// <returns>Instance of <see cref="CollectionInfo"/> about the <paramref name="type"/>.</returns>
        /// <exception cref="NullReferenceException"/>
        public static CollectionInfo GetCollectionInfo(this Type type)
        {
            // Don't allow null reference
            type.ThrowIfNull();

            // Return collection information about the type
            return new CollectionInfo(type);
        } 

        /// <summary>
        /// Gets the content of the property that should be a table cell, but if
        /// <paramref name="fallbackIfNotCell"/> is true, a value from the property
        /// will be read even if the property doesn't have <see cref="TableCellAttribute"/>.
        /// </summary>
        /// <param name="property">Property to get the content from.</param>
        /// <param name="instance">Instance of the object that contains the property to get the value from.</param>
        /// <param name="fallbackIfNotCell">If the property doesn't have <see cref="TableCellAttribute"/>,
        /// should the value from the property be used? (if false, just returns null)</param>
        /// <returns></returns>
        public static string GetTableCellContent(this PropertyInfo property, object instance, bool fallbackIfNotCell = true)
        {
            // Don't allow nulls
            property.ThrowIfNull();
            instance.ThrowIfNull(nameof(instance));

            // Get the value of the property from the instance
            var propertyValue = property.GetValue(instance)?.ToString();

            // Get the attribute from the property
            var cell = property.GetCustomAttribute<TableCellAttribute>();

            // If the attribute isn't defined...
            if (cell == null)
                // If the property fallback should be used...
                return fallbackIfNotCell
                    // Return the value of the property
                    ? propertyValue
                    // Otherwise, return null
                    : null;

            // Content of the cell
            string content;

            // If the content is defined by the attribute...
            if (cell.Content != null)
            {
                // If the property content should be used over the attribute content...
                if (cell.PropertyValueOverAttributeValue)
                    // Use the property's value, unless it's null, then fallback to the attribute's value
                    content = propertyValue ?? cell.Content;

                // Otherwise...
                else
                    // Use the content defined by the attribute
                    content = cell.Content;
            }

            // Otherwise...
            else
                // Set the content to the property value
                content = propertyValue;

            // Return the content
            return content;
        }

        #endregion
    }
}
