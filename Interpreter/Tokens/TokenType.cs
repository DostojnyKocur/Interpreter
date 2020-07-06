using System.Collections.Generic;

namespace Interpreter.Tokens
{
    public enum TokenType
    {
        TypeNumber,
        TypeVoid,
        ConstNumber,

        Plus,
        Minus,
        Multiplication,
        Divide,
        Modulo,

        Id,
        Return,

        ScopeBegin,
        ScopeEnd,
        LeftParen,
        RightParen,
        Assign,
        Semicolon,
        Comma,

        EOF
    }

    public static class TokenTypes
    {
        public static readonly Dictionary<char, TokenType> For = new Dictionary<char, TokenType>
        {
            { '+', TokenType.Plus },
            { '-', TokenType.Minus },
            { '*', TokenType.Multiplication },
            { '%', TokenType.Modulo },
            { '(', TokenType.LeftParen },
            { ')', TokenType.RightParen },
            { '{', TokenType.ScopeBegin },
            { '}', TokenType.ScopeEnd },
            { '=', TokenType.Assign },
            { ';', TokenType.Semicolon },
            { ',', TokenType.Comma }
        };

        public static TokenType? GetForChar(char @char)
        {
            if (For.ContainsKey(@char))
            {
                return For[@char];
            }
            else
            {
                return null;
            }
        }
    }
}
