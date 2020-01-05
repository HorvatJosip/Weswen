using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Weswen
{
    /// <summary>
    /// Pr(operty) or (F)ield - encapsulates the <see cref="MemberInfo"/> and
    /// exposes the getter and setter.
    /// </summary>
    public class Prield
    {
        #region Private Getters

        private PropertyInfo Prop => Member as PropertyInfo;
        private FieldInfo Field => Member as FieldInfo;

        #endregion

        #region Properties

        /// <summary>
        /// Defines whether or not this <see cref="Prield"/> is a property.
        /// </summary>
        public bool IsProperty { get; }

        /// <summary>
        /// Defines whether or not this <see cref="Prield"/> is an indexer.
        /// </summary>
        public bool IsIndexer => IsProperty && Prop.GetIndexParameters().Length != 0;

        /// <summary>
        /// Gets the declaring type of the <see cref="Member"/>.
        /// </summary>
        public Type DeclaringType => Member.DeclaringType;

        /// <summary>
        /// Gets the reflected type of the <see cref="Member"/>.
        /// </summary>
        public Type ReflectedType => Member.ReflectedType;

        /// <summary>
        /// Gets <see cref="Type"/> of the property or field.
        /// </summary>
        public Type Type => IsProperty
            ? Prop.PropertyType
            : Field.FieldType;

        /// <summary>
        /// The encapsulated <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>.
        /// </summary>
        public MemberInfo Member { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Sets up the field or property.
        /// </summary>
        /// <param name="member"><see cref="PropertyInfo"/> or <see cref="FieldInfo"/>.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public Prield(MemberInfo member)
        {
            // Don't allow nulls
            member.ThrowIfNull(nameof(member));

            // Set the member property
            Member = member;

            // Check the type of the member
            switch (member)
            {
                // If it is a property...
                case PropertyInfo prop:
                    // Declare that it is a property
                    IsProperty = true;
                    break;

                // If it is a field...
                case FieldInfo field:
                    // Declare that it isn't a property
                    IsProperty = false;
                    break;

                // If it is neither property nor field...
                default:
                    // Throw argument exception
                    throw new ArgumentException(
                        $"Passed in {nameof(MemberInfo)} must be either a {nameof(PropertyInfo)} or a {nameof(FieldInfo)}.",
                        nameof(member)
                    );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a value from the target's field or property.
        /// </summary>
        /// <param name="target">Object whose field or property value will be fetched.</param>
        /// <param name="indexerParams">If <see cref="Member"/> is an indexer, pass the index parameters here</param>
        /// <returns>Value from the field or property</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="TargetException"/>
        /// <exception cref="TargetParameterCountException"/>
        /// <exception cref="MethodAccessException"/>
        /// <exception cref="TargetInvocationException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="FieldAccessException"/>
        public object GetValue(object target, object[] indexerParams = null)
        {
            return IsProperty
                // If this prield is a property, get the value using the PropertyInfo#GetValue
                ? Prop.GetValue(target, indexerParams)
                // Otherwise, get the value using the FieldInfo#GetValue
                : Field.GetValue(target);
        }

        /// <summary>
        /// Sets a value of the target's field or property.
        /// </summary>
        /// <param name="target">Object whose field or property value will be set.</param>
        /// <param name="value">Value to set the target's property to.</param>
        /// <param name="indexerParams">If <see cref="Member"/> is an indexer, pass the index parameters here</param>
        /// <returns>Value from the field or property</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="TargetException"/>
        /// <exception cref="TargetParameterCountException"/>
        /// <exception cref="MethodAccessException"/>
        /// <exception cref="TargetInvocationException"/>
        /// <exception cref="FieldAccessException"/>
        public void SetValue(object target, object value, object[] indexerParams = null)
        {
            // If this prield is a property...
            if (IsProperty)
            {
                // Set the value using the PropertyInfo#SetValue if it exists
                if (Prop.GetSetMethod() != null)
                {
                    // If it isn't an indexer, don't use indexer parameters
                    if (!IsIndexer)
                        Prop.SetValue(target, value);
                    else // It is an indexer
                        Prop.SetValue(target, value, indexerParams);
                }
            }
            else
                // Otherwise, set the value using the FieldInfo#SetValue
                Field.SetValue(target, value);
        }

        /// <summary>
        /// String representation of the <see cref="Prield"/>.
        /// </summary>
        /// <returns>/>String representation of the <see cref="Prield"/>.</returns>
        public override string ToString() => Member.ToString();

        /// <summary>
        /// Generates a list of <see cref="Prield"/>s out of a collection of <see cref="MemberInfo"/>.
        /// </summary>
        /// <param name="members">Members of a type, for example a collection of <see cref="FieldInfo"/>.</param>
        /// <returns>List of <see cref="Prield"/>s.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static List<Prield> Generate(IEnumerable<MemberInfo> members)
        {
            // Don't allow nulls
            members.ThrowIfNull(nameof(members));

            // Convert each element of the collection into a prield and return the list
            return members.Select(member => new Prield(member)).ToList();
        } 

        #endregion
    }
}
