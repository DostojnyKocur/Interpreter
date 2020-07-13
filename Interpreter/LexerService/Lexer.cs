using System;
using System.Collections.Generic;
using System.Text;
using Interpreter.Errors;
using Interpreter.LexerService.Tokens;

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
            { "else",  new Token(TokenType.Else, "else") },
            { "while",  new Token(TokenType.While, "while") }
        };

        private string _text = string.Empty;
        private int _position = 0;
        private uint _lineNumber = 1;
        private uint _column = 1;

        public Lexer(string text) => (_text, CurrentChar) = (text, text[_position]);

        public char CurrentChar { get; private set; } = char.MinValue;
        private char NextChar => _position + 1 > _text.Length - 1 ? char.MaxValue : _text[_position + 1];
        private bool CurrentCharValid => CurrentChar != char.MaxValue;

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

                if(CurrentChar == '\"')
                {
                    return GetString();
                }

                if (CurrentChar == '/')
                {
                    if (NextChar == '*')
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
                    CurrentChar = _text[_position];
                    _column += 1;
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
            while ((CurrentChar != '*') || (NextChar != '/'))
            {
                Advance();
            }

            Advance(2);  // '*' '/'
        }

        private Token GetCompareOperator()
        {
            switch (CurrentChar)
            {
                case '=':
                    if (NextChar == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.Equal, null, _lineNumber, _column);
                    }
                    break;
                case '>':
                    if (NextChar == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.GreaterEqual, null, _lineNumber, _column);
                    }
                    Advance();
                    return new Token(TokenType.Greater, null, _lineNumber, _column);
                case '<':
                    if (NextChar == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.LessEqual, null, _lineNumber, _column);
                    }
                    Advance();
                    return new Token(TokenType.Less, null, _lineNumber, _column);
                case '!':
                    if (NextChar == '=')
                    {
                        Advance(2);
                        return new Token(TokenType.NotEqual, null, _lineNumber, _column);
                    }
                    Advance();
                    return new Token(TokenType.Not, null, _lineNumber, _column);
                case '&':
                    if (NextChar == '&')
                    {
                        Advance(2);
                        return new Token(TokenType.And, null, _lineNumber, _column);
                    }
                    break;
                case '|':
                    if (NextChar == '|')
                    {
                        Advance(2);
                        return new Token(TokenType.Or, null, _lineNumber, _column);
                    }
                    break;

            }
            return null;
        }

        private Token GetId()
        {
            var result = GetLexeme(() => char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_');

            if (ReserverKeywords.ContainsKey(result))
            {
                return ReserverKeywords[result].Copy(_lineNumber, _column);
            }

            return new Token(TokenType.Id, result, _lineNumber, _column);
        }

        private Token GetNumber()
        {
            var result = GetLexeme(() => char.IsLetterOrDigit(CurrentChar) || CurrentChar == '.');

            return new Token(TokenType.ConstNumber, result, _lineNumber, _column);
        }

        private Token GetString()
        {
            Advance(); // Beggining "

            var value = GetLexeme(() => char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_');

            Advance(); // finishing "

            return new Token(TokenType.ConstString, value, _lineNumber, _column);
        }

        private string GetLexeme(Func<bool> predicate)
        {
            var stringBuilder = new StringBuilder();

            while (predicate() && CurrentCharValid)
            {
                stringBuilder.Append(CurrentChar);
                Advance();
            }

            return stringBuilder.ToString();
        }
    }
}
