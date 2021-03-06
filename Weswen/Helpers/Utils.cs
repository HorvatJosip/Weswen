using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;

namespace Weswen
{
    /// <summary>
    /// Collection of useful methods.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Reference to <see cref="Generator.Generate{T}"/> method.
        /// </summary>
        private static MethodInfo generateMethod = typeof(Generator).GetAMethod(nameof(Generator.Generate));

        #region Properties

        /// <summary>
        /// Random number generator.
        /// </summary>
        public static Random Rng { get; } = new Random();

        /// <summary>
        /// Lowercase version of English alphabet.
        /// </summary>
        public static string EnglishAlphabet { get; } = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Lowercase version of <see cref="EnglishAlphabet"/> with few extra characters (čćžšđ).
        /// </summary>
        public static string Characters { get; } = EnglishAlphabet + "čćžšđ";

        /// <summary>
        /// String of numbers 0, 1, 2, 3, 4, 5, 6, 7, 8 and 9.
        /// </summary>
        public static string Numbers { get; } = "0123456789";

        /// <summary>
        /// <see cref="EnglishAlphabet"/> combined with <see cref="Numbers"/>.
        /// </summary>
        public static string AlphaNumerics { get; } = EnglishAlphabet + Numbers;

        /// <summary>
        /// <see cref="EnglishAlphabet"/> both lowercase and uppercase combined with <see cref="Numbers"/>.
        /// </summary>
        public static string AlphaNumericsWithUpper { get; } = EnglishAlphabet + EnglishAlphabet.ToUpper() + Numbers;

        #endregion

        #region Throw Helpers

        /// <summary>
        /// Helper for throwing <see cref="ArgumentNullException"/> for
        /// multiple parameters.
        /// </summary>
        /// <param name="parameters">Parameters to test.</param>
        /// <exception cref="ArgumentNullException"/>
        public static void ThrowIfParameterNull(params (object obj, string paramName)[] parameters)
        {
            // If the parameters are null, throw argument null exception
            parameters.ThrowIfNull(nameof(parameters));

            // Foreach parameter key value pair...
            foreach (var param in parameters)
                // Throw if the value is null and pass in its name
                param.obj.ThrowIfNull(param.paramName);
        }

        /// <summary>
        /// Helper for throwing <see cref="NullReferenceException"/> for
        /// multiple objects.
        /// </summary>
        /// <param name="objectsToTest">Objects to test.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="NullReferenceException"/>
        public static void ThrowIfNull(params object[] objectsToTest)
        {
            // If the parameters are null, throw argument null exception
            objectsToTest.ThrowIfNull(nameof(objectsToTest));

            // Foreach object to test...
            foreach (var obj in objectsToTest)
                // Throw null reference exception if it is null
                obj.ThrowIfNull();
        }


        #endregion

        #region Randomness

        /// <summary>
        /// Creates a collection filled with random data.
        /// </summary>
        /// <typeparam name="T">Types that are stored in the collection.</typeparam>
        /// <returns>Collection filled with random data.</returns>
        public static IEnumerable<T> CreateListWithRandomData<T>()
            => CreateListWithRandomData<T>(Rng.Next(5, 50));

        /// <summary>
        /// Creates a collection filled with random data.
        /// </summary>
        /// <typeparam name="T">Types that are stored in the collection.</typeparam>
        /// <param name="count">Number of elements to put into the collection.</param>
        /// <returns>Collection filled with random data.</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static IEnumerable<T> CreateListWithRandomData<T>(int count)
        {
            // Don't allow the count to be less than zero
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            // Create a list of items of type T
            return new byte[count].Select(item => Generator.Instance.Generate<T>()).ToList();
        }

        /// <summary>
        /// Creates random data based on the types passed in.
        /// </summary>
        /// <param name="types">Types to generate the data for.</param>
        /// <returns>An array of objects generated by <see cref="Generator"/>.</returns>
        public static object[] CreateRandomData(params Type[] types)
        {
            // Create an array to match the size of type array
            var array = new object[types?.Length ?? 0];

