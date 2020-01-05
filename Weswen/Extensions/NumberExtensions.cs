namespace Weswen
{
    /// <summary>
    /// Extension methods for numbers (<see cref="double"/>, <see cref="int"/>, ...).
    /// </summary>
    public static class NumberExtensions
    {
        #region Between

        /// <summary>
        /// Determines if a number is in the interval.
        /// </summary>
        /// <param name="number">Number to test.</param>
        /// <param name="left">Left side of the interval.</param>
        /// <param name="right">Right side of the interval.</param>
        /// <param name="options">Interval inclusion option.</param>
        /// <exception cref="UnknownEnumValueException"/>
        public static bool Between(this decimal number, decimal left, decimal right,
            InclusionOptions options = InclusionOptions.BothInclusive)
        {
            switch (options)
            {
                case InclusionOptions.BothInclusive:
                    return number >= left && number <= right;
                case InclusionOptions.OnlyLeftInclusive:
                    return number >= left && number < right;
                case InclusionOptions.OnlyRightInclusive:
                    return number > left && number <= right;
                case InclusionOptions.BothExclusive:
                    return number > left && number < right;
                default:
                    throw new UnknownEnumValueException();
            }
        }

        /// <summary>
        /// Determines if a number is in the interval.
        /// </summary>
        /// <param name="number">Number to test.</param>
        /// <param name="left">Left side of the interval.</param>
        /// <param name="right">Right side of the interval.</param>
        /// <param name="options">Interval inclusion option.</param>
        /// <exception cref="UnknownEnumValueException"/>
        public static bool Between(this double number, decimal left, decimal right,
            InclusionOptions options = InclusionOptions.BothInclusive)
            => Between((decimal)number, left, right, options);

        /// <summary>
        /// Determines if a number is in the interval.
        /// </summary>
        /// <param name="number">Number to test.</param>
        /// <param name="left">Left side of the interval.</param>
        /// <param name="right">Right side of the interval.</param>
        /// <param name="options">Interval inclusion option.</param>
        /// <exception cref="UnknownEnumValueException"/>
        public static bool Between(this float number, decimal left, decimal right,
            InclusionOptions options = InclusionOptions.BothInclusive)
            => Between((decimal)number, left, right, options);

        /// <summary>
        /// Determines if a number is in the interval.
        /// </summary>
        /// <param name="number">Number to test.</param>
        /// <param name="left">Left side of the interval.</param>
        /// <param name="right">Right side of the interval.</param>
        /// <param name="options">Interval inclusion option.</param>
        /// <exception cref="UnknownEnumValueException"/>
        public static bool Between(this long number, decimal left, decimal right,
            InclusionOptions options = InclusionOptions.BothInclusive)
            => Between((decimal)number, left, right, options);

        /// <summary>
        /// Determines if a number is in the interval.
        /// </summary>
        /// <param name="number">Number to test.</param>
        /// <param name="left">Left side of the interval.</param>
        /// <param name="right">Right side of the interval.</param>
        /// <param name="options">Interval inclusion option.</param>
        /// <exception cref="UnknownEnumValueException"/>
        public static bool Between(this int number, decimal left, decimal right,
            InclusionOptions options = InclusionOptions.BothInclusive)
            => Between((decimal)number, left, right, options);

        #endregion

        #region Format

        /// <summary>
        /// Formats the number by separating thousands with the culture specific separator.
        /// </summary>
        /// <param name="number">Number to format.</param>
        /// <param name="useDecimalPlaces">Should the decimal places be used?</param>
        /// <returns></returns>
        public static string Format(this decimal number, bool useDecimalPlaces = false)
            => string.Format($"{{0:n{(useDecimalPlaces ? "" : "0")}}}", number);

        /// <summary>
        /// Formats the number by separating thousands with the culture specific separator.
        /// </summary>
        /// <param name="number">Number to format.</param>
        /// <param name="useDecimalPlaces">Should the decimal places be used?</param>
        /// <returns></returns>
        public static string Format(this double number, bool useDecimalPlaces = false)
            => Format((decimal)number, useDecimalPlaces);

        /// <summary>
        /// Formats the number by separating thousands with the culture specific separator.
        /// </summary>
        /// <param name="number">Number to format.</param>
        /// <param name="useDecimalPlaces">Should the decimal places be used?</param>
        /// <returns></returns>
        public static string Format(this float number, bool useDecimalPlaces = false)
            => Format((decimal)number, useDecimalPlaces);

        /// <summary>
        /// Formats the number by separating thousands with the culture specific separator.
        /// </summary>
        /// <param name="number">Number to format.</param>
        /// <param name="useDecimalPlaces">Should the decimal places be used?</param>
        /// <returns></returns>
        public static string Format(this long number, bool useDecimalPlaces = false)
            => Format((decimal)number, useDecimalPlaces);

        /// <summary>
        /// Formats the number by separating thousands with the culture specific separator.
        /// </summary>
        /// <param name="number">Number to format.</param>
        /// <param name="useDecimalPlaces">Should the decimal places be used?</param>
        /// <returns></returns>
        public static string Format(this int number, bool useDecimalPlaces = false)
            => Format((decimal)number, useDecimalPlaces);

        #endregion
    }
}
