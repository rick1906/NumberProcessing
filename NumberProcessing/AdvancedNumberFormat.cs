using System;
using System.Collections.Generic;
using System.Text;

namespace NumberProcessing
{
    public class AdvancedNumberFormat
    {
        public const char MultiplyExp = 'E';
        public const char MultiplyDot = '·';
        public const char MultiplyCross = '×';
        public const char PointDot = '.';
        public const char PointComma = ',';
        public const char PlusMinus = '±';

        public static AdvancedNumberFormat Default { get { return new AdvancedNumberFormat(null); } }
        public static AdvancedNumberFormat Current { get; set; } = Default;

        public bool ShortFormat { get; set; } = true;
        public char DecimalPoint { get; set; } = PointDot;
        public char MultiplySign { get; set; } = MultiplyCross;
        public int MinPowerOf10 { get; set; } = 5;
        public int MaxErrorDigits { get; set; } = 2;
        public int MaxErrorValue { get; set; } = 39;
        public int MaxValueDigits { get; set; } = 0;
        public bool SpacesNearMultiply { get; set; } = true;
        public bool SpacesNearPlusMinus { get; set; } = true;
        public bool SpaceBeforeExp { get; set; } = false;
        public bool SpaceBeforeBracket { get; set; } = true;

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