            // Go through array
            for (int i = 0; i < array.Length; i++)
                // Generate an object and place it into the array
                array[i] = generateMethod
                    .MakeGenericMethod(types[i])
                    .Invoke(Generator.Instance, null);

            // Return the array
            return array;
        }

        /// <summary>
        /// Creates a random string out of allowed characters of given length.
        /// </summary>
        /// <param name="length">Length of the string to generate.</param>
        /// <param name="allowedCharacters">Characters that will be used in the generation.</param>
        /// <returns>Random string out of allowed characters.</returns>
        public static string CreateRandomString(int length, string allowedCharacters)
        {
            var characters = new char[length];

            for (int i = 0; i < length; i++)
                characters[i] = allowedCharacters.GetRandomItem();

            return string.Concat(characters);
        }

        /// <summary>
        /// Creates a random string out of allowed characters of random length.
        /// </summary>
        /// <param name="allowedCharacters">Characters that will be used in the generation.</param>
        /// <returns>Random string out of allowed characters.</returns>
        public static string CreateRandomString(string allowedCharacters)
            => CreateRandomString(Rng.Next(5, 20), allowedCharacters);

        /// <summary>
        /// Does a roll and returns true if the value is within the given percentage.
        /// </summary>
        /// <param name="percentage">Percent chance for success.</param>
        /// <returns>If the value that was rolled is success.</returns>
        public static bool PercentageRoll(double percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage));

            return Rng.Next(101) <= percentage;
        }

        #endregion

        #region Reflection

        /// <summary>
        /// Traverses through the object's hierarchy and provides information about
        /// each <see cref="Prield"/> in the hierarchy as it goes through it.
        /// </summary>
        /// <param name="obj">Object to traverse.</param>
        /// <param name="objectChanged"><para>Method that gets called every time a new object
        /// in the hierarchy is being processed.</para>
        /// <para>If it returns true, the recursion will occur.</para>
        /// <para>There are specific cases where the return value won't matter: call on the
        /// root element (because it always has to go into recursion) and when the
        /// prield's value cannot be read because its parent is null.</para></param>
        /// <param name="skipBackingFields">Should the backing fields of properties be skipped or not?</param>
        /// <param name="skipNullPrieldsChildren">Should there be iteration through prield's
        /// children if its value is null?</param>
        /// <param name="flags"><see cref="BindingFlags"/> used for accessing the fields and properties.</param>
        /// <exception cref="StackOverflowException"/>
        /// <exception cref="ArgumentNullException"/>
        public static void TraverseThroughObjectsHierarchy(
            object obj, Func<TraversionInfo, bool> objectChanged, bool skipBackingFields = true,
            bool skipNullPrieldsChildren = true, BindingFlags flags = TypeExtensions.DefaultPrieldFlags)
        {
            // Don't allow nulls
            obj.ThrowIfNull(nameof(obj));

            // Create a new type spec collection
            var typeSpecs = new TypeSpecCollection();

            // Add the initial object to the type spec collection
            typeSpecs.AddObjectToTypeSpec(obj.GetType(), obj);

            // Declare the current level in the hierarchy to the top level value
            var level = TraversionInfo.TopLevel;

            // Call the object changed with the hierarchy root
            objectChanged?.Invoke(new TraversionInfo(null, obj, null, level));

            // Start the recursion
            Recursion(obj.GetType());

            // Inner function used for recursion
            void Recursion(Type type)
            {
                // Increase the level
                level++;

                // Find the parent's value
                var parent = typeSpecs.GetLastValue(type);

                // Skip iterating throught prields only if null skip is on and the parent is null
                if (!(skipNullPrieldsChildren && parent == null))
                {

                    // Get all of the prields for current type
                    var prields = type.GetPrields(skipBackingFields, flags);

                    // Loop through all of the fields and properties
                    foreach (var prield in prields)
                    {
                        if (parent == null)
                        {
                            // Notify that the prield's value cannot be read since the parent is null
                            objectChanged?.Invoke(new TraversionInfo(null, null, prield, level));

                            // Skip to the next prield
                            continue;
                        }

                        // Get the value of the current prield from the parent
                        var current = prield.GetValue(parent);

                        // Create traversion info based on the values
                        var traversionInfo = new TraversionInfo(parent, current, prield, level);

                        // Check if the recursion should be triggered in the current state
                        var triggerRecursion = objectChanged?.Invoke(traversionInfo) == true;

                        // Get the updated value of the current prield from the parent
                        var updatedValue = prield.GetValue(parent);

                        // Memorize the updated value in the type spec collection
                        typeSpecs.AddObjectToTypeSpec(prield.Type, updatedValue);

                        // If the recursion should be triggered...
                        if (triggerRecursion)
                            // Trigger the recursion for the current prield's type
                            Recursion(prield.Type);

                    } // foreach

                } // if skip nulls

                // Reduce the level after the current type's prields were traversed through
                level--;
            }
        }

        /// <summary>
        /// Tries to get an instance of the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to get the instance for.</typeparam>
        /// <returns>Instance of type <typeparamref name="T"/> or null.</returns>
        public static T TryGetInstantiatedObject<T>()
        {
            // Find the default value for the type
            T defaultValue = default;

            // If the default value isn't null, just return it
            if (defaultValue != null)
                return defaultValue;

            // Try to return an instance created by default constructor
            try { return Activator.CreateInstance<T>(); }
            catch
            {
                // Try to instantiate it with one of its constructors
                foreach (var constructor in typeof(T).GetConstructors())
                {
                    try
                    {
                        // Try to return the instance created with that consutructor with random data
                        return (T)constructor.Invoke(
                            CreateRandomData(constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray())
                        );
                    }
                    catch { }
                }

                // If everything fails, return null
                return defaultValue;
            }
        }

        /// <summary>
        /// Generates an instance from a constructor for the type <typeparamref name="T"/>
        /// using the <paramref name="constructorParameterTypes"/> and calls it with
        /// values that were passed in by index. those that weren't passed in,
        /// it generates using the <see cref="Generator"/>.
        /// </summary>
        /// <typeparam name="T">Type to get the instance for.</typeparam>
        /// <param name="constructorParameterTypes">Types that need to be passed into
        /// the constructor.</param>
        /// <param name="constructorData">Some or all of the data that goes into the constructor</param>
        /// <returns>Instance of type <typeparamref name="T"/></returns>
        /// <exception cref="MemberAccessException"/>
        /// <exception cref="MethodAccessException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="TargetInvocationException"/>
        /// <exception cref="TargetParameterCountException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="InvalidOperationException"/>
        public static T InvokeConstructor<T>(
            Type[] constructorParameterTypes,
            params (int parameterIndex, object parameterValue)[] constructorData
        )
        {
            // Don't allow the parameter types to be null
            constructorParameterTypes.ThrowIfNull(nameof(constructorParameterTypes));

            // Find the constructor with reflection
            var constructor = typeof(T).GetConstructor(constructorParameterTypes);

            // Get an array of objects for the constructor parameter types
            var constructorParameters = CreateRandomData(constructorParameterTypes);

            // If there are some parameters passed in...
            if (!constructorData.IsNullOrEmpty())
                // Loop through the generated parameters...
                for (int i = 0; i < constructorParameters.Length; i++)
                    // Loop through the passed in parameters...
                    foreach (var (parameterIndex, parameterValue) in constructorData)
                        // If the index and the type match...
                        if (parameterIndex == i &&
                            parameterValue?.GetType() == constructorParameterTypes[i])
                        {
                            // Update the parameter value with the passed in one
                            constructorParameters[i] = parameterValue;

                            // Stop the search for the parameter at this index
                            break;
                        }

            // Create an instance of that type using the constructor
            return (T)constructor.Invoke(constructorParameters);
        }

        /// <summary>
        /// Creates a collection like <see cref="HashSet{T}"/>. In that case, type
        /// parameter (<typeparamref name="T"/>) would be <see cref="HashSet{T}"/>.
        /// <para>This works for collections that are arrays (assignable from 
        /// <see cref="Array"/>) or that have one type parameter and can be created
        /// using constructor that takes in <see cref="IEnumerable{T}"/>.</para>
        /// </summary>
        /// <typeparam name="T">Collection type to create, e.g. <see cref="HashSet{T}"/>.</typeparam>
        /// <returns>List with data made using <see cref="CreateListWithRandomData{T}()"/>.</returns>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="MemberAccessException"/>
        /// <exception cref="MethodAccessException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="TargetInvocationException"/>
        /// <exception cref="TargetParameterCountException"/>
        /// <exception cref="SecurityException"/>
        public static T CreateCollection<T>()
        {
            // Get information about the type - is it a collection or not
            var collectionInfo = typeof(T).GetCollectionInfo();

            // Make sure that T is actually an array or a collection with only one generic argument
            if (!(collectionInfo.IsArray || (collectionInfo.IsCollection && collectionInfo.CollectionTypes.Length == 1)))
                return default;

            // If it is an array
            if (collectionInfo.IsArray)
            {
                // Get the type of the array
                var arrayType = collectionInfo.ArrayType;

                // Create the array with random size
                var array = Array.CreateInstance(arrayType, Rng.Next(5, 50));

                // Get the generator method for the array type
                var generatorMethod = generateMethod.MakeGenericMethod(arrayType);

                //Loop through the array
                for (int i = 0; i < array.Length; i++)
                    // Fill the elements using the generator method
                    array.SetValue(generatorMethod.Invoke(Generator.Instance, null), i);

                // Return the array converted to T[]
                return (T)Convert.ChangeType(array, typeof(T));
            }
            else // If it is an IEnumerable collection
            {
                // Get the type of collection
                var collectionType = collectionInfo.CollectionTypes[0];

                // Get the generator method for the collection
                var generatorMethod = typeof(Utils).GetAMethod(nameof(CreateListWithRandomData))
                                .MakeGenericMethod(collectionType);

                // Get the IEnumerable<type of collection> type
                var ienumerableType = typeof(IEnumerable<>).MakeGenericType(collectionType);
                // Find the constructor that takes in the ienumerable type
                var constructor = typeof(T).GetConstructor(new[] { ienumerableType });

                // Return the object created by the constructor
                return (T)constructor.Invoke(new object[] { generatorMethod.Invoke(null, null) });
            }
        }

        /// <summary>
        /// Searches for a type inside the current <see cref="Assembly"/> and all of the referenced ones.
        /// </summary>
        /// <param name="typeName">Name of the type to find (can be name, full name or
        /// assembly qualified name).</param>
        /// <exception cref="ArgumentNullException"/>
        public static Type FindType(string typeName)
        {
            // Don't allow nulls
            typeName.ThrowIfNull(nameof(typeName));

            // Helper method for finding the type in an assembly
            Type Finder(Assembly ass) => ass?.GetTypes().FirstOrDefault(type =>
                typeName.In(type.Name, type.FullName, type.AssemblyQualifiedName)
            );

            // Check if the type is inside the current assembly
            var targetType = Finder(Assembly.GetExecutingAssembly());

            // Go through all of the referenced assemblies
            foreach (var assName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                // If the type was found, return it
                if (targetType != null)
                    return targetType;

                // Check if the type is inside the assembly
                targetType = Finder(Assembly.Load(assName));
            }

            // Type wasn't found, return null
            return null;
        }

        #endregion
    }
}
