using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NumberProcessing.Units
{
    public struct NumberContainer
    {
        private AdvancedNumber _number;
        private ModifierType _modifier;
        private string _unit;

        public AdvancedNumber Number { get { return _number; } }
        public ModifierType Modifier { get { return _modifier; } }
        public string Unit { get { return string.IsNullOrEmpty(_unit) ? "" : _unit; } }

        public decimal Value { get { return _number.Value; } }
        public decimal Error { get { return _number.Error; } }
        public int PowerOf10 { get { return _number.PowerOf10; } }

        public double ResultValue { get { return _number.ResultValue; } }
        public double ResultError { get { return _number.ResultError; } }
        public bool ResultIsDecimal { get { return _number.ResultIsDecimal; } }

        public decimal DecimalResultValue { get { return _number.DecimalResultValue; } }
        public decimal DecimalResultError { get { return _number.DecimalResultError; } }

        public double RelativeError { get { return _number.RelativeError; } }

        private NumberContainer(AdvancedNumber number, string unit, ModifierType modifier)
        {
            _number = number;
            _unit = unit;
            _modifier = modifier;
        }

        public NumberContainer(AdvancedNumber number, string unit)
        {
            _number = number;
            _unit = unit;
            _modifier = ModifierType.Normal;
        }

        public NumberContainer(AdvancedNumber number)
        {
            _number = number;
            _unit = null;
            _modifier = ModifierType.Normal;
        }

        public bool HasUnit()
        {
            return !string.IsNullOrEmpty(_unit);
        }

        public bool IsValidUnit(params string[] baseUnitVariants)
        {
            if (string.IsNullOrEmpty(_unit)) {
                return baseUnitVariants == null || baseUnitVariants.Length == 0;
            } else {
                return MetricUnits.ValidateUnit(_unit, baseUnitVariants);
            }
        }

        public bool IsValidTimeUnit()
        {
            if (string.IsNullOrEmpty(_unit)) {
                return false;
            } else {
                return MetricUnits.ValidateTimeUnit(_unit);
            }
        }

        public bool IsCompatibleWith(NumberContainer nc, params string[] baseUnitVariants)
        {
            if (baseUnitVariants == null || baseUnitVariants.Length == 0) {
                if (!HasUnit() && !nc.HasUnit()) {
                    return true;
                }
                if (IsValidTimeUnit() && nc.IsValidTimeUnit()) {
                    return true;
                }
            }
            return MetricUnits.UnitsAreCompatible(_unit, nc._unit);
        }

        public IComparable ToComparable(params string[] baseUnitVariants)
        {
            AdvancedNumber value;
            if (TryGetNormalizedValue(out value, baseUnitVariants)) {
                return value;
            } else {
                return null;
            }
        }

        public IComparable ToComparableTime()
        {
            AdvancedNumber value;
            if (TryGetNormalizedTimeValue(out value)) {
                return value;
            } else {
                return null;
            }
        }

        public double GetNormalizedResultTimeValue()
        {
            double value;
            if (TryGetNormalizedResultTimeValue(out value)) {
                return value;
            } else {
                return double.NaN;
            }
        }

        public double GetNormalizedResultValue(params string[] baseUnitVariants)
        {
            double value;
            if (TryGetNormalizedResultValue(out value, baseUnitVariants)) {
                return value;
            } else {
                return double.NaN;
            }
        }

        public bool TryGetNormalizedResultTimeValue(out double value)
        {
            AdvancedNumber n;
            if (TryGetNormalizedTimeValue(out n)) {
                value = n.ResultValue;
                return true;
            } else {
                value = double.NaN;
                return false;
            }
        }

        public bool TryGetNormalizedResultValue(out double value, params string[] baseUnitVariants)
        {
            AdvancedNumber n;
            if (TryGetNormalizedValue(out n, baseUnitVariants)) {
                value = n.ResultValue;
                return true;
            } else {
                value = double.NaN;
                return false;
            }
        }

        public AdvancedNumber GetNormalizedTimeValue()
        {
            AdvancedNumber value;
            if (TryGetNormalizedTimeValue(out value)) {
                return value;
            } else {
                throw new InvalidOperationException();
            }
        }

        public AdvancedNumber GetNormalizedValue(params string[] baseUnitVariants)
        {
            AdvancedNumber value;
            if (TryGetNormalizedValue(out value, baseUnitVariants)) {
                return value;
            } else {
                throw new InvalidOperationException();
            }
        }

        public bool TryGetNormalizedTimeValue(out AdvancedNumber value)
        {
            if (MetricUnits.ValidateTimeUnit(_unit)) {
                decimal k = MetricUnits.GetTimeMultiplier(_unit);
                value = new AdvancedNumber(_number.Value * k, _number.Error * k, _number.PowerOf10);
                return true;
            } else {
                value = 0;
                return false;
            }
        }

        public bool TryGetNormalizedValue(out AdvancedNumber value, params string[] baseUnitVariants)
        {
            if (baseUnitVariants == null || baseUnitVariants.Length == 0) {
                if (!HasUnit()) {
                    value = _number;
                    return true;
                }
                if (TryGetNormalizedTimeValue(out value)) {
                    return true;
                }
            }

            int exp;
            if (MetricUnits.ValidateUnit(_unit, out exp, baseUnitVariants)) {
                value = new AdvancedNumber(_number.Value, _number.Error, _number.PowerOf10 + exp);
                return true;
            } else {
                value = _number;
                return false;
            }
        }

        public static bool TryParse(string s, out NumberContainer value)
        {
            try {
                return TryParseInternal(s, out value);
            } catch (FormatException) {
                value = default(NumberContainer);
                return false;
            } catch (OverflowException) {
                value = default(NumberContainer);
                return false;
            }
        }

        public static NumberContainer Parse(string s)
        {
            NumberContainer value;
            if (TryParseInternal(s, out value)) {
                return value;
            } else {
                throw new FormatException();
            }
        }

        private static bool TryParseInternal(string s, out NumberContainer result)
        {
            string st = s.TrimEnd();
            if (st.Length == 0) {
                result = default(NumberContainer);
                return false;
            }

            int spaceIndex = -1;
            int digitIndex = -1;
            for (int i = st.Length - 1; i >= 0; --i) {
                if (char.IsWhiteSpace(st[i])) {
                    spaceIndex = i;
                    break;
                }
                if (char.IsDigit(st[i]) || st[i] == ')') {
                    digitIndex = i;
                    break;
                }
            }

            string unit = "";
            string number = st.Trim();
            if (spaceIndex >= 0) {
                unit = st.Substring(spaceIndex + 1);
                number = st.Substring(0, spaceIndex).TrimEnd();
            } else if (digitIndex >= 0) {
                unit = st.Substring(digitIndex + 1);
                number = st.Substring(0, digitIndex + 1);
            }
            if (unit.ToCharArray().Any(c => char.IsDigit(c) || c == '(' || c == ')') || number.Length == 0) {
                unit = "";
                number = st.Trim();
            }
            if (number.Length == 0) {
                result = default(NumberContainer);
                return false;
            }

            ModifierType status = ModifierType.Normal;
            if (number[0] == LessThan) {
                status = ModifierType.LessThan;
            } else if (number[0] == GreaterThan) {
                status = ModifierType.GreaterThan;
            } else if (number[0] == LessOrEqualThan) {
                status = ModifierType.LessOrEqualThan;
            } else if (number[0] == GreaterOrEqualThan) {
                status = ModifierType.GreaterOrEqualThan;
            } else if (number[0] == ApproximateEquals) {
                status = ModifierType.ApproximateEquals;
            } else if (number[0] == Approximate || number[0] == '∼') {
                status = ModifierType.Approximate;
            }

            if (status != ModifierType.Normal) {
                number = number.Substring(1).TrimStart();
            } else {
                string minusSigns = SpecialChars.MinusSigns;
                if (number.ToCharArray().Any(c => minusSigns.IndexOf(c) >= 0)) {
                    Regex split = new Regex("\\d+.*([" + SpecialChars.EscapeRegex(minusSigns) + "]).*\\d+");
                    Match match = split.Match(number);
                    if (match.Success) {
                        int index = match.Groups[1].Index;
                        string xnum = number.Substring(0, index) + AdvancedNumberFormat.PlusMinus + number.Substring(index + 1);
                        AdvancedNumber exn;
                        if (AdvancedNumber.TryParse(xnum, out exn)) {
                            decimal avg = (exn.Value + exn.Error) / 2;
                            decimal err = Math.Abs(exn.Value - exn.Error) / 2;
                            result = new NumberContainer(new AdvancedNumber(avg, err, exn.PowerOf10), unit, ModifierType.Range);
                            return true;
                        }
                    }
                }
                if (number[0] == '(' && number[number.Length - 1] == ')') {
                    string ns = number.Substring(1, number.Length - 2).Trim();
                    AdvancedNumber exn;
                    if (AdvancedNumber.TryParse(ns, out exn)) {
                        result = new NumberContainer(exn, unit, ModifierType.ApproximateBrackets);
                        return true;
                    }
                }
            }

            result = new NumberContainer(new AdvancedNumber(number), unit, status);
            return true;
        }

        private const char Approximate = '~';
        private const char ApproximateEquals = '≈';
        private const char LessThan = '<';
        private const char GreaterThan = '>';
        private const char LessOrEqualThan = '≤';
        private const char GreaterOrEqualThan = '≥';

        public enum ModifierType
        {
            Normal = 0,
            Approximate = 1,
            ApproximateEquals = 2,
            ApproximateBrackets = 3,
            LessThan = 11,
            GreaterThan = 12,
            LessOrEqualThan = 13,
            GreaterOrEqualThan = 14,
            Range = 100,
        }
    }
}
