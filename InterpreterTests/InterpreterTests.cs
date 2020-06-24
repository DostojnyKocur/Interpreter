using Interpreter;
using NUnit.Framework;

namespace InterpreterTests
{
    public class InterpreterTests
    {
        [TestCase("3.2    -   1", "2.2")]
        [TestCase("7.9   +  9.1", "17")]
        [TestCase("6%4 * 9", "18")]
        [TestCase("14 + 2 * 3 - 6 / 2", "17")]
        public void CorrectValues(string value, string expected)
        {
            var lexer = new Lexer(value);
            var interpreter = new Interpreter.Interpreter(lexer);
            var result = interpreter.Run();

            Assert.AreEqual(expected, result);
        }
    }
}
