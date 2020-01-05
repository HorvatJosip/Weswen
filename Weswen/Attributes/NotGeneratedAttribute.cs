using System;

namespace Weswen
{
    /// <summary>
    /// Describes that this property / field is not filled by
    /// the <see cref="Generator"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NotGeneratedAttribute : Attribute
    {
    }
}
