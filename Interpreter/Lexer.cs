using System.Collections.Generic;
using Interpreter.Errors;
using Interpreter.Tokens;

namespace Interpreter
{
    public class Lexer
    {
        private static readonly Dictionary<string, Token> ReserverKeywords = new Dictionary<string, Token>
        {
            { "return",  new Token(TokenType.Return, "return") },
            { "void",  new Token(TokenType.TypeVoid, "void") },
            { "number",  new Token(TokenType.TypeNumber, "number") },
            { "bool",  new Token(TokenType.TypeBool, "bool") },
            { "true",  new Token(TokenType.ConstBool, "true") },
            { "false",  new Token(TokenType.ConstBool, "false") },
            { "if",  new Token(TokenType.If, "if") },
            { "else",  new Token(TokenType.Else, "else") },
            { "while",  new Token(TokenType.While, "while") }
        };

        private string _text = string.Empty;
        private int _position = 0;
        private uint _lineNumber = 1;
        private uint _column = 1;

        public Lexer(string text) => (_text, CurrentChar) = (text, text[_position]);

        public char CurrentChar { get; private set; } = char.MinValue;

        public Token GetNextToken()
        {
            while (CurrentChar != char.MaxValue)
            {
                if (char.IsWhiteSpace(CurrentChar))
                {
                    SkipWhitespace();
                    continue;
                }

                if (char.IsDigit(CurrentChar))
                {
                    return GetNumber();
                }

                if (char.IsLetterOrDigit(CurrentChar))
                {
                    return GetId();
                }

                if (CurrentChar == '/')
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
                    var compareOperator = GetCompareOperator();
                    if (compareOperator != null)
                    {
                        return compareOperator;
                    }

                    var tokenType = TokenTypes.GetForChar(CurrentChar);
                    Advance();
                    if (tokenType is null)
                    {
                        var message = $"Lexer error on '{CurrentChar}' line: {_lineNumber} column: {_column}";
                        throw new LexerError(message: message);
                    }
                    return new Token((TokenType)tokenType, null, _lineNumber, _column);
                }
            }

            return new Token(TokenType.EOF, null, _lineNumber, _column);
        }

        private void Advance()
        {
            if (CurrentChar == '\n')
            {
                _lineNumber += 1;
                _column = 0;
            }

            _position += 1;

            if (_position > _text.Length - 1)
            {
                CurrentChar = char.MaxValue;
            }
            else
            {
                CurrentChar = _text[_position];
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
            while (char.IsWhiteSpace(CurrentChar) && CurrentChar != char.MaxValue)
            {
                Advance();
            }
        }

        private void SkipComment()
        {
            while ((CurrentChar != '*') || (Peek() != '/'))
            {
                Advance();
            }

            Advance();  // '*'
            Advance();  // '/'
        }

        private Token GetCompareOperator()
        {
            switch (CurrentChar)
            {
                case '=':
                    if (Peek() == '=')
                    {
                        Advance();
                        Advance();
                        return new Token(TokenType.Equal, null, _lineNumber, _column);
                    }
                    break;
                case '>':
                    if (Peek() == '=')
                    {
                        Advance();
                        Advance();
                        return new Token(TokenType.GreaterEqual, null, _lineNumber, _column);
                    }
                    Advance();
                    return new Token(TokenType.Greater, null, _lineNumber, _column);
                case '<':
                    if (Peek() == '=')
                    {
                        Advance();
                        Advance();
                        return new Token(TokenType.LessEqual, null, _lineNumber, _column);
                    }
                    Advance();
                    return new Token(TokenType.Less, null, _lineNumber, _column);
                case '!':
                    if (Peek() == '=')
                    {
                        Advance();
                        Advance();
                        return new Token(TokenType.NotEqual, null, _lineNumber, _column);
                    }
                    Advance();
                    return new Token(TokenType.Not, null, _lineNumber, _column);
                case '&':
                    if (Peek() == '&')
                    {
                        Advance();
                        Advance();
                        return new Token(TokenType.And, null, _lineNumber, _column);
                    }
                    break;
                case '|':
                    if (Peek() == '|')
                    {
                        Advance();
                        Advance();
                        return new Token(TokenType.Or, null, _lineNumber, _column);
                    }
                    break;

            }
            return null;
        }

        private Token GetId()
        {
            var result = string.Empty;

            while ((char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_') && CurrentChar != char.MaxValue)
            {
                result += CurrentChar;
                Advance();
            }

            if (ReserverKeywords.ContainsKey(result))
            {
                return ReserverKeywords[result].Copy(_lineNumber, _column);
            }

            return new Token(TokenType.Id, result, _lineNumber, _column);
        }

        private Token GetNumber()
        {
            var result = string.Empty;

            while ((char.IsDigit(CurrentChar) || CurrentChar == '.') && CurrentChar != char.MaxValue)
            {
                result += CurrentChar;
                Advance();
            }

            return new Token(TokenType.ConstNumber, result, _lineNumber, _column);
        }
    }
}
