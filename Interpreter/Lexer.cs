using System.Collections.Generic;
using Interpreter.Errors;
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
        private uint _lineNumber = 1;
        private uint _column = 1;

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

                if(_currentChar == '/')
                {
                    if (Peek() == '*')
                    {
                        SkipComment();
                        continue;
                    }
                    else
                    {
                        Advance();
                        return new Token(TokenType.Divide, null, _lineNumber, _column);
                    }
                }
                else
                {
                    var tokenType = TokenTypes.GetForChar(_currentChar);
                    Advance();
                    if (tokenType is null)
                    {
                        var message = $"Lexer error on '{_currentChar}' line: {_lineNumber} column: {_column}";
                        throw new LexerError(message: message);
                    }
                    return new Token((TokenType)tokenType, null, _lineNumber, _column);
                }
            }

            return new Token(TokenType.EOF, null, _lineNumber, _column);
        }

        private void Advance()
        {
            if(_currentChar == '\n')
            {
                _lineNumber += 1;
                _column = 0;
            }

            _position += 1;

            if (_position > _text.Length - 1)
            {
                _currentChar = char.MaxValue;
            }
            else
            {
                _currentChar = _text[_position];
                _column += 1;
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

            return new Token(TokenType.Id, result, _lineNumber, _column);
        }

        private Token GetNumber()
        {
            var result = string.Empty;

            while ((char.IsDigit(_currentChar) || _currentChar == '.') && _currentChar != char.MaxValue)
            {
                result += _currentChar;
                Advance();
            }

            return new Token(TokenType.ConstNumber, result, _lineNumber, _column);
        }
    }
}
