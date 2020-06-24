using System;

namespace Interpreter
{
    public class Interpreter
    {
        private string _text = string.Empty;
        private int _position = 0;
        private char _currentChar = char.MinValue;
        private Token _currentToken = null;

        public Interpreter(string text) => (_text, _currentChar) = (text, text[_position]);

        public string Run()
        {
            _currentToken = GetNextToken();

            var left = _currentToken;

            Eat(TokenType.Number);

            var operation = _currentToken;

            Eat(operation.Type);

            var right = _currentToken;

            Eat(TokenType.Number);

            double result = 0;
            switch(operation.Type)
            {
                case TokenType.Plus:
                    result = left.Value.ToNumber() + right.Value.ToNumber();
                    break;
                case TokenType.Minus:
                    result = left.Value.ToNumber() - right.Value.ToNumber();
                    break;
                case TokenType.Mul:
                    result = left.Value.ToNumber() * right.Value.ToNumber();
                    break;
                case TokenType.Div:
                    result = left.Value.ToNumber() / right.Value.ToNumber();
                    break;
                case TokenType.Mod:
                    result = left.Value.ToNumber() % right.Value.ToNumber();
                    break;
            }

            return $"{result}";
        }

        private Token GetNextToken()
        {
            while(_currentChar != char.MaxValue)
            {
                if(char.IsWhiteSpace(_currentChar))
                {
                    SkipWhitespace();
                    continue;
                }

                if(char.IsDigit(_currentChar))
                {
                    return new Token(TokenType.Number, GetNumber());
                }

                switch(_currentChar)
                {
                    case '+':
                        Advance();
                        return new Token(TokenType.Plus);
                    case '-':
                        Advance();
                        return new Token(TokenType.Minus);
                    case '*':
                        Advance();
                        return new Token(TokenType.Mul);
                    case '/':
                        Advance();
                        return new Token(TokenType.Div);
                    case '%':
                        Advance();
                        return new Token(TokenType.Mod);
                }


                throw new InvalidOperationException("Error parsing input");
            }

            return new Token(TokenType.EOF);
        }

        private void Eat(TokenType tokenType)
        {
            if(_currentToken.Type == tokenType)
            {
                _currentToken = GetNextToken();
            }
            else
            {
                throw new InvalidOperationException("Error parsing input");
            }
        }

        private void Advance()
        {
            _position += 1;

            if (_position > _text.Length - 1)
            {
                _currentChar = char.MaxValue;
            }
            else
            {
                _currentChar = _text[_position];
            }
        }

        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(_currentChar) && _currentChar != char.MaxValue)
            {
                Advance();
            }
        }

        private string GetNumber()
        {
            var result = string.Empty;

            while ((char.IsDigit(_currentChar) || _currentChar == '.') && _currentChar != char.MaxValue)
            {
                result += _currentChar;
                Advance();
            }

            return result;
        }
    }
}
