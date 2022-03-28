using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NumberProcessing
{
    public static class SpecialChars
    {
        public static readonly string MultiplySigns = "*•·×⨯x";
        public static readonly string MinusSigns = "-−—–";
        public static readonly string PlusSigns = "+";

        public static string EscapeRegex(string str)
        {
            return Regex.Escape(str).Replace("-", "\\-");
        }
    }
}
