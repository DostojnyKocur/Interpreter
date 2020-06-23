using NUnit.Framework;

namespace InterpreterTests
{
    public class InterpreterTests
    {
        [TestCase("1+1", "2")]
        [TestCase("7+9", "16")]
        public void CorrectValues(string value, string expected)
        {
            var interpreter = new Interpreter.Interpreter(value);
            var result = interpreter.Run();

            Assert.AreEqual(expected, result);
        }
    }
}
