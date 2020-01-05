using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Weswen
{
    /// <summary>
    /// Collection of <see cref="TypeSpec{T}"/>s.
    /// </summary>
    public class TypeSpecCollection
    {
        #region Fields

        /// <summary>
        /// Since <see cref="TypeSpec{T}"/> is a generic class,
        /// a collection of objects will be needed to store diferrent
        /// <see cref="TypeSpec{T}"/>s.
        /// </summary>
        private ICollection<object> typeSpecs = new List<object>();

        /// <summary>
        /// Reference to the <see cref="GetLastValue{T}"/> method.
        /// </summary>
        private MethodInfo getLastValueMethod;

        /// <summary>
        /// Reference to the <see cref="GenerateValue{T}"/> method.
        /// </summary>
        private MethodInfo generateValueMethod;

        /// <summary>
        /// Reference to the <see cref="AddObjectToTypeSpec{T}(T)"/> method.
        /// </summary>
        private MethodInfo addObjectToTypeSpecMethod;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TypeSpecCollection()
        {
            // Get reference to the generic methods using reflection
            getLastValueMethod = GetType().GetAMethod(nameof(GetLastValue));
            addObjectToTypeSpecMethod = GetType().GetAMethod(
                nameof(AddObjectToTypeSpec),
                parameters => parameters.Length == 1 && parameters[0].ParameterType.IsGenericParameter
            );
            generateValueMethod = GetType().GetAMethod(nameof(GenerateValue));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Used to define type spec for a <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">Type to specify.</typeparam>
        /// <param name="typeSpec">Information about a <see cref="Type"/>.</param>
        /// <exception cref="ArgumentNullException"/>
        public void SetTypeSpec<T>(TypeSpec<T> typeSpec)
        {
            // Don't allow the type spec to be null
            typeSpec.ThrowIfNull(nameof(typeSpec));

            // Remove it from the list if it exists
            typeSpecs.Remove(typeSpec);
            // Add it to the list
            typeSpecs.Add(typeSpec);
        }

        /// <summary>
        /// Used to define type spec for a <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">Type to specify.</typeparam>
        /// <param name="generator">Generator that will be used in the <see cref="TypeSpec{T}"/>.</param>
        /// <exception cref="ArgumentNullException"/>
        public void SetTypeSpec<T>(Expression<Func<T>> generator)
            => SetTypeSpec(new TypeSpec<T> { Generator = generator });

        /// <summary>
        /// Tries to get the <see cref="TypeSpec{T}"/> from <see cref="typeSpecs"/>.
        /// If it doesn't find it, a new one is created, added to the list and then returned.
        /// </summary>
        /// <typeparam name="T">Type whose spec is required.</typeparam>
        /// <returns><see cref="TypeSpec{T}"/> for the type <typeparamref name="T"/>.</returns>
        public TypeSpec<T> GetTypeSpec<T>()
        {
            // Declare the type spec and initialize it to null
            TypeSpec<T> typeSpec = null;

            // Foreach spec in defined specs...
            foreach (var spec in typeSpecs)
                // If it is a spec that defines type T...
                if (spec is TypeSpec<T> t)
                {
                    // Assign it to the type spec local variable
                    typeSpec = t;

                    // Stop searching for the matching spec
                    break;
                }

            // If the spec wasn't found...
            if (typeSpec == null)
            {
                // Declare one that uses default implementation
                typeSpec = new TypeSpec<T>();

                // Add it to the type spec list
                typeSpecs.Add(typeSpec);
            }

            // Return the type spec
            return typeSpec;
        }

        /// <summary>
        /// Gets the last stored value of type <typeparamref name="T"/>
        /// stored into the <see cref="TypeSpec{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type to fetch.</typeparam>
        /// <returns><see cref="TypeSpec{T}.LastValue"/></returns>
        public T GetLastValue<T>() => GetTypeSpec<T>().LastValue;

        /// <summary>
        /// Gets the last stored value of the given <paramref name="typeSpecType"/>.
        /// </summary>
        /// <param name="typeSpecType">Type to get the value for.</param>
        /// <returns>Last value stored into the type spec for given type.</returns>
        /// <exception cref="ArgumentNullException"/>
        public object GetLastValue(Type typeSpecType)
        {
            // Don't allow nulls
            typeSpecType.ThrowIfNull(nameof(typeSpecType));

            return getLastValueMethod.MakeGenericMethod(typeSpecType).Invoke(this, null);
        }

        /// <summary>
        /// Adds an object of type <typeparamref name="T"/> to the
        /// <see cref="TypeSpec{T}.HistoryOfValues"/> collection.
        /// </summary>
        /// <typeparam name="T">Type of object to add.</typeparam>
        /// <param name="objectToAdd">Object to add to the <see cref="TypeSpec{T}"/>.</param>
        public void AddObjectToTypeSpec<T>(T objectToAdd)
            => GetTypeSpec<T>().HistoryOfValues.Add(objectToAdd);

        /// <summary>
        /// Adds an object of the specified type to the
        /// <see cref="TypeSpec{T}.HistoryOfValues"/> collection.
        /// </summary>
        /// <param name="type">Type of object to add.</param>
        /// <param name="objectToAdd">Object to add to the <see cref="TypeSpec{T}"/>.</param>
        /// <exception cref="ArgumentNullException"/>
        public void AddObjectToTypeSpec(Type type, object objectToAdd)
        {
            // Don't allow the type to be null
            type.ThrowIfNull(nameof(type));

            // Make a generic version of the method
            var method = addObjectToTypeSpecMethod.MakeGenericMethod(type);

            //Invoke it with the given parameter
            method.Invoke(this, new[] { objectToAdd });
        }

        /// <summary>
        /// Generates an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to create an instance for.</typeparam>
        /// <returns>Instance of type <typeparamref name="T"/></returns>
        public T GenerateValue<T>()
            => GetTypeSpec<T>().GenerateValue();

        /// <summary>
        /// Generates an instance of the specified type.
        /// </summary>
        /// <param name="type">Type to create an instance for.</param>
        /// <returns>Instance of the specified type</returns>
        /// <exception cref="ArgumentNullException"/>
        public object GenerateValue(Type type)
        {
            // Don't allow the type to be null
            type.ThrowIfNull(nameof(type));

            // Create a generic method with the type and invoke it
            return generateValueMethod.MakeGenericMethod(type).Invoke(this, null);
        } 

        #endregion
    }
}
