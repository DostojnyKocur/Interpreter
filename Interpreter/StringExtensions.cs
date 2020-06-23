namespace Interpreter
{
    public static class StringExtensions
    {
        public static double ToNumber(this string @string)
        {
            return double.Parse(@string);
        }
    }
}
