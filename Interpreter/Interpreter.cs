using System;
using System.Linq;

namespace Interpreter
{
    public class Interpreter
    {
        private static readonly TokenType[] Operators = { TokenType.Plus, TokenType.Minus, TokenType.Mul, TokenType.Div, TokenType.Mod };

        private string _text = string.Empty;
        private int _position = 0;
        private char _currentChar = char.MinValue;
        private Token _currentToken = null;

        public Interpreter(string text) => (_text, _currentChar) = (text, text[_position]);

        public string Run()
        {
            _currentToken = GetNextToken();

            var result = Term();

            while(Operators.Contains(_currentToken.Type))
            {
                var token = _currentToken;

                switch (token.Type)
                {
                    case TokenType.Plus:
                        Eat(TokenType.Plus);
                        result += Term();
                        break;
                    case TokenType.Minus:
                        Eat(TokenType.Minus);
                        result -= Term();
                        break;
                    case TokenType.Mul:
                        Eat(TokenType.Mul);
                        result *= Term();
                        break;
                    case TokenType.Div:
                        Eat(TokenType.Div);
                        result /= Term();
                        break;
                    case TokenType.Mod:
                        Eat(TokenType.Mod);
                        result %= Term();
                        break;
                }
            }

            return $"{result}";
        }

        private double Term()
        {
            var token = _currentToken;
            Eat(TokenType.Number);

            return token.Value.ToNumber();
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
