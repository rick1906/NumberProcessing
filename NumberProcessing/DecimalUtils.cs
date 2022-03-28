using System;
using System.Collections.Generic;
using System.Text;

namespace NumberProcessing
{
    internal static class DecimalUtils
    {
        public static readonly int DecimalExpLimit = GetNormalPow10(decimal.MaxValue);
        public static readonly int DoubleExpLimit = GetNormalPow10(double.MaxValue);

        public static bool CanBeDecimal(int exp)
        {
            return exp < DecimalExpLimit && exp > -DecimalExpLimit;
        }

        public static bool CanBeDouble(int exp)
        {
            return exp < DoubleExpLimit && exp > -DoubleExpLimit;
        }

        public static int GetNormalPow10(double x)
        {
            double z = Math.Abs(x);
            return z > 0 ? (int)Math.Floor(Math.Log10(z)) : 0;
        }

        public static int GetNormalPow10(decimal x)
        {
            return GetNormalPow10((double)x);
        }

        public static decimal Pow10(int k)
        {
            if (k == 0) { return 1; }
            decimal r = 1m;
            decimal z = k > 0 ? 10m : 0.1m;
            k = Math.Abs(k);
            for (int i = 0; i < k; ++i) { r *= z; }
            return r;
        }

        public static double Pow10Double(int k)
        {
            if (Math.Abs(k) < DecimalExpLimit) {
                return (double)Pow10(k);
            } else {
                return Math.Pow(10, k);
            }
        }

        public static int GetMaxPow10(decimal x)
        {
            return GetNormalPow10(x);
        }

        public static int GetMinPow10(decimal x)
        {
            if (x == 0) return 0;
            x = Math.Abs(x);
            int k = 0;
            if (Math.Floor(x) != x) {
                do { x *= 10; k--; } while (Math.Floor(x) != x);
            } else {
                x /= 10;
                while (Math.Floor(x) == x) { x /= 10; k++; }
            }
            return k;
        }

        public static decimal ProcessErrorDigits(decimal error, int maxErrorDigits, int maxErrorValue, int vp1, int vp2, ref int ep1, ref int ep2)
        {
            if (maxErrorDigits > 0) {
                ep2 = Math.Min(ep2, vp2);
                ep2 = Math.Max(ep1 - maxErrorDigits + 1, ep2);
            }

            ep2 = Math.Min(vp1, ep2);
            long e = decimal.ToInt64(Math.Round(error * Pow10(-ep2)));
            if (maxErrorValue > 0 && maxErrorDigits > 0) {
                int mp = GetMaxPow10(maxErrorValue) + 1;
                long lim = maxErrorDigits > mp ? decimal.ToInt64(maxErrorValue * Pow10(maxErrorDigits - mp)) : maxErrorValue;
                while (e > lim && ep2 < vp1) {
                    e = decimal.ToInt64(Math.Round(0.1m * e));
                    ep2++;
                }
            }

            return e * Pow10(ep2);
        }

        public static decimal Round(decimal value, int decimals)
        {
            if (decimals >= 0) { return Math.Round(value, decimals); }
            value *= Pow10(decimals);
            return Math.Round(value) * Pow10(-decimals);
        }
    }
}
