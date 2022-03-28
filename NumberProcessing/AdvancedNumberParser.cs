using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NumberProcessing
{
    public static class AdvancedNumberParser
    {
        private static readonly string PlusMinusRx = AdvancedNumberFormat.PlusMinus.ToString();
        private static readonly string MinusSignsRx = "[" + SpecialChars.EscapeRegex(SpecialChars.MinusSigns) + "]";
        private static readonly string MinusPlusSignsRx = "[" + SpecialChars.EscapeRegex(SpecialChars.MinusSigns + SpecialChars.PlusSigns) + "]";

        private static readonly string RxValue = "(?<m>" + MinusSignsRx + ")?(?<i>\\d+)(?:[\\.,](?<f>\\d*))?";
        private static readonly string RxValueSpaced = "(?<m>" + MinusSignsRx + ")?\\s*(?<i>\\d+)\\s*(?:[\\.,]\\s*(?<f>\\d*))?";
        private static readonly string RxExpVar1 = "(?:E\\s*(?<em>" + MinusPlusSignsRx + ")?(?<e>\\d+))";
        private static readonly string RxExpVar2 = "(?:[" + SpecialChars.MultiplySigns + "]\\s*10\\s*E?(?<em>" + MinusPlusSignsRx + ")?(?<e>\\d+))";
        private static readonly string RxErrorValue = "(?<di>\\d+)(?:[\\.,](?<df>\\d*))?";
        private static readonly string RxErrorValueSpaced = "(?<di>\\d+)\\s*(?:[\\.,]\\s*(?<df>\\d*))?";
        private static readonly string RxErrorExp = "(?:E\\s*(?<dem>\\-)?(?<de>\\d+))";
        private static readonly string RxErrorVar1 = "(?:\\((?<d>\\d+)\\))";
        private static readonly string RxErrorVar1Spaced = "(?:\\(\\s*(?<d>\\d+)\\s*\\))";
        private static readonly string RxErrorVar2 = "(?:" + PlusMinusRx + "\\s*" + RxErrorValue + ")";
        private static readonly string RxErrorVar2Spaced = "(?:" + PlusMinusRx + "\\s*" + RxErrorValueSpaced + ")";
        private static readonly string RxErrorVar3 = "(?<sd>" + PlusMinusRx + "\\s*" + RxErrorValue + "\\s*" + RxErrorExp + ")";
        private static readonly string RxErrorVar3Spaced = "(?<sd>" + PlusMinusRx + "\\s*" + RxErrorValueSpaced + "\\s*" + RxErrorExp + ")";

        private static readonly string RxAllExpError = RxExpVar1 + "\\s*" + "(?:" + RxErrorVar1 + "|" + RxErrorVar3 + ")";
        private static readonly string RxAllExpErrorSpaced = RxExpVar1 + "\\s*" + "(?:" + RxErrorVar1Spaced + "|" + RxErrorVar3Spaced + ")";
        private static readonly string RxAllErrorExp = RxErrorVar1 + "\\s*" + "(?:" + RxExpVar1 + "|" + RxExpVar2 + ")";
        private static readonly string RxAllErrorExpSpaced = RxErrorVar1Spaced + "\\s*" + "(?:" + RxExpVar1 + "|" + RxExpVar2 + ")";
        private static readonly string RxAllError = "(?:" + RxErrorVar1 + "|" + RxErrorVar2 + ")";
        private static readonly string RxAllErrorSpaced = "(?:" + RxErrorVar1Spaced + "|" + RxErrorVar2Spaced + ")";
        private static readonly string RxAllExp = "(?:" + RxExpVar1 + "|" + RxExpVar2 + ")";
        private static readonly string RxAllExpSpaced = "(?:" + RxExpVar1 + "|" + RxExpVar2 + ")";

        private static readonly string RxMainVar1 = RxValue + "\\s*" + "(?:" + RxAllExpError + "|" + RxAllErrorExp + "|" + RxAllError + "|" + RxAllExp + ")?";
        private static readonly string RxMainVar1Spaced = RxValueSpaced + "\\s*" + "(?:" + RxAllExpErrorSpaced + "|" + RxAllErrorExpSpaced + "|" + RxAllErrorSpaced + "|" + RxAllExpSpaced + ")?";
        private static readonly string RxMainVar2 = "\\(\\s*" + RxValue + "\\s*" + RxErrorVar2 + "\\s*\\)" + "\\s*" + "(?:" + RxExpVar1 + "|" + RxExpVar2 + ")";
        private static readonly string RxMainVar2Spaced = "\\(\\s*" + RxValueSpaced + "\\s*" + RxErrorVar2Spaced + "\\s*\\)" + "\\s*" + "(?:" + RxExpVar1 + "|" + RxExpVar2 + ")";

        private static readonly string RxMainAll = "(?:" + RxMainVar1 + "|" + RxMainVar2 + ")";
        private static readonly string RxMainAllSpaced = "(?:" + RxMainVar1Spaced + "|" + RxMainVar2Spaced + ")";

        private static readonly Regex NumberRegex = new Regex("^" + RxMainAll + "$", RegexOptions.IgnoreCase);
        private static readonly Regex NumberRegexSpaced = new Regex("^\\s*" + RxMainAllSpaced + "\\s*$", RegexOptions.IgnoreCase);

        public static Regex GetRegex(bool strict)
        {
            return strict ? NumberRegex : NumberRegexSpaced;
        }

        public static Regex GetRegex()
        {
            return NumberRegex;
        }

        public static Regex GetGlobalRegex(bool strict)
        {
            return new Regex(GetRegexString(strict), RegexOptions.IgnoreCase);
        }

        public static Regex GetGlobalRegex()
        {
            return new Regex(GetRegexString(), RegexOptions.IgnoreCase);
        }

        public static string GetRegexString(bool strict)
        {
            return strict ? RxMainAll : RxMainAllSpaced;
        }

        public static string GetRegexString()
        {
            return RxMainAll;
        }

        public static MatchCollection MatchAll(string s, bool strict)
        {
            return GetGlobalRegex(strict).Matches(s);
        }

        public static Match Match(string s, bool strict)
        {
            return GetRegex(strict).Match(s);
        }

        public static AdvancedNumber Parse(Match match)
        {
            Group m = match.Groups["m"];     // minus
            Group i = match.Groups["i"];     // int part
            Group f = match.Groups["f"];     // float part
            Group em = match.Groups["em"];   // exp minus
            Group e = match.Groups["e"];     // exp value
            Group d = match.Groups["d"];     // D in brackets
            Group di = match.Groups["di"];   // D int part
            Group df = match.Groups["df"];   // D float part
            Group dem = match.Groups["dem"]; // D exp minus
            Group de = match.Groups["de"];   // D exp value
            Group sd = match.Groups["sd"];   // sepadated D exp

            string n = i.Value + (f.Length > 0 ? ("." + f.Value) : "");
            decimal v = decimal.Parse(n, CultureInfo.InvariantCulture);
            if (m.Length > 0) {
                v = -v;
            }

            decimal dv = 0;
            int exp = 0;

            if (e.Success) {
                exp = int.Parse(e.Value);
                if (em.Length > 0 && em.Value != "+") {
                    exp = -exp;
                }
            }

            if (d.Length > 0) {
                dv = int.Parse(d.Value);
                if (f.Length > 0) {
                    dv *= DecimalUtils.Pow10(-f.Length);
                }
            } else if (di.Success) {
                string nd = di.Value + (df.Length > 0 ? ("." + df.Value) : "");
                dv = decimal.Parse(nd, CultureInfo.InvariantCulture);
                if (sd.Success) {
                    int dexp = 0;
                    if (de.Success) {
                        dexp = int.Parse(de.Value);
                        if (dem.Length > 0) dexp = -dexp;
                    }
                    int ded = dexp - exp;
                    if (ded != 0) {
                        dv *= DecimalUtils.Pow10(ded);
                    }
                }
            } else {
                dv = 0;
            }

            return new AdvancedNumber(v, dv, exp);
        }

        public static bool TryParse(Match match, out AdvancedNumber number)
        {
            try {
                number = Parse(match);
                return true;
            } catch (FormatException) {
                number = default(AdvancedNumber);
                return false;
            } catch (OverflowException) {
                number = default(AdvancedNumber);
                return false;
            }
        }

        public static AdvancedNumber Parse(string s, bool strict, out Match match)
        {
            match = Match(s, strict);
            if (match.Success) {
                return Parse(match);
            } else {
                throw new FormatException();
            }
        }

        public static bool TryParse(string s, bool strict, out AdvancedNumber number, out Match match)
        {
            match = Match(s, strict);
            if (match.Success) {
                return TryParse(match, out number);
            } else {
                number = default(AdvancedNumber);
                return false;
            }
        }

        public static KeyValuePair<Match, AdvancedNumber>[] ParseAll(string s, bool strict)
        {
            MatchCollection matches = MatchAll(s, strict);
            List<KeyValuePair<Match, AdvancedNumber>> result = new List<KeyValuePair<Match, AdvancedNumber>>();
            foreach (Match m in matches) {
                if (TryParse(m, out AdvancedNumber n)) {
                    result.Add(new KeyValuePair<Match, AdvancedNumber>(m, n));
                }
            }
            return result.ToArray();
        }
    }
}
