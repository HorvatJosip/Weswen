namespace Weswen
{
    /// <summary>
    /// Information about the object state during the hierarchy traversion.
    /// </summary>
    public class TraversionInfo
    {
        /// <summary>
        /// Defines the level of the root object in the hierarchy.
        /// </summary>
        public const int TopLevel = 1;

        #region Properties

        /// <summary>
        /// Determines if this is the traversion just started (current object
        /// is the top level object in the hierarchy).
        /// </summary>
        public bool FirstCall => Level == TopLevel;

        /// <summary>
        /// Determines if the <see cref="CurrentPrield"/> has a value.
        /// </summary>
        public bool ValueWasFound => CurrentObj != null && Parent != null;

        /// <summary>
        /// Value of the current prield's parent.
        /// </summary>
        public object Parent { get; }

        /// <summary>
        /// Value of the current prield.
        /// </summary>
        public object CurrentObj { get; }

        /// <summary>
        /// Current prield in the hierarchy.
        /// </summary>
        public Prield CurrentPrield { get; }

        /// <summary>
        /// Current object's hierarchy level. <para>1 is the top level and it
        /// increases as traversion through the hierarchy gets deeper.</para>
        /// </summary>
        public int Level { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Only constructor.
        /// </summary>
        /// <param name="parent">Value of the current prield's parent.</param>
        /// <param name="currentObj">Value of the current prield.</param>
        /// <param name="currentPrield">Current prield in the hierarchy.</param>
        /// <param name="level">Current object's hierarchy level.</param>
        public TraversionInfo(object parent, object currentObj, Prield currentPrield, int level)
        {
            // Fill the properties with passed in values
            Parent = parent;
            CurrentObj = currentObj;
            CurrentPrield = currentPrield;
            Level = level;

            // Don't allow the level to be less than the top level
            if (Level < TopLevel)
                Level = TopLevel;
        }

        #endregion

        #region Method

        /// <summary>
        /// String representation of <see cref="TraversionInfo"/>.
        /// </summary>
        /// <returns>String representation of <see cref="TraversionInfo"/>.</returns>
        public override string ToString()
        {
            // Indent for every level except the first one
            var tabs = new string('\t', Level - 1);
            // Create an instance of the stringbuilder
            var builder = new System.Text.StringBuilder();
            // What will be written if a value is null
            var nullReplacement = "null";

            // If we are on the root object...
            if (FirstCall)
                // Print only its value because there is no parent or prield
                builder.AppendLine($"{CurrentObj}");

            else
            {
                // Write out the parent on the first line
                builder.AppendLine($"{tabs}{Parent ?? nullReplacement}");
                // Then add the prield type and name followed by its value after the '=' sign
                builder.AppendLine($"{tabs}  {CurrentPrield?.ToString() ?? nullReplacement} = {CurrentObj ?? nullReplacement}");
            }

            // Return the stringbuilder instance's string representation
            return builder.ToString();
        } 

        #endregion
    }
}
