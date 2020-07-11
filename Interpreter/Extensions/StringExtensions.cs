using System;

namespace Interpreter.Extensions
{
    public static class StringExtensions
    {
        public static double ToNumber(this string @string)
        {
            return double.Parse(@string);
        }

        public static bool ToBool(this string @string)
        {
            if(@string.ToLower() != @string)
            {
                throw new FormatException(@string);
            }

            return bool.Parse(@string);
        }
    }
}
