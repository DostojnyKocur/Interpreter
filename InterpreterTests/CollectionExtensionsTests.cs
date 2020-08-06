using System.Collections.Generic;
using Interpreter.Common.Extensions;
using NUnit.Framework;

namespace InterpreterTests
{
    [TestFixture]
    public class CollectionExtensionsTests
    {
        [Test]
        public void ToPrintCorrectValues()
        {
            var collection = new List<dynamic>
            {
                1, 2, 3
            };

            string result = collection.ToPrint();

            Assert.AreEqual("[1, 2, 3]", result);
        }
    }
}