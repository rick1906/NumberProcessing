using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NumberProcessing
{
    public struct AdvancedNumber : IComparable, IComparable<AdvancedNumber>, IConvertible, IEquatable<AdvancedNumber>
    {
        public const int MaxPowerOf10 = 0x00FFFFFF;

        public static readonly AdvancedNumber PositiveInfinity = new AdvancedNumber(1, 0, MaxPowerOf10);
        public static readonly AdvancedNumber NegativeInfinity = new AdvancedNumber(-1, 0, MaxPowerOf10);

        public decimal Value {
            get { return _value; }
        }

        public decimal Error {
            get { return _error; }
        }

        public int PowerOf10 {
            get { return _exp; }
        }

        public double ResultValue {
            get { return (double)_value * DecimalUtils.Pow10Double(_exp); }
        }

        public double ResultError {
            get { return (double)_error * DecimalUtils.Pow10Double(_exp); }
        }

        public bool ResultIsDecimal {
            get { return DecimalUtils.CanBeDecimal(GetNormalPowerOf10()); }
        }

        public bool ResultIsDouble {
            get { return DecimalUtils.CanBeDouble(GetNormalPowerOf10()); }
        }

        public decimal DecimalResultValue {
            get { return _value * DecimalUtils.Pow10(_exp); }
        }

        public decimal DecimalResultError {
            get { return _error * DecimalUtils.Pow10(_exp); }
        }

        public double RelativeError {
            get { return _value != 0 ? (double)_error / (double)Math.Abs(_value) : 0; }
        }

        private decimal _error;
        private decimal _value;
        private int _exp;

        public AdvancedNumber(decimal value, decimal error, int powerOf10)
        {
            _value = value;
            _error = error;
            _exp = powerOf10;
            if (_error < 0) {
                throw new ArgumentOutOfRangeException(nameof(error));
            }
        }

        public AdvancedNumber(decimal value, decimal error) : this(value, error, 0)
        {
        }

        public AdvancedNumber(decimal value) : this(value, 0, 0)
        {
        }

        public AdvancedNumber(int value, decimal error, int powerOf10) : this((decimal)value, error, powerOf10)
        {
        }

        public AdvancedNumber(int value, decimal error) : this(value, error, 0)
        {
        }

        public AdvancedNumber(int value) : this(value, 0, 0)
        {
        }

        public AdvancedNumber(double value, double error, int powerOf10)
        {
            if (double.IsInfinity(value)) {
                _value = 1;
                _error = 0;
                _exp = MaxPowerOf10;
            } else {
                int exp = DecimalUtils.GetNormalPow10(value);
                double z = DecimalUtils.Pow10Double(-exp);
                _value = (decimal)(value * z);
                _error = (decimal)(error * z);
                _exp = exp + powerOf10;
            }
        }

        public AdvancedNumber(double value, double error) : this(value, error, 0)
        {
        }

        public AdvancedNumber(double value) : this(value, 0, 0)
        {
        }

        public AdvancedNumber(string s)
        {
            this = Parse(s);
        }

        public int GetNormalPowerOf10()
        {
            return DecimalUtils.GetNormalPow10(_value) + _exp;
        }

        public string ToString(AdvancedNumberFormat format)
        {
            if (_exp >= MaxPowerOf10) {
                if (_value > 0) {
                    return double.PositiveInfinity.ToString(CultureInfo.InvariantCulture);
                } else if (_value < 0) {
                    return double.NegativeInfinity.ToString(CultureInfo.InvariantCulture);
                }
            }

            decimal value = _value;
            decimal cutValue = _value;
            int vp1 = DecimalUtils.GetMaxPow10(value);
            if (format.MaxValueDigits > 0) {
                int cut = -vp1 + format.MaxValueDigits - 1;
                cutValue = DecimalUtils.Round(value, cut);
            }

            int vp2 = DecimalUtils.GetMinPow10(cutValue);
            int minp = vp1;

            decimal error = _error;
            if (error > 0) {
                int ep1 = DecimalUtils.GetMaxPow10(error);
                int ep2 = DecimalUtils.GetMinPow10(error);
                error = DecimalUtils.ProcessErrorDigits(error, format.MaxErrorDigits, format.MaxErrorValue, vp1, vp2, ref ep1, ref ep2);
                value = DecimalUtils.Round(value, -ep2);
                minp = Math.Max(minp, ep1);
                error *= DecimalUtils.Pow10(-minp);
            }

            value *= DecimalUtils.Pow10(-minp);
            int exp = _exp + minp;

            if (format.MinPowerOf10 == 0 || format.MinPowerOf10 > 0 && exp < format.MinPowerOf10 && exp > -format.MinPowerOf10) {
                value *= DecimalUtils.Pow10(exp);
                error *= DecimalUtils.Pow10(exp);
                exp = 0;
            }

            string sn = "";
            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberDecimalSeparator = format.DecimalPoint.ToString();
            if (format.ShortFormat) {
                int mpv = DecimalUtils.GetMinPow10(value);
                int le = error > 0 ? Math.Min(DecimalUtils.GetMinPow10(error), mpv) : mpv;
                sn += value.ToString(le < 0 ? ("F" + (-le).ToString()) : "F0", nfi);
                if (error > 0) {
                    decimal er2 = le >= 0 ? error : error * DecimalUtils.Pow10(-le);
                    int ie = decimal.ToInt32(Math.Round(er2));
                    if (format.SpaceBeforeBracket) sn += " ";
                    sn += "(" + ie.ToString() + ")";
                }
                if (exp != 0) {
                    string sep;
                    if (char.ToUpper(format.MultiplySign) == 'E') {
                        sep = "E";
                        if (format.SpaceBeforeExp) sep = " " + sep;
                    } else {
                        sep = format.MultiplySign.ToString();
                        if (format.SpacesNearMultiply) sep = " " + sep + " ";
                        sep += "10E";
                    }
                    sn += sep + exp.ToString();
                }
            } else {
                int lv = DecimalUtils.GetMinPow10(value);
                sn += value.ToString(lv < 0 ? ("F" + (-lv).ToString()) : "F0", nfi);
                if (error > 0) {
                    char pm = AdvancedNumberFormat.PlusMinus;
                    sn += format.SpacesNearPlusMinus ? (" " + pm + " ") : pm.ToString();
                    int le = Math.Min(lv, DecimalUtils.GetMinPow10(error));
                    sn += error.ToString(le < 0 ? ("F" + (-le).ToString()) : "F0", nfi);
                }
                if (exp != 0) {
                    sn = "(" + sn + ")";
                    string sep;
                    if (char.ToUpper(format.MultiplySign) == 'E') {
                        sep = "E";
                        if (format.SpaceBeforeExp) sep = " " + sep;
                    } else {
                        sep = format.MultiplySign.ToString();
                        if (format.SpacesNearMultiply) sep = " " + sep + " ";
                        sep += "10E";
                    }
                    sn += sep + exp.ToString();
                }
            }

            return sn;
        }

        public string ToString(bool shortFormat, char decimalPoint, char multiplySign, int minPowerOf10, int maxErrorDigits, int maxValueDigits)
        {
            return ToString(new AdvancedNumberFormat(shortFormat, decimalPoint, multiplySign, minPowerOf10, maxErrorDigits, maxValueDigits));
        }

        public string ToString(bool shortFormat, char decimalPoint, char multiplySign, int minPowerOf10)
        {
            return ToString(new AdvancedNumberFormat(shortFormat, decimalPoint, multiplySign, minPowerOf10));
        }

        public string ToString(bool shortFormat, char decimalPoint, char multiplySign)
        {
            return ToString(new AdvancedNumberFormat(shortFormat, decimalPoint, multiplySign));
        }

        public string ToString(bool shortFormat)
        {
            return ToString(new AdvancedNumberFormat(shortFormat));
        }

        public override string ToString()
        {
            return ToString(new AdvancedNumberFormat());
        }

        public int CompareTo(AdvancedNumber n)
        {
            int dexp = n._exp - _exp;
            if (dexp + DecimalUtils.GetNormalPow10(n._value) < DecimalUtils.DecimalExpLimit) {
                decimal v = n._value * DecimalUtils.Pow10(dexp);
                return _value.CompareTo(v);
            } else {
                double v = n.ResultValue;
                return ResultValue.CompareTo(v);
            }
        }

        public int CompareTo(object obj)
        {
            if (obj is AdvancedNumber) {
                return CompareTo((AdvancedNumber)obj);
            } else if (obj is int && ResultIsDecimal) {
                return DecimalResultValue.CompareTo((int)obj);
            } else if (obj is decimal && ResultIsDecimal) {
                return DecimalResultValue.CompareTo((decimal)obj);
            } else {
                try {
                    double num = Convert.ToDouble(obj);
                    return ResultValue.CompareTo(num);
                } catch (Exception) {
                    return 0;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is AdvancedNumber) {
                AdvancedNumber num = (AdvancedNumber)obj;
                return Equals(num);
            }
            return false;
        }

        public bool Equals(AdvancedNumber other)
        {
            if (ResultIsDecimal && other.ResultIsDecimal) {
                return DecimalResultValue == other.DecimalResultValue && DecimalResultError == other.DecimalResultError;
            } else {
                return ResultValue == other.ResultValue && ResultError == other.ResultError;
            }
        }

        public override int GetHashCode()
        {
            return ResultIsDecimal ? DecimalResultValue.GetHashCode() : ResultValue.GetHashCode();
        }

        public static bool operator ==(AdvancedNumber n1, AdvancedNumber n2)
        {
            return n1.Equals(n2);
        }

        public static bool operator !=(AdvancedNumber n1, AdvancedNumber n2)
        {
            return !n1.Equals(n2);
        }

        public static explicit operator AdvancedNumber(double value)
        {
            if (double.IsNaN(value)) {
                throw new ArithmeticException();
            }
            return new AdvancedNumber(value);
        }

        public static implicit operator AdvancedNumber(decimal value)
        {
            return new AdvancedNumber(value);
        }

        public static implicit operator AdvancedNumber(int value)
        {
            return new AdvancedNumber(value);
        }

        public static AdvancedNumber operator +(AdvancedNumber num)
        {
            return num;
        }

        public static AdvancedNumber operator -(AdvancedNumber num)
        {
            return new AdvancedNumber(-num._value, num._error, num._exp);
        }

        private static double CalculateError(decimal error1, decimal error2)
        {
            return CalculateError((double)error1, (double)error2);
        }

        private static double CalculateError(double error1, double error2)
        {
            if (error1 <= 0) {
                return error2;
            } else if (error2 <= 0) {
                return error1;
            } else {
                return Math.Sqrt(error1 * error1 + error2 * error2);
            }
        }

        public static AdvancedNumber operator +(AdvancedNumber n1, AdvancedNumber n2)
        {
            int p = GetCommonPowerOf10(n1, n2);
            AdvancedNumber nn1 = n1.NormalizeTo(p);
            AdvancedNumber nn2 = n2.NormalizeTo(p);
            return new AdvancedNumber(n1._value + n2._value, (decimal)CalculateError(nn1._error, nn2._error), p);
        }

        public static AdvancedNumber operator -(AdvancedNumber n1, AdvancedNumber n2)
        {
            int p = GetCommonPowerOf10(n1, n2);
            AdvancedNumber nn1 = n1.NormalizeTo(p);
            AdvancedNumber nn2 = n2.NormalizeTo(p);
            return new AdvancedNumber(n1._value - n2._value, (decimal)CalculateError(nn1._error, nn2._error), p);
        }

        public static AdvancedNumber operator *(AdvancedNumber n1, AdvancedNumber n2)
        {
            double v = (double)n1._value * (double)n2._value;
            double re = CalculateError(n1.RelativeError, n2.RelativeError);
            double e = Math.Abs(v) * re;
            return new AdvancedNumber(v, e, n1._exp + n2._exp);
        }

        public static AdvancedNumber operator *(AdvancedNumber n1, double n2)
        {
            double v = (double)n1._value * n2;
            double e = Math.Abs(v) * n1.RelativeError;
            return new AdvancedNumber(v, e, n1._exp);
        }

        public static AdvancedNumber operator /(AdvancedNumber n1, AdvancedNumber n2)
        {
            double v = (double)n1._value / (double)n2._value;
            double re = CalculateError(n1.RelativeError, n2.RelativeError);
            double e = Math.Abs(v) * re;
            return new AdvancedNumber(v, e, n1._exp - n2._exp);
        }

        public static AdvancedNumber operator /(AdvancedNumber n1, double n2)
        {
            double v = (double)n1._value / n2;
            double e = Math.Abs(v) * n1.RelativeError;
            return new AdvancedNumber(v, e, n1._exp);
        }

        public static bool operator >(AdvancedNumber n1, AdvancedNumber n2)
        {
            return n1.CompareTo(n2) > 0;
        }

        public static bool operator <(AdvancedNumber n1, AdvancedNumber n2)
        {
            return n1.CompareTo(n2) < 0;
        }

        public static bool operator >=(AdvancedNumber n1, AdvancedNumber n2)
        {
            return n1.CompareTo(n2) >= 0;
        }

        public static bool operator <=(AdvancedNumber n1, AdvancedNumber n2)
        {
            return n1.CompareTo(n2) <= 0;
        }

        public static bool IsPositiveInfinity(AdvancedNumber num)
        {
            return num._value > 0 && num._exp >= MaxPowerOf10;
        }

        public static bool IsNegativeInfinity(AdvancedNumber num)
        {
            return num._value < 0 && num._exp >= MaxPowerOf10;
        }

        public bool IsPositiveInfinity()
        {
            return IsPositiveInfinity(this);
        }

        public bool IsNegativeInfinity()
        {
            return IsNegativeInfinity(this);
        }

        public AdvancedNumber NormalizeError(int digits)
        {
            if (digits <= 0) { throw new ArgumentOutOfRangeException(nameof(digits)); }
            int ep1 = DecimalUtils.GetMaxPow10(_error);
            int r = ep1 + digits - 1;
            decimal e = DecimalUtils.Round(_error, r);
            decimal v = DecimalUtils.Round(_value, r);
            return new AdvancedNumber(v, e, _exp);
        }

        public AdvancedNumber NormalizeError()
        {
            int ep1 = DecimalUtils.GetMaxPow10(_error);
            int ep2 = DecimalUtils.GetMinPow10(_error);
            int digits = ep1 - ep2;
            return (digits > 0) ? NormalizeError(digits) : this;
        }

        public AdvancedNumber Normalize()
        {
            int k = DecimalUtils.GetNormalPow10(_value);
            decimal m = DecimalUtils.Pow10(-k);
            return new AdvancedNumber(_value * m, _error * m, _exp + k);
        }

        public AdvancedNumber NormalizeTo(int powerOf10)
        {
            int d = _exp - powerOf10;
            decimal m = DecimalUtils.Pow10(d);
            return new AdvancedNumber(_value * m, _error * m, d);
        }

        private static int GetCommonPowerOf10(AdvancedNumber n1, AdvancedNumber n2)
        {
            return Math.Max(n1._exp, n2._exp);
        }

        public static AdvancedNumber Parse(string s, bool strict)
        {
            return AdvancedNumberParser.Parse(s, strict, out Match m);
        }

        public static AdvancedNumber Parse(string s)
        {
            return Parse(s, false);
        }

        public static bool TryParse(string s, bool strict, out AdvancedNumber number)
        {
            return AdvancedNumberParser.TryParse(s, strict, out number, out Match ma);
        }

        public static bool TryParse(string s, out AdvancedNumber number)
        {
            return TryParse(s, false, out number);
        }

        private IConvertible AsIConvertible()
        {
            return ResultIsDecimal ? DecimalResultValue : (IConvertible)ResultValue;
        }

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Double;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return AsIConvertible().ToBoolean(provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return AsIConvertible().ToByte(provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return AsIConvertible().ToChar(provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return AsIConvertible().ToDateTime(provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return ResultIsDecimal ? DecimalResultValue : ((IConvertible)ResultValue).ToDecimal(provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return ResultValue;
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return AsIConvertible().ToInt16(provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return AsIConvertible().ToInt32(provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return AsIConvertible().ToInt64(provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return AsIConvertible().ToSByte(provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return AsIConvertible().ToSingle(provider);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            object format = provider.GetFormat(GetType());
            if (format is AdvancedNumberFormat) {
                return ToString((AdvancedNumberFormat)format);
            }
            return ToString();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return AsIConvertible().ToType(conversionType, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return AsIConvertible().ToUInt16(provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return AsIConvertible().ToUInt32(provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return AsIConvertible().ToUInt64(provider);
        }
    }
}
