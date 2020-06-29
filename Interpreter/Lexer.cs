using System;
using System.Collections.Generic;
using Interpreter.Tokens;

namespace Interpreter
{
    public class Lexer
    {
        private static readonly Dictionary<string, Token> ReserverKeywords = new Dictionary<string, Token>
        {
            { "number",  new Token(TokenType.TypeNumber, "number") }
        };

        private string _text = string.Empty;
        private int _position = 0;
        private char _currentChar = char.MinValue;

        public Lexer(string text) => (_text, _currentChar) = (text, text[_position]);

        public Token GetNextToken()
        {
            while (_currentChar != char.MaxValue)
            {
                if (char.IsWhiteSpace(_currentChar))
                {
                    SkipWhitespace();
                    continue;
                }

                if (char.IsDigit(_currentChar))
                {
                    return GetNumber();
                }

                if (char.IsLetterOrDigit(_currentChar))
                {
                    return GetId();
                }

                switch (_currentChar)
                {
                    case '+':
                        Advance();
                        return new Token(TokenType.Plus);
                    case '-':
                        Advance();
                        return new Token(TokenType.Minus);
                    case '*':
                        Advance();
                        return new Token(TokenType.Multiplication);
                    case '/':
                        if (Peek() == '*')
                        {
                            SkipComment();
                            continue;
                        }
                        else
                        {
                            Advance();
                            return new Token(TokenType.Divide);
                        }
                    case '%':
                        Advance();
                        return new Token(TokenType.Modulo);
                    case '(':
                        Advance();
                        return new Token(TokenType.LeftParen);
                    case ')':
                        Advance();
                        return new Token(TokenType.RightParen);
                    case '{':
                        Advance();
                        return new Token(TokenType.ScopeBegin);
                    case '}':
                        Advance();
                        return new Token(TokenType.ScopeEnd);
                    case '=':
                        Advance();
                        return new Token(TokenType.Assign);
                    case ';':
                        Advance();
                        return new Token(TokenType.Semicolon);
                    case ',':
                        Advance();
                        return new Token(TokenType.Comma);
                }

                throw new InvalidOperationException("Error parsing input");
            }

            return new Token(TokenType.EOF);
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

        private char Peek()
        {
            var peekPosition = _position + 1;

            if (peekPosition > _text.Length - 1)
            {
                return char.MaxValue;
            }
            else
            {
                return _text[peekPosition];
            }
        }

        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(_currentChar) && _currentChar != char.MaxValue)
            {
                Advance();
            }
        }

        private void SkipComment()
        {
            while ((_currentChar != '*') || (Peek() != '/'))
            {
                Advance();
            }

            Advance();  // '*'
            Advance();  // '/'
        }

        private Token GetId()
        {
            var result = string.Empty;

            while ((char.IsLetterOrDigit(_currentChar) || _currentChar == '_') && _currentChar != char.MaxValue)
            {
                result += _currentChar;
                Advance();
            }

            if (ReserverKeywords.ContainsKey(result))
            {
                return ReserverKeywords[result];
            }

            return new Token(TokenType.Id, result);
        }

        private Token GetNumber()
        {
            var result = string.Empty;

            while ((char.IsDigit(_currentChar) || _currentChar == '.') && _currentChar != char.MaxValue)
            {
                result += _currentChar;
                Advance();
            }

            return new Token(TokenType.ConstNumber, result);
        }
    }
}
