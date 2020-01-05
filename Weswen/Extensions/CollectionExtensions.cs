using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Weswen
{
    /// <summary>
    /// Extension methods for collections (<see cref="IEnumerable"/> and beyond).
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Determines if a collection is null or has no elements.
        /// </summary>
        /// <typeparam name="T">Types stored in the collection.</typeparam>
        /// <param name="collection">Collection to check.</param>
        /// <returns>Whether or not the collection is null or empty.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
            => collection == null || collection.Count() == 0;

        /// <summary>
        /// Gets a random item from the collection.
        /// </summary>
        /// <typeparam name="T">Types stored in the collection.</typeparam>
        /// <param name="collection">Collection of items to get the random value from.</param>
        /// <param name="unwantedItems">Items (if any) that you want to exclude.</param>
        /// <returns>Random item from the collection</returns>
        public static T GetRandomItem<T>(this IEnumerable<T> collection, params T[] unwantedItems)
        {
            // If there are no elements or the collection is the same as the unwanted items...
            if (collection.IsNullOrEmpty() || collection.HasSameElementsAs(unwantedItems))
                // Return the default value
                return default;

            // Loop until an item, that isn't in the unwanted items list, is hit
            while (true)
            {
                // Get a random element from the collection
                var value = collection.ElementAt(Utils.Rng.Next(collection.Count()));

                // If unwanted items are null or they don't contain the value...
                if (unwantedItems?.Contains(value) != true)
                    // Return the value
                    return value;
            }
        }

        /// <summary>
        /// Checks if the two collections are equal in both count and elements.
        /// <para>The order doesn't matter.</para>
        /// </summary>
        /// <typeparam name="T">Types in the collections.</typeparam>
        /// <param name="first">First collection.</param>
        /// <param name="second">Collection to compare against.</param>
        /// <returns>Whether or not the two collection have same elements.</returns>
        /// <exception cref="NullReferenceException"/>
        public static bool HasSameElementsAs<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            // Don't allow null reference...
            first.ThrowIfNull();

            // If the second one is null, just return false
            if (second == null)
                return false;

            // Two collection have same elements if their count is equal and first
            // except second has no elements
            return first.Count() == second.Count() && first.Except(second).Count() == 0;
        }

        /// <summary>
        /// Checks if the item is inside a collection of values.
        /// </summary>
        /// <typeparam name="T">Type to check.</typeparam>
        /// <param name="item">Item to check.</param>
        /// <param name="targetValues">Collection to check the item against.</param>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="ArgumentNullException"/>
        public static bool In<T>(this T item, params T[] targetValues)
        {
            // Don't allow null reference
            item.ThrowIfNull();

            // Don't allow the array to be null
            targetValues.ThrowIfNull(nameof(targetValues));

            // Return whether or not the item is in the collection
            return targetValues.Contains(item);
        }
    }
}
