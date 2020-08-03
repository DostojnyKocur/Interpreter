using Interpreter.Common.Tokens;
using Interpreter.LexerService;
using NUnit.Framework;

namespace InterpreterTests
{
    [TestFixture]
    public class LexeerServiceTests
    {
        [TestCase("+", TokenType.Plus)]
        [TestCase(">=", TokenType.GreaterEqual)]
        [TestCase("void", TokenType.TypeVoid)]
        [TestCase("variable", TokenType.Identifier)]
        public void When_find_token_in_text_Then_return_token(string text, TokenType expectedTokenType)
        {
            var lexer = new Lexer(text);
            var token = lexer.GetNextToken();

            Assert.AreEqual(expectedTokenType, token.Type);
        }
    }
}
