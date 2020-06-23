using System;

namespace Interpreter
{
    public class Interpreter
    {
        private string _text = string.Empty;
        private int _position = 0;
        private Token _currentToken = null;

        public Interpreter(string text) => _text = text;

        public string Run()
        {
            _currentToken = GetNextToken();

            var left = _currentToken;

            Eat(TokenType.Number);

            var @operator = _currentToken;

            Eat(TokenType.Plus);

            var right = _currentToken;

            Eat(TokenType.Number);

            var result = left.Value.ToNumber() + right.Value.ToNumber();

            return $"{result}";
        }

        private Token GetNextToken()
        {
            var text = _text;

            if(_position > text.Length - 1)
            {
                return new Token(TokenType.EOF);
            }

            var currentChar = text[_position];

            if(char.IsDigit(currentChar))
            {
                _position += 1;
                return new Token(TokenType.Number, $"{currentChar}");
            }

            if(currentChar == '+')
            {
                _position += 1;
                return new Token(TokenType.Plus, $"{currentChar}");
            }

            throw new InvalidOperationException("Error parsing input");
        }

        private void Eat(TokenType tokenType)
        {
            if(_currentToken.Type == tokenType)
            {
                _currentToken = GetNextToken();
            }
        }
    }
}
