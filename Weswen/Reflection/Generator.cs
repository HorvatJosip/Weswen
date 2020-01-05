using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Weswen
{
    /// <summary>
    /// Used to generate data (usually random) for fields and properties.
    /// </summary>
    public class Generator
    {
        #region Fields

        private TypeSpecCollection typeSpecs = new TypeSpecCollection();

        #endregion

        #region Properties

        /// <summary>
        /// Singleton instance of this class.
        /// </summary>
        public static Generator Instance { get; } = new Generator();

        /// <summary>
        /// Defines when should the generator go through the object's
        /// hierarchy and initialize all of the objects found in it.
        /// <para>By default, hierarchy won't be initialized if the object's type
        /// is primitive, a collection, or one of the following types:
        /// <see cref="string"/>, <see cref="DateTime"/>, <see cref="TimeSpan"/></para>
        /// <para>If this is set to null, hierarchy will be always initialized.</para>
        /// </summary>
        public Expression<Func<Type, bool>> InitializeHierarchy { get; set; }

        /// <summary>
        /// Defines if the generator should skip the backing fields while generating
        /// values. This means that properties that don't have set method can still
        /// be randomly generated.
        /// </summary>
        public bool SkipBackingFields { get; set; }

        #endregion

        #region Constructor

        private Generator()
        {
            // Set default type specs for some simple types
            SetTypeSpec(() => Utils.Rng.Next(-10000, 10000)); // int
            SetTypeSpec(() => Utils.Rng.Next(-10000, 10000) / 7.1261); // double
            SetTypeSpec(() => Utils.Rng.Next(-10000, 10000) / -51.221m); // decimal
            SetTypeSpec(() => Utils.AlphaNumericsWithUpper.GetRandomItem()); // char
            SetTypeSpec(() => Utils.Rng.Next(2) == 0); // bool
            SetTypeSpec(() => Utils.CreateRandomString(Utils.AlphaNumericsWithUpper + " ")); // string
            SetTypeSpec(() => DateTime.Now.AddSeconds(Utils.Rng.Next(int.MinValue, int.MaxValue))); // DateTime
            SetTypeSpec(() => DateTime.Now.AddSeconds(Utils.Rng.Next(int.MinValue, int.MaxValue)).TimeOfDay); // TimeSpan
            SetTypeSpec(() => Guid.NewGuid()); // Guid

            // Define when should the hierarchy be initialized
            InitializeHierarchy = type => !(
                type.IsPrimitive ||
                type.In(typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)) ||
                type.GetCollectionInfo().IsCollection
            );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method used for generating an instance of type <typeparamref name="T"/>
        /// with all of its fields and properties set using the <see cref="TypeSpec{T}"/>s.
        /// </summary>
        /// <typeparam name="T">Type that will be generated.</typeparam>
        /// <returns>Instance of type <typeparamref name="T"/>.</returns>
        public T Generate<T>()
        {
            // Get the type spec from the collection
            var typeSpec = typeSpecs.GetTypeSpec<T>();

            // Use it to create an instance of object of type T
            var instance = typeSpec.GetInstance();

            // Check if the hierarchy should also be initialized
            if (InitializeHierarchy?.Compile().Invoke(typeof(T)) == false)
                // If not, just return the instance
                return instance;

            // Set all of the prields of the instance in its hierarchy
            Utils.TraverseThroughObjectsHierarchy(instance, info =>
            {
                // Ignore the root object
                if (info.FirstCall)
                    return false;

                // If the prield doesn't have the NotGenerated attribute defined...
                if (!Attribute.IsDefined(info.CurrentPrield.Member, typeof(NotGeneratedAttribute)))
                {
                    // Generate the new value from the prield's type
                    var newValue = typeSpecs.GenerateValue(info.CurrentPrield.Type);

                    // Set the new value to the prield
                    info.CurrentPrield.SetValue(info.Parent, newValue);
                }

                // Alias for the prield's type
                var type = info.CurrentPrield.Type;

                // Don't go into recursion if the type is primitive, the same as the parent's one or a collection
                return !(type.IsPrimitive || type == type.DeclaringType || type.GetCollectionInfo().IsCollection);
            }, skipBackingFields: SkipBackingFields);

            // Return the instance
            return instance;
        }

        /// <summary>
        /// Defines or overrides the type spec for the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type that the type spec is being defined for.</typeparam>
        /// <param name="typeSpec">Type spec to define or override.</param>
        public void SetTypeSpec<T>(TypeSpec<T> typeSpec)
            => typeSpecs.SetTypeSpec(typeSpec);

        /// <summary>
        /// Defines or overrides the type spec for the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type that the type spec is being defined for.</typeparam>
        /// <param name="generator">Generator that will be used in the <see cref="TypeSpec{T}"/>.</param>
        public void SetTypeSpec<T>(Expression<Func<T>> generator)
            => typeSpecs.SetTypeSpec(generator);

        /// <summary>
        /// Gets one instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to get the instance for.</typeparam>
        /// <returns>Instance of type <typeparamref name="T"/></returns>
        public static T One<T>() => Instance.Generate<T>();

        /// <summary>
        /// Gets random amount of instances of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to get a collection for.</typeparam>
        /// <returns>Collection of items of type <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> Many<T>() => Utils.CreateCollection<List<T>>();

        /// <summary>
        /// Gets <paramref name="count"/> instances of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to get a collection for.</typeparam>
        /// <param name="count">Number of items that will be put into the collection.
        /// <para>Has to be at least 0.</para></param>
        /// <param name="differentEachTime">If this is false, the object will be created once and repeated
        /// until the collection is filled.</param>
        /// <param name="progress">Progress callback.</param>
        /// <returns>Collection of items of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static IEnumerable<T> Many<T>(long count, bool differentEachTime = true, IProgress<double> progress = null)
        {
            // Don't allow the count to be less than zero
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            // Local method for reporting progress
            void ReportProgress(int current)
            {
                // Calculate the percentage of items generated
                var percentageDone = 100.0 * current / count;

                // Report the progress
                progress.Report(percentageDone);
            }

            // Create the collection for the items and initialize it with default values
            var items = new T[count];

            // If the list should be filled with different items...
            if (differentEachTime)
                // Loop count times...
                for (int i = 0; i < count; i++)
                {
                    // Generate an item and add it to the list
                    items[i] = Instance.Generate<T>();

                    // If there is a progress callback...
                    if (progress != null)
                        // Report the progress
                        ReportProgress(i + 1);
                }

            // Otherwise...
            else
            {
                // Generate an instance
                var instance = One<T>();

                // Loop count times...
                for (int i = 0; i < count; i++)
                {
                    // Add the instance to the array
                    items[i] = instance;

                    // If there is a progress callback...
                    if(progress != null)
                        // Report the progress
                        ReportProgress(i + 1);
                }
            }

            // Return the items
            return items;
        }

        #endregion
    }
}
