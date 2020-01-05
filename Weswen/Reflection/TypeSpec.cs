using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Weswen
{
    /// <summary>
    /// Specifies information about a type and holds
    /// history of values of its type.
    /// </summary>
    public class TypeSpec<T>
    {
        #region Properties

        /// <summary>
        /// Collection that holds values of type T.
        /// </summary>
        public List<T> HistoryOfValues { get; } = new List<T>();

        /// <summary>
        /// Gets the last value from the <see cref="HistoryOfValues"/>.
        /// </summary>
        public T LastValue => HistoryOfValues.Count != 0
            ? HistoryOfValues[HistoryOfValues.Count - 1]
            : default;

        /// <summary>
        /// Used to generate an instance of this type.<para>By default returns the result
        /// of <see cref="Utils.TryGetInstantiatedObject{T}"/>), or, if <typeparamref name="T"/>
        /// is assignable from <see cref="IEnumerable"/> with one generic argument, it creates
        /// a collection using <see cref="Utils.CreateListWithRandomData{T}()"/>.</para>
        /// </summary>
        public Expression<Func<T>> Generator { get; set; }

        /// <summary>
        /// Defines which constructor should be used for creating
        /// instance of this object (if left null, <see cref="Generator"/>
        /// will be used for generating an instance).
        /// </summary>
        public Type[] ConstructorParameterTypes { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TypeSpec()
        {
            try
            {
                // Try to create a collection (assumes that T is IEnumerable<something>)
                var collection = Utils.CreateCollection<T>();

                if (!Equals(collection, default(T)))
                    // If type T is some sort of collection that can be assigned from
                    // IEnumerable<>, generate it that way
                    Generator = () => Utils.CreateCollection<T>();
            }
            catch { }

            // T isn't a collection and can't be generated using the CreateCollection<T>
            if (Generator == null)
                Generator = () => Utils.TryGetInstantiatedObject<T>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of an object of type <typeparamref name="T"/>. If the
        /// <see cref="ConstructorParameterTypes"/> are null, the <see cref="Generator"/>
        /// will be used for generating the instance. If it isn't null, you can specify some
        /// or all of the parameters using the parameter <paramref name="constructorData"/>.
        /// </summary>
        /// <param name="constructorData">Data passed into the constructor. You can pass all of the
        /// elements here, or leave some to be generated for you using the <see cref="Weswen.Generator"/>.</param>
        /// <returns>Instance of type <typeparamref name="T"/>.</returns>
        public T GetInstance(params (int parameterIndex, object parameterValue)[] constructorData)
        {
            // Instance that will be created
            T instance;

            // If the constructor parameters aren't specified...
            if (ConstructorParameterTypes == null)
                // Generate it using the generator func
                instance = GenerateValue();

            // Or, if the constructor parameters are specified...
            else
            {
                try
                {
                    // Try to generate an instance using the constructor that is
                    // defined by constructor parameter types property
                    instance = Utils.InvokeConstructor<T>(ConstructorParameterTypes, constructorData);
                }
                catch (Exception ex)
                {
                    // Generating instance with the constructor failed,
                    // switch back to generator func
                    instance = GenerateValue();
                }
            } // else

            return instance;
        }

        /// <summary>
        /// Uses the <see cref="Generator"/> to generate the value or
        /// assigns a default value if the <see cref="Generator"/> is null.
        /// </summary>
        /// <returns>Instance of type <typeparamref name="T"/>.</returns>
        public T GenerateValue() =>
                Generator != null
                    ? Generator.Compile().Invoke()
                    : default; 

        #endregion
    }
}
