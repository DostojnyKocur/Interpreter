using System;
using System.Collections.Generic;
using System.Text;
using Interpreter.Common.Errors;
using Interpreter.Common.Tokens;

namespace Interpreter.LexerService
{
    public class Lexer : ILexer
    {
        private static readonly Dictionary<string, Token> ReserverKeywords = new Dictionary<string, Token>
        {
            { "return",  new Token(TokenType.Return, "return") },
            { "break",  new Token(TokenType.Break, "break") },
            { "continue",  new Token(TokenType.Continue, "continue") },
            { "void",  new Token(TokenType.TypeVoid, "void") },
            { "number",  new Token(TokenType.TypeNumber, "number") },
            { "bool",  new Token(TokenType.TypeBool, "bool") },
            { "string",  new Token(TokenType.TypeString, "string") },
            { "true",  new Token(TokenType.ConstBool, "true") },
            { "false",  new Token(TokenType.ConstBool, "false") },
            { "if",  new Token(TokenType.If, "if") },
            { "elif",  new Token(TokenType.Elif, "elif") },
            { "else",  new Token(TokenType.Else, "else") },
            { "while",  new Token(TokenType.While, "while") },
            { "for",  new Token(TokenType.For, "for") }
        };

        private string _text = string.Empty;
        private int _position = 0;
        private uint _lineNumber = 1;
        private uint _column = 1;

        public Lexer(string text) => (_text, CurrentChar) = (text, text[_position]);

        public char CurrentChar { get; private set; } = char.MinValue;
        private char _nextChar => _position + 1 > _text.Length - 1 ? char.MaxValue : _text[_position + 1];
        private bool _currentCharIsValid => CurrentChar != char.MaxValue;

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
                    return GetIdentifier();
                }

                if (CurrentChar == '\"')
                {
                    return GetString();
                }

                if (CurrentChar == '/')
                {
                    switch (_nextChar)
                    {
                        case '*':
                            SkipComment();
                            continue;
                        case '/':
                            SkipSingleLineComment();
                            continue;
                    }
                }

                var token = GetToken();
                if (token is null)
                {
                    var message = $"Lexer error on '{CurrentChar}' line: {_lineNumber} column: {_column}";
                    throw new LexerError(message: message);
                }
                return token;

            }

            return new Token(TokenType.EOF, null, _lineNumber, _column);
        }

        private void Advance(uint howMany = 1)
        {
            for (var i = 0; i < howMany; ++i)
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
                    if (CurrentChar == '\t')
                    {
                        _column += 4;
                    }
                    else
                    {
                        _column += 1;
                    }

                    CurrentChar = _text[_position];
                }
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
            while ((CurrentChar != '*') || (_nextChar != '/'))
            {
                Advance();
            }
            Advance(2);  // '*' '/'
        }

        private void SkipSingleLineComment()
        {
            while (CurrentChar != '\n')
            {
                Advance();
            }
            Advance();
        }

        private Token GetToken()
        {
            var lineNumber = _lineNumber;
            var column = _column;

            switch (CurrentChar)
            {
                case ';':
                    Advance();
                    return new Token(TokenType.Semicolon, null, lineNumber, column);
                case ':':
                    Advance();
                    return new Token(TokenType.Colon, null, lineNumber, column);
                case ',':
                    Advance();
                    return new Token(TokenType.Comma, null, lineNumber, column);
                case '+':
                    Advance();
                    return new Token(TokenType.Plus, null, lineNumber, column);
                case '-':
                    Advance();
                    return new Token(TokenType.Minus, null, lineNumber, column);
                case '*':
                    Advance();
                    return new Token(TokenType.Multiplication, null, lineNumber, column);
                case '/':
                    Advance();
                    return new Token(TokenType.Divide, null, lineNumber, column);
                case '%':
                    Advance();
                    return new Token(TokenType.Modulo, null, lineNumber, column);
                case '(':
                    Advance();
                    return new Token(TokenType.LeftParen, null, lineNumber, column);
                case ')':
                    Advance();
                    return new Token(TokenType.RightParen, null, lineNumber, column);
                case '{':
                    Advance();
                    return new Token(TokenType.ScopeBegin, null, lineNumber, column);
                case '}':
                    Advance();
                    return new Token(TokenType.ScopeEnd, null, lineNumber, column);
                case '[':
                    Advance();
                    return new Token(TokenType.LeftBracket, null, lineNumber, column);
                case ']':
                    Advance();
                    return new Token(TokenType.RigthBracket, null, lineNumber, column);
                case '=':
                    if (_nextChar == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.Equal, null, lineNumber, column);
                    }
                    else
                    {
                        Advance();
                        return new Token(TokenType.Assign, null, lineNumber, column);
                    }
                case '>':
                    if (_nextChar == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.GreaterEqual, null, lineNumber, column);
                    }
                    Advance();
                    return new Token(TokenType.Greater, null, lineNumber, column);
                case '<':
                    if (_nextChar == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.LessEqual, null, lineNumber, column);
                    }
                    Advance();
                    return new Token(TokenType.Less, null, lineNumber, column);
                case '!':
                    if (_nextChar == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.NotEqual, null, lineNumber, column);
                    }
                    Advance();
                    return new Token(TokenType.Not, null, lineNumber, column);
                case '&':
                    if (_nextChar == '&')
                    {
                        Advance(2);
                        return new Token(TokenType.And, null, lineNumber, column);
                    }
                    break;
                case '|':
                    if (_nextChar == '|')
                    {
                        Advance(2);
                        return new Token(TokenType.Or, null, lineNumber, column);
                    }
                    break;

            }
            return null;
        }

        private Token GetIdentifier()
        {
            var lineNumber = _lineNumber;
            var column = _column;

            var result = GetLexeme(() => char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_');

            if (ReserverKeywords.ContainsKey(result))
            {
                return ReserverKeywords[result].Copy(lineNumber, column);
            }

            return new Token(TokenType.Identifier, result, lineNumber, column);
        }

        private Token GetNumber()
        {
            var lineNumber = _lineNumber;
            var column = _column;

            var result = GetLexeme(() => char.IsLetterOrDigit(CurrentChar) || CurrentChar == '.');

            return new Token(TokenType.ConstNumber, result, lineNumber, column);
        }

        private Token GetString()
        {
            Advance(); // Beggining "

            var lineNumber = _lineNumber;
            var column = _column;

            var value = GetLexeme(() => char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_');

            Advance(); // finishing "

            return new Token(TokenType.ConstString, value, lineNumber, column);
        }

        private string GetLexeme(Func<bool> predicate)
        {
            var stringBuilder = new StringBuilder();

            while (predicate() && _currentCharIsValid)
            {
                stringBuilder.Append(CurrentChar);
                Advance();
            }

            return stringBuilder.ToString();
        }
    }
}
