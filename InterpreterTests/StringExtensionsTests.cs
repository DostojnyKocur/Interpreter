using System;
using Interpreter.Common.Extensions;
using NUnit.Framework;

namespace InterpreterTests
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [TestCase("1", 1)]
        [TestCase("1.1", 1.1)]
        [TestCase("6.87", 6.87)]
        public void ToNumberCorrectValues(string value, double expected)
        {
            double result = value.ToNumber();

            Assert.AreEqual(expected, result);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("a")]
        [TestCase("@")]
        public void ToNumberThrowExceptions(string value)
        {
            Assert.Throws<FormatException>(() => value.ToNumber());
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        public void ToBoolCorrectValues(string value, bool expected)
        {
            bool result = value.ToBool();

            Assert.AreEqual(expected, result);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("a")]
        [TestCase("@")]
        [TestCase("True")]
        [TestCase("FALSE")]
        public void ToBoolThrowExceptions(string value)
        {
            Assert.Throws<FormatException>(() => value.ToBool());
        }
    }
}