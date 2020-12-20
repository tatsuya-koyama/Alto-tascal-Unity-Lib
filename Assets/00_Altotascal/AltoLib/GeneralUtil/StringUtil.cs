using System;
using System.Linq;

namespace AltoLib
{
    public class StringUtil
    {
        public static string Repeat(string str, int times)
        {
            if (times <= 0) { return String.Empty; }

            return string.Concat(Enumerable.Repeat(str, times));
        }
    }
}
