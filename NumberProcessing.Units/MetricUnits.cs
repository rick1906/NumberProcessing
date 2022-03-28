using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NumberProcessing.Units
{
    public static class MetricUnits
    {
        private static Dictionary<int, string[]> Prefixes {
            get {
                if (prefixes == null) {
                    prefixes = GeneratePrefixes();
                }
                return prefixes;
            }
        }

        private static Dictionary<int, string[][]> TimeUnits {
            get {
                if (timeUnits == null) {
                    timeUnits = GenerateTimeUnits();
                }
                return timeUnits;
            }
        }

        private static List<string[]> BaseUnits {
            get {
                if (baseUnits == null) {
                    baseUnits = GenerateBaseUnits();
                }
                return baseUnits;
            }
        }

        private static Dictionary<int, string[]> prefixes = null;
        private static Dictionary<int, string[][]> timeUnits = null;
        private static List<string[]> baseUnits = null;

        private static Dictionary<int, string[]> GeneratePrefixes()
        {
            Dictionary<int, string[]> collection = new Dictionary<int, string[]>();

            collection.Add(3, new string[] { "к", "k" });
            collection.Add(6, new string[] { "М", "M" });
            collection.Add(9, new string[] { "Г", "G" });
            collection.Add(12, new string[] { "Т", "T" });
            collection.Add(15, new string[] { "П", "P" });

            collection.Add(-3, new string[] { "м", "m" });
            collection.Add(-6, new string[] { "мк", "µ" });
            collection.Add(-9, new string[] { "н", "n" });
            collection.Add(-12, new string[] { "п", "p" });
            collection.Add(-15, new string[] { "ф", "f" });

            return collection;
        }

        private static Dictionary<int, string[][]> GenerateTimeUnits()
        {
            Dictionary<int, string[][]> collection = new Dictionary<int, string[][]>();
            collection.Add(31556952, new string[][] { // average seconds in one year
                new string[] { "лет", "лет", "год", "года", "г", "л" },
                new string[] { "y", "years", "year" },
            });
            collection.Add(2629746, new string[][] { // average seconds in one month
                new string[] { "мес", "месяцев", "месяц", "месяца"},
                new string[] { "m", "months", "month", null, "mon" },
            });
            collection.Add(604800, new string[][] { // seconds in week
                new string[] { "нед", "недель", "неделя", "недели", "н" },
                new string[] { "w", "weeks", "week" },
            });
            collection.Add(86400, new string[][] { // seconds in day
                new string[] { "сут", "дней", "день", "дня", "суток", "д" },
                new string[] { "d", "days", "day" },
            });
            collection.Add(3600, new string[][] { // seconds in hour
                new string[] { "ч", "часов", "час", "часа" },
                new string[] { "h", "hours", "hour" },
            });
            collection.Add(60, new string[][] { // seconds in minute
                new string[] { "мин", "минут", "минута", "минуты" },
                new string[] { "min", "minutes", "minute" },
            });
            collection.Add(1, new string[][] { // seconds in second
                new string[] { "с", "секунд", "секунда", "секунды", "сек" },
                new string[] { "s", "seconds", "second", null, "sec" },
            });
            return collection;
        }

        private static List<string[]> GenerateBaseUnits()
        {
            List<string[]> collection = new List<string[]>();
            return collection;
        }

        private static int SetupBaseUnitIndex(string key, params string[] units)
        {
            int index = -1;
            int count = BaseUnits.Count;
            for (int ix = 0; ix < count; ++ix) {
                string[] registered = BaseUnits[ix];
                bool found1 = registered.Contains(key, StringComparer.CurrentCultureIgnoreCase);
                bool found2 = registered.Intersect(units, StringComparer.CurrentCultureIgnoreCase).Count() > 0;
                if (found1 || found2) {
                    if (registered[0] == key) {
                        index = ix;
                    } else {
                        throw new ArgumentException("Similar unit is already registered");
                    }
                }
            }
            return index;
        }

        private static string[] PrepareUnits(string[] baseUnitVariants)
        {
            if (baseUnitVariants == null) {
                return null;
            } else if (baseUnitVariants.Length == 0) {
                return GetRegisteredBaseUnits();
            } else {
                return baseUnitVariants;
            }
        }

        private static bool BaseUnitVariantsNotSet(string[] baseUnitVariants)
        {
            return baseUnitVariants != null && baseUnitVariants.Length == 0;
        }

        public static bool UnitsAreCompatible(string unit1, string unit2, params string[] baseUnitVariants)
        {
            string u1 = ExtractBaseUnit(unit1, true, baseUnitVariants);
            string u2 = ExtractBaseUnit(unit2, true, baseUnitVariants);
            return u1 == u2;
        }

        public static string[] GetRegisteredBaseUnits()
        {
            List<string> result = new List<string>();
            int count = BaseUnits.Count;
            for (int k = 0; k < count; ++k) {
                result.AddRange(baseUnits[k]);
            }
            return result.ToArray();
        }

        public static void RegisterBaseUnit(params string[] unitVariants)
        {
            if (unitVariants != null && unitVariants.Length > 0) {
                int index = SetupBaseUnitIndex(unitVariants[0], unitVariants);
                if (index < 0) {
                    BaseUnits.Add(unitVariants);
                } else {
                    BaseUnits[index] = unitVariants;
                }
            }
        }

        public static void RegisterBaseUnit(string key, int variantIndex, string unit)
        {
            int index = SetupBaseUnitIndex(key, new string[1] { unit });
            if (index < 0) {
                string[] item = new string[variantIndex + 1];
                item[0] = key;
                item[variantIndex] = unit;
            } else {
                string[] item = BaseUnits[index];
                if (item.Length > variantIndex) {
                    item[variantIndex] = unit;
                } else {
                    Array.Resize(ref item, variantIndex + 1);
                    item[variantIndex] = unit;
                    BaseUnits[index] = item;
                }
            }
        }

        public static string[] GetBaseUnitVariantsFor(string baseUnit)
        {
            if (string.IsNullOrEmpty(baseUnit)) {
                return null;
            }
            foreach (string[] group in BaseUnits) {
                if (group.Contains(baseUnit, StringComparer.CurrentCultureIgnoreCase)) {
                    return group;
                }
            }
            return null;
        }

        public static decimal GetTimeMultiplier(string timeUnit)
        {
            string[] secUnits = TimeUnits[1].Select(a => a[0]).ToArray();
            int powerOf10;
            if (ValidateUnit(timeUnit, out powerOf10, secUnits) && powerOf10 <= 0) {
                decimal x = 1;
                for (int i = 0; i < -powerOf10; ++i) { x = x / 10; }
                return x;
            } else {
                foreach (KeyValuePair<int, string[][]> item in TimeUnits) {
                    foreach (string[] variants in item.Value) {
                        foreach (string uv in variants) {
                            if (String.Equals(uv, timeUnit, StringComparison.CurrentCulture)) {
                                return item.Key;
                            }
                        }
                    }
                }
                return 0;
            }
        }

        public static bool ValidateTimeUnit(string timeUnit)
        {
            return !string.IsNullOrEmpty(timeUnit) && GetTimeMultiplier(timeUnit) > 0;
        }

        public static string NormalizeTimeUnit(string timeUnit, int varinantIndex)
        {
            decimal m = GetTimeMultiplier(timeUnit);
            if (m < 1) {
                string[] secUnits = TimeUnits[1].Select(a => a[0]).ToArray();
                return NormalizeUnit(timeUnit, varinantIndex, secUnits);
            } else {
                int x = (int)Math.Round(m);
                return GetTimeUnit(x, varinantIndex);
            }
        }

        public static string NormalizeTimeUnit(string timeUnit)
        {
            return NormalizeTimeUnit(timeUnit, 0);
        }

        public static string GetTimeUnit(int multiplier, int varinantIndex)
        {
            if (TimeUnits.ContainsKey(multiplier)) {
                return TimeUnits[multiplier][varinantIndex][0];
            }
            return null;
        }

        public static string GetTimeUnit(int multiplier)
        {
            return GetTimeUnit(multiplier, 0);
        }

        public static string ExtractBaseUnit(string unit, string prefix)
        {
            if (prefix == null || unit == null) {
                return unit;
            } else {
                return unit.Substring(prefix.Length);
            }
        }

        public static string ExtractBaseUnit(string unit, int variantIndex, params string[] baseUnitVariants)
        {
            string prefix = ExtractPrefix(unit, baseUnitVariants);
            string baseUnit = ExtractBaseUnit(unit, prefix);
            if (variantIndex < 0) {
                return baseUnit;
            }
            if (BaseUnitVariantsNotSet(baseUnitVariants)) {
                baseUnitVariants = GetBaseUnitVariantsFor(baseUnit);
            }
            if (baseUnitVariants != null && variantIndex < baseUnitVariants.Length) {
                return baseUnitVariants[variantIndex];
            }
            return null;
        }

        public static string ExtractBaseUnit(string unit, bool normalize, params string[] baseUnitVariants)
        {
            return ExtractBaseUnit(unit, normalize ? 0 : -1, baseUnitVariants);
        }

        public static string PrefixFor(int powerOf10, int varinantIndex)
        {
            if (powerOf10 == 0) {
                return "";
            }
            if (Prefixes.ContainsKey(powerOf10)) {
                return Prefixes[powerOf10][varinantIndex];
            }
            return null;
        }

        public static string PrefixFor(int powerOf10)
        {
            return PrefixFor(powerOf10, 0);
        }

        public static string ExtractPrefix(string unit, params string[] baseUnitVariants)
        {
            baseUnitVariants = PrepareUnits(baseUnitVariants);
            if (unit == null) {
                return null;
            }
            if (baseUnitVariants == null) {
                return "";
            }
            foreach (string s in baseUnitVariants) {
                if (s.Length <= unit.Length) {
                    int pos = unit.Length - s.Length;
                    string x = unit.Substring(pos);
                    if (String.Equals(x, s, StringComparison.CurrentCultureIgnoreCase)) {
                        return unit.Substring(0, pos);
                    }
                }
            }
            return null;
        }

        public static bool ValidatePrefix(string prefix, out int powerOf10)
        {
            powerOf10 = 0;
            if (prefix == null) {
                return false;
            }
            if (prefix.Length == 0) {
                return true;
            }
            foreach (KeyValuePair<int, string[]> item in Prefixes) {
                foreach (string pv in item.Value) {
                    if (String.Equals(prefix, pv, StringComparison.CurrentCulture)) {
                        powerOf10 = item.Key;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool ValidatePrefix(string prefix)
        {
            int powerOf10;
            return ValidatePrefix(prefix, out powerOf10);
        }

        public static bool ValidateUnit(string unit, out int powerOf10, out string prefix, params string[] baseUnitVariants)
        {
            baseUnitVariants = PrepareUnits(baseUnitVariants);
            if (baseUnitVariants == null) {
                if (String.IsNullOrEmpty(unit)) {
                    powerOf10 = 0;
                    prefix = "";
                    return true;
                }
            } else {
                prefix = ExtractPrefix(unit, baseUnitVariants);
                if (prefix != null) {
                    return ValidatePrefix(prefix, out powerOf10);
                }
            }
            powerOf10 = 0;
            prefix = null;
            return false;
        }

        public static bool ValidateUnit(string unit, out int powerOf10, params string[] baseUnitVariants)
        {
            string prefix;
            return ValidateUnit(unit, out powerOf10, out prefix, baseUnitVariants);
        }

        public static bool ValidateUnit(string unit, params string[] baseUnitVariants)
        {
            int powerOf10;
            return ValidateUnit(unit, out powerOf10, baseUnitVariants);
        }

        public static string NormalizeUnit(string unit, int variantIndex, params string[] baseUnitVariants)
        {
            int powerOf10;
            if (ValidateUnit(unit, out powerOf10, baseUnitVariants)) {
                string prefix = PrefixFor(powerOf10, variantIndex);
                if (string.IsNullOrEmpty(unit)) {
                    return unit;
                }
                if (BaseUnitVariantsNotSet(baseUnitVariants)) {
                    baseUnitVariants = GetBaseUnitVariantsFor(ExtractBaseUnit(unit, prefix));
                }
                if (prefix != null && baseUnitVariants != null && variantIndex < baseUnitVariants.Length) {
                    return prefix + baseUnitVariants[variantIndex];
                }
            }
            return null;
        }

        public static string NormalizeUnit(string unit, params string[] baseUnitVariants)
        {
            return NormalizeUnit(unit, 0, baseUnitVariants);
        }

        public static int GetPowerOf10(string prefix)
        {
            int powerOf10;
            if (ValidatePrefix(prefix, out powerOf10)) {
                return powerOf10;
            } else {
                throw new ArgumentException("Invalid prefix supplied");
            }
        }

        public static int GetPowerOf10(string unit, params string[] baseUnitVariants)
        {
            int powerOf10;
            if (ValidateUnit(unit, out powerOf10, baseUnitVariants)) {
                return powerOf10;
            } else {
                throw new ArgumentException("Invalid unit supplied");
            }
        }
    }
}
