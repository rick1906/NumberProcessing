using System;
using System.Collections.Generic;
using System.Text;

namespace NumberProcessing
{
    /// <summary>
    /// Format of a string representaion of <c>AdvancedNumber</c>.
    /// </summary>
    public class AdvancedNumberFormat
    {
        public const char MultiplyExp = 'E';
        public const char MultiplyDot = '·';
        public const char MultiplyCross = '×';
        public const char PointDot = '.';
        public const char PointComma = ',';
        public const char PlusMinus = '±';

        /// <summary>
        /// Default format.
        /// </summary>
        public static AdvancedNumberFormat Default { get { return new AdvancedNumberFormat(null); } }

        /// <summary>
        /// Currently used global format.
        /// </summary>
        public static AdvancedNumberFormat Current { get; set; } = Default;

        /// <summary>
        /// Put error in brackets: 3.14(1) for 3.14 value and 0.01 error.
        /// </summary>
        public bool ShortFormat { get; set; } = true;

        /// <summary>
        /// Decimal point character.
        /// </summary>
        public char DecimalPoint { get; set; } = PointDot;

        /// <summary>
        /// Multiply sign for power-of-10 multiplier.
        /// </summary>
        public char MultiplySign { get; set; } = MultiplyCross;

        /// <summary>
        /// Minimum power-of-10 to put out of brackets.
        /// </summary>
        public int MinPowerOf10 { get; set; } = 5;

        /// <summary>
        /// Maximum digits in error representaion.
        /// </summary>
        public int MaxErrorDigits { get; set; } = 2;

        /// <summary>
        /// Maximum error significant value for short representaion.
        /// </summary>
        public int MaxErrorValue { get; set; } = 39;

        /// <summary>
        /// Maximum value digits.
        /// </summary>
        public int MaxValueDigits { get; set; } = 0;

        /// <summary>
        /// Spaces before and after multiply sign.
        /// </summary>
        public bool SpacesNearMultiply { get; set; } = true;

        /// <summary>
        /// Spaces before and after plusminus sign.
        /// </summary>
        public bool SpacesNearPlusMinus { get; set; } = true;

        /// <summary>
        /// Space before E in power-of-10 (example: 3.141 x 10E5).
        /// </summary>
        public bool SpaceBeforeExp { get; set; } = false;

        /// <summary>
        /// Space before bracket for short error representation.
        /// </summary>
        public bool SpaceBeforeBracket { get; set; } = true;

        /// <summary>
        /// Return new instance with same configuration.
        /// </summary>
        /// <returns></returns>
        public AdvancedNumberFormat Copy()
        {
            return new AdvancedNumberFormat(this);
        }

        public AdvancedNumberFormat(AdvancedNumberFormat parent)
        {
            if (parent != null) {
                ShortFormat = parent.ShortFormat;
                DecimalPoint = parent.DecimalPoint;
                MultiplySign = parent.MultiplySign;
                MinPowerOf10 = parent.MinPowerOf10;
                MaxErrorDigits = parent.MaxErrorDigits;
                MaxErrorValue = parent.MaxErrorValue;
                MaxValueDigits = parent.MaxValueDigits;
                SpacesNearMultiply = parent.SpacesNearMultiply;
                SpacesNearPlusMinus = parent.SpacesNearPlusMinus;
                SpaceBeforeExp = parent.SpaceBeforeExp;
                SpaceBeforeBracket = parent.SpaceBeforeBracket;
            }
        }

        public AdvancedNumberFormat(bool shortFormat, char decimalPoint, char multiplySign, int minPowerOf10, int maxErrorDigits, int maxValueDigits) : this(Current)
        {
            ShortFormat = shortFormat;
            DecimalPoint = decimalPoint;
            MultiplySign = multiplySign;
            MinPowerOf10 = minPowerOf10;
            MaxErrorDigits = maxErrorDigits;
            MaxValueDigits = maxValueDigits;
        }

        public AdvancedNumberFormat(bool shortFormat, char decimalPoint, char multiplySign, int minPowerOf10) : this(Current)
        {
            ShortFormat = shortFormat;
            DecimalPoint = decimalPoint;
            MultiplySign = multiplySign;
            MinPowerOf10 = minPowerOf10;
        }

        public AdvancedNumberFormat(bool shortFormat, char decimalPoint, char multiplySign) : this(Current)
        {
            ShortFormat = shortFormat;
            DecimalPoint = decimalPoint;
            MultiplySign = multiplySign;
        }

        public AdvancedNumberFormat(bool shortFormat) : this(Current)
        {
            ShortFormat = shortFormat;
        }

        public AdvancedNumberFormat() : this(Current)
        {
        }
    }
}
