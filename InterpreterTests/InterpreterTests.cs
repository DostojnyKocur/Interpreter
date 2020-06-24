using NUnit.Framework;

namespace InterpreterTests
{
    public class InterpreterTests
    {
        [TestCase("3.2    -   1", "2.2")]
        [TestCase("7.9   +  9.1", "17")]
        public void CorrectValues(string value, string expected)
        {
            var interpreter = new Interpreter.Interpreter(value);
            var result = interpreter.Run();

            Assert.AreEqual(expected, result);
        }
    }
}
