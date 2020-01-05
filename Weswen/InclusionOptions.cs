namespace Weswen
{
    /// <summary>
    /// Specifies interval inclusion.
    /// </summary>
    public enum InclusionOptions
    {
        /// <summary>
        /// Both left and right are included.
        /// </summary>
        BothInclusive,

        /// <summary>
        /// Only left is included, right is excluded.
        /// </summary>
        OnlyLeftInclusive,

        /// <summary>
        /// Only right is included, left is excluded.
        /// </summary>
        OnlyRightInclusive,

        /// <summary>
        /// Both left and right are excluded.
        /// </summary>
        BothExclusive
    }
}
