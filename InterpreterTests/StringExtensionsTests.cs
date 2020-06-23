using System;
using Interpreter;
using NUnit.Framework;

namespace InterpreterTests
{
    public class StringExtensionsTests
    {
        [TestCase("1", 1)]
        [TestCase("1.1", 1.1)]
        [TestCase("6.87", 6.87)]
        public void CorrectValues(string value, double expected)
        {
            double result = value.ToNumber();

            Assert.AreEqual(expected, result);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("a")]
        [TestCase("@")]
        public void ThrowExceptions(string value)
        {
            Assert.Throws<FormatException>(() => value.ToNumber());
        }
    }
}